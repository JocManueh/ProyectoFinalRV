/*
 * DESCRIPCIÓN: Palabra clave tocable. Muestra "???" hasta que el jugador la toca;
 * entonces revela el texto real y avisa a TablaHeroes.
 */
using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class PistaRevelable : MonoBehaviour
{
    [SerializeField] TMP_Text etiqueta;
    [SerializeField] string textoOculto = "???";
    [SerializeField] string textoReal;
    [SerializeField] AudioSource audioSource;

    public event Action<PistaRevelable> OnRevelada;
    public bool Revelada { get; private set; }
    public string TextoReal => textoReal;

    XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (etiqueta != null) etiqueta.text = textoOculto;
    }

    void OnEnable()  => interactable.selectEntered.AddListener(AlTocar);
    void OnDisable() => interactable.selectEntered.RemoveListener(AlTocar);

    void AlTocar(SelectEnterEventArgs args)
    {
        if (Revelada) return;
        Revelar();
    }

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

    // Permite asignar el texto desde TablaHeroes en runtime
    public void Configurar(string oculto, string real)
    {
        textoOculto = oculto;
        textoReal   = real;
        Revelada    = false;
        if (etiqueta != null) etiqueta.text = textoOculto;
    }
}
