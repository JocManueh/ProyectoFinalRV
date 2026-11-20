/*
 * DESCRIPCIÓN: Simula un botón físico de laboratorio en realidad virtual. Al ser 
 * presionado, anima el hundimiento de su tapa, emite un sonido y vibración suave, 
 * evita dobles clics accidentales y dispara el evento conectado a él.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class BotonLab : MonoBehaviour
{
    public UnityEvent OnPressed;
    [SerializeField] Transform parteMovil; // la tapa que se hunde
    [SerializeField] float hundimiento = 0.01f; // 1 cm
    [SerializeField] float segundosBloqueo = 0.3f; // evita doble pulsación
    [SerializeField] AudioSource audioSource;

    XRSimpleInteractable interactable;
    Vector3 posInicial;
    float ultimaPulsacion = -99f;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (parteMovil == null) parteMovil = transform;
        posInicial = parteMovil.localPosition;
    }

    void OnEnable() => interactable.selectEntered.AddListener(AlSeleccionar);
    void OnDisable() => interactable.selectEntered.RemoveListener(AlSeleccionar);

    void AlSeleccionar(SelectEnterEventArgs args) => Pulsar();

    public void Pulsar()
    {
        if (Time.unscaledTime - ultimaPulsacion < segundosBloqueo) return;
        ultimaPulsacion = Time.unscaledTime;

        Debug.Log($"[BotonLab] Pulsado: {name}");
        StopAllCoroutines();
        StartCoroutine(Animar());

        if (audioSource != null)
            audioSource.PlayOneShot(SimpleTones.Beep(520f, 0.08f));
        if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();

        OnPressed?.Invoke();
    }

    IEnumerator Animar()
    {
        parteMovil.localPosition = posInicial + Vector3.down * hundimiento;
        yield return new WaitForSecondsRealtime(0.15f);
        parteMovil.localPosition = posInicial;
    }
}