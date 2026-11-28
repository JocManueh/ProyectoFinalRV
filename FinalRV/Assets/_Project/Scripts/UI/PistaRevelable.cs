/*
 * DESCRIPCIÓN: Palabra clave tocable en un Canvas world-space.
 * Tiene su propio TMP_Text que muestra "???" hasta que el jugador
 * la toca con el rayo o el dedo; entonces revela el texto real.
 *
 * Requiere: XRSimpleInteractable + BoxCollider (en world-space)
 * El BoxCollider se añade automáticamente en Awake si no existe.
 */
using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PistaRevelable : MonoBehaviour
{
    [Header("Texto")]
    [SerializeField] TMP_Text etiqueta;          // TMP_Text propio de esta pista
    [SerializeField] string textoOculto = "???";
    [SerializeField] string textoReal;

    [Header("Feedback")]
    [SerializeField] AudioSource audioSource;

    public event Action<PistaRevelable> OnRevelada;
    public bool   Revelada   { get; private set; }
    public string TextoReal  => textoReal;

    XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        // XRI necesita un Collider para detectar el rayo/dedo en world-space.
        // Si no hay ninguno, añadimos un BoxCollider que cubra el RectTransform.
        if (GetComponent<Collider>() == null)
        {
            var rect = GetComponent<RectTransform>();
            var box  = gameObject.AddComponent<BoxCollider>();
            if (rect != null)
            {
                // sizeDelta está en canvas-pixels; la escala del canvas (0.003) lo convierte a metros
                float scaleCanvas = transform.lossyScale.x; // ~0.003
                box.size   = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, 10f);
                box.center = Vector3.zero;
            }
            else
            {
                box.size = new Vector3(0.4f, 0.06f, 0.02f);
            }
        }

        // Mostrar texto oculto al inicio
        if (etiqueta != null) etiqueta.text = textoOculto;
    }

    void OnEnable()  => interactable.selectEntered.AddListener(AlTocar);
    void OnDisable() => interactable.selectEntered.RemoveListener(AlTocar);

    void AlTocar(SelectEnterEventArgs args) { if (!Revelada) Revelar(); }

    public void Revelar()
    {
        if (Revelada) return;
        Revelada = true;

        if (etiqueta != null) etiqueta.text = textoReal;

        Debug.Log($"[Pista] Revelada: {textoReal}");

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(660f, 0.12f));
        if (HapticFeedback.Instance != null)
            HapticFeedback.Instance.Suave();
        if (NOVA.Instance != null)
            NOVA.Instance.Say($"Pista desbloqueada: {textoReal}");

        OnRevelada?.Invoke(this);
    }

    /// <summary>Configura el texto desde TablaHeroes antes de que empiece el juego.</summary>
    public void Configurar(string oculto, string real)
    {
        textoOculto = oculto;
        textoReal   = real;
        Revelada    = false;
        if (etiqueta != null) etiqueta.text = textoOculto;
    }
}
