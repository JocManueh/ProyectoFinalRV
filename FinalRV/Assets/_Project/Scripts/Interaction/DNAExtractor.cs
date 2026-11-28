/*
 * DESCRIPCIÓN: Extractor de ADN en 4 pasos con sockets en orden:
 * 1-Araña en tubo, 2-Detergente, 3-Sal, 4-Alcohol frío + varilla agarrable.
 * Cada socket valida que el objeto insertado tenga el ChemicalSO correcto.
 * Al completar los 4 pasos instancia una ampolla "ADN de araña" con volumen 10.
 * Si se intenta usar la araña sin procesar en el matraz → E2.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DNAExtractor : MonoBehaviour
{
    [Header("Sockets en orden (0=araña, 1=detergente, 2=sal, 3=alcohol)")]
    [SerializeField] XRSocketInteractor[] sockets = new XRSocketInteractor[4];

    [Header("Sustancia esperada por cada socket (mismo orden que sockets[])")]
    [Tooltip("Dejar null en sockets[0] si la araña se identifica por requiresProcessing")]
    [SerializeField] ChemicalSO[] sustanciasEsperadas = new ChemicalSO[4];

    [Header("Varilla agarrable")]
    [SerializeField] XRGrabInteractable varilla;

    [Header("Resultado")]
    [SerializeField] ChemicalSO adnSO;
    [SerializeField] GameObject ampollaPrefab;
    [SerializeField] Transform puntoSpawn;
    [SerializeField] float escalaSpawn = 0.0797f;

    [Header("Feedback")]
    [SerializeField] Renderer[] pilotosOrden;
    [SerializeField] AudioSource audioSource;

    public event System.Action OnADNExtraido;

    readonly bool[] completados = new bool[4];
    bool varillaUsada;
    bool extraido;

    static readonly string[] nombresStep =
        { "araña", "detergente", "sal", "alcohol frío" };

    void OnEnable()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            int idx = i;
            if (sockets[idx] != null)
                sockets[idx].selectEntered.AddListener(args => AlInsertar(idx, args));
        }
        if (varilla != null)
            varilla.selectEntered.AddListener(AlUsarVarilla);
    }

    void OnDisable()
    {
        for (int i = 0; i < sockets.Length; i++)
            if (sockets[i] != null) sockets[i].selectEntered.RemoveAllListeners();
        if (varilla != null)
            varilla.selectEntered.RemoveListener(AlUsarVarilla);
    }

    void AlInsertar(int idx, SelectEnterEventArgs args)
    {
        if (extraido) return;

        // ── Validar sustancia esperada ────────────────────────────────────────
        ChemicalSO esperada = (sustanciasEsperadas != null && idx < sustanciasEsperadas.Length)
            ? sustanciasEsperadas[idx] : null;

        if (esperada != null)
        {
            // Buscar ChemicalContainer en el objeto o en sus hijos
            var cont = args.interactableObject.transform
                           .GetComponentInChildren<ChemicalContainer>();

            if (cont == null || cont.sustancia != esperada)
            {
                string recibido = cont?.sustancia?.displayName ?? "desconocido";
                Debug.Log($"[E3] Socket {idx + 1} ({nombresStep[idx]}): " +
                          $"se esperaba '{esperada.displayName}', llegó '{recibido}'.");

                if (ScoreSystem.Instance != null)
                    ScoreSystem.Instance.RegisterError(ErrorCode.E3_Incorrecto,
                        $"Extractor paso {idx + 1}: esperado {esperada.displayName}, recibido {recibido}");

                if (NOVA.Instance != null)
                    NOVA.Instance.Say($"Reactivo incorrecto. En este paso necesitas {esperada.displayName}.");

                if (audioSource != null)
                    audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));

                if (HapticFeedback.Instance != null)
                    HapticFeedback.Instance.Error();

                // Expulsar el objeto del socket tras un frame
                StartCoroutine(ExpulsarSocket(sockets[idx]));
                return;
            }
        }
        else if (idx == 0)
        {
            // Socket 0 (araña): validar por requiresProcessing si no hay SO asignado
            var cont = args.interactableObject.transform
                           .GetComponentInChildren<ChemicalContainer>();
            if (cont == null || (cont.sustancia != null && !cont.sustancia.requiresProcessing))
            {
                Debug.Log("[E3] Socket araña: el objeto insertado no es la araña.");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say("Coloca la araña en el primer tubo.");
                StartCoroutine(ExpulsarSocket(sockets[idx]));
                return;
            }
        }

        // ── Validar orden ─────────────────────────────────────────────────────
        for (int i = 0; i < idx; i++)
        {
            if (!completados[i])
            {
                Debug.Log($"[E5] Paso {idx + 1} insertado fuera de orden.");
                if (ScoreSystem.Instance != null)
                    ScoreSystem.Instance.RegisterError(ErrorCode.E5_Orden,
                        $"Extractor paso {idx + 1} antes del {i + 1}");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say($"Orden incorrecto. Primero completa el paso {i + 1}: {nombresStep[i]}.");
                if (audioSource != null)
                    audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));
                StartCoroutine(ExpulsarSocket(sockets[idx]));
                return;
            }
        }

        // ── Paso correcto ─────────────────────────────────────────────────────
        completados[idx] = true;
        PintarPiloto(idx, Color.green);
        Debug.Log($"[DNAExtractor] Paso {idx + 1}/4 completado: {nombresStep[idx]}");

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(660f + idx * 110f, 0.1f));
        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Suave();

        VerificarCompleto();
    }

    void AlUsarVarilla(SelectEnterEventArgs args)
    {
        if (varillaUsada || extraido) return;

        bool todosListos = true;
        foreach (var c in completados) if (!c) { todosListos = false; break; }

        if (!todosListos)
        {
            if (NOVA.Instance != null)
                NOVA.Instance.Say("Completa los 4 pasos antes de usar la varilla.");
            return;
        }

        varillaUsada = true;
        Debug.Log("[DNAExtractor] Varilla usada → extracción completa.");
        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(880f, 0.2f));
        StartCoroutine(InstanciarADN());
    }

    void VerificarCompleto()
    {
        foreach (var c in completados) if (!c) return;
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Todos los reactivos insertados. Ahora usa la varilla para mezclar.");
    }

    IEnumerator InstanciarADN()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        extraido = true;

        if (ampollaPrefab != null && puntoSpawn != null)
        {
            var go = Instantiate(ampollaPrefab, puntoSpawn.position, puntoSpawn.rotation);
            go.transform.localScale = Vector3.one * escalaSpawn;
            var cont = go.GetComponent<ChemicalContainer>();
            if (cont != null)
            {
                cont.sustancia = adnSO;
                cont.volumen   = 10;
                cont.Refrescar();
                OnADNExtraido?.Invoke();
            }
            Debug.Log("[DNAExtractor] ✅ Ampolla ADN de araña instanciada.");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("ADN de araña extraído correctamente. Recoge la ampolla.");
            if (HapticFeedback.Instance != null)
                HapticFeedback.Instance.Pulse(0.7f, 0.25f);
        }
        else
        {
            Debug.LogWarning("[DNAExtractor] Falta ampollaPrefab o puntoSpawn.");
        }
    }

    // Espera un frame y libera el socket para que el objeto salga
    IEnumerator ExpulsarSocket(XRSocketInteractor socket)
    {
        yield return null;
        if (socket != null && socket.hasSelection)
            socket.interactionManager?.SelectExit(socket,
                socket.firstInteractableSelected);
    }

    void PintarPiloto(int idx, Color c)
    {
        if (pilotosOrden != null && idx < pilotosOrden.Length && pilotosOrden[idx] != null)
            pilotosOrden[idx].material.color = c;
    }
}
