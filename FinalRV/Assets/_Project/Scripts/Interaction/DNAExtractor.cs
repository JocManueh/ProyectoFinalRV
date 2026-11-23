/*
 * DESCRIPCIÓN: Extractor de ADN en 4 pasos con sockets en orden:
 * 1-Araña en tubo, 2-Detergente, 3-Sal, 4-Alcohol frío + varilla agarrable.
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

    [Header("Varilla agarrable")]
    [SerializeField] XRGrabInteractable varilla;

    [Header("Resultado")]
    [SerializeField] ChemicalSO adnSO;          // Chemical_ADN_Arana
    [SerializeField] GameObject ampollaPrefab;   // prefab Ampolla
    [SerializeField] Transform puntoSpawn;

    [Header("Feedback")]
    [SerializeField] Renderer[] pilotosOrden;   // 4 esferas indicadoras
    [SerializeField] AudioSource audioSource;

    readonly bool[] completados = new bool[4];
    bool varillaUsada;
    bool extraido;

    void OnEnable()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            int idx = i; // captura para lambda
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

        // Verificar orden: todos los anteriores deben estar completos
        for (int i = 0; i < idx; i++)
        {
            if (!completados[i])
            {
                Debug.Log($"[E5] Paso {idx + 1} insertado fuera de orden.");
                if (ScoreSystem.Instance != null)
                    ScoreSystem.Instance.RegisterError(ErrorCode.E5_Orden,
                        $"Extractor paso {idx + 1} antes del {i + 1}");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say($"Orden incorrecto. Primero completa el paso {i + 1}.");
                if (audioSource != null)
                    audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.3f));
                return;
            }
        }

        completados[idx] = true;
        PintarPiloto(idx, Color.green);
        Debug.Log($"[DNAExtractor] Paso {idx + 1}/4 completado.");
        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(660f + idx * 110f, 0.1f));
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();

        VerificarCompleto();
    }

    void AlUsarVarilla(SelectEnterEventArgs args)
    {
        if (varillaUsada || extraido) return;
        // La varilla solo cuenta si los 4 sockets están completos
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
            var cont = go.GetComponent<ChemicalContainer>();
            if (cont != null)
            {
                cont.sustancia = adnSO;
                cont.volumen   = 10;
                cont.Refrescar();
            }
            Debug.Log("[DNAExtractor] ✅ Ampolla ADN de araña instanciada.");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("ADN de araña extraído correctamente. Recoge la ampolla.");
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Pulse(0.7f, 0.25f);
        }
        else
        {
            Debug.LogWarning("[DNAExtractor] Falta ampollaPrefab o puntoSpawn.");
        }
    }

    void PintarPiloto(int idx, Color c)
    {
        if (pilotosOrden != null && idx < pilotosOrden.Length && pilotosOrden[idx] != null)
            pilotosOrden[idx].material.color = c;
    }
}
