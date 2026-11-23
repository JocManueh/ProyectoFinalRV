/*
 * DESCRIPCIÓN: Cámara blindada con tapa. Contiene la ampolla de tritio.
 * Si el jugador agarra la ampolla directamente con la mano (sin pinzas),
 * NOVA avisa y se registra E7 (−25 pts). Las pinzas son dos XRGrabInteractable
 * marcados con el tag "Pinzas".
 */
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TritiumChamber : MonoBehaviour
{
    [Header("Ampolla de tritio")]
    [SerializeField] XRGrabInteractable ampollaTritiio;

    [Header("Tapa de la cámara")]
    [SerializeField] Transform tapa;
    [SerializeField] float anguloAbierto = -90f;

    [Header("Feedback")]
    [SerializeField] Renderer piloto;
    [SerializeField] AudioSource audioSource;

    bool tapaAbierta;
    float ultimoAviso = -99f;
    const float intervaloAviso = 3f;

    void OnEnable()
    {
        if (ampollaTritiio != null)
            ampollaTritiio.selectEntered.AddListener(AlAgarrarAmpolla);
    }

    void OnDisable()
    {
        if (ampollaTritiio != null)
            ampollaTritiio.selectEntered.RemoveListener(AlAgarrarAmpolla);
    }

    void AlAgarrarAmpolla(SelectEnterEventArgs args)
    {
        // Comprueba si el interactor pertenece a las pinzas (tag "Pinzas")
        bool esPinzas = args.interactorObject.transform.CompareTag("Pinzas");

        if (!esPinzas)
        {
            if (Time.unscaledTime - ultimoAviso < intervaloAviso) return;
            ultimoAviso = Time.unscaledTime;

            Debug.Log("[E7] Tritio agarrado con la mano directamente.");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E7_Radiacion,
                    "Tritio agarrado sin pinzas");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("¡Peligro! Usa las pinzas para manipular el tritio. Radiación detectada.");
            if (audioSource != null)
                audioSource.PlayOneShot(SimpleTones.Beep(220f, 0.4f));
            if (HapticFeedback.Instance != null)
                HapticFeedback.Instance.Error();
            if (piloto != null)
                StartCoroutine(ParpadeoRojo());
        }
        else
        {
            Debug.Log("[TritiumChamber] Tritio agarrado correctamente con pinzas.");
            if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
        }
    }

    // Llamar desde un BotonLab o trigger para abrir la tapa
    public void AbrirTapa()
    {
        if (tapaAbierta || tapa == null) return;
        tapaAbierta = true;
        tapa.localRotation = Quaternion.Euler(anguloAbierto, 0f, 0f);
        Debug.Log("[TritiumChamber] Tapa abierta.");
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Cámara blindada abierta. Usa las pinzas para extraer el tritio.");
    }

    IEnumerator ParpadeoRojo()
    {
        if (piloto != null) piloto.material.color = Color.red;
        yield return new WaitForSecondsRealtime(0.6f);
        if (piloto != null) piloto.material.color = Color.white;
    }
}
