using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CamaraBlindada : MonoBehaviour
{
    [SerializeField] Transform tapa;
    [SerializeField] Vector3 desplazamientoAbierta = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float segundos = 0.6f;
    [SerializeField] XRGrabInteractable ampolla;   // la ampolla de tritio
    [SerializeField] AudioSource audioSource;
    public UnityEvent OnAbierta;

    public bool Abierta { get; private set; }
    Vector3 posCerrada;

    void Awake()
    {
        if (tapa != null) posCerrada = tapa.localPosition;
        if (ampolla != null) ampolla.enabled = false; // bloqueada hasta abrir
    }

    // Conectar al OnPressed del BotonLab
    public void AbrirTapa()
    {
        if (Abierta) return;
        Abierta = true;
        if (audioSource != null) audioSource.PlayOneShot(SimpleTones.Beep(300f, 0.25f));
        StartCoroutine(Mover());
    }

    IEnumerator Mover()
    {
        Vector3 destino = posCerrada + desplazamientoAbierta;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / segundos;
            tapa.localPosition = Vector3.Lerp(posCerrada, destino, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        if (ampolla != null) ampolla.enabled = true;
        OnAbierta?.Invoke();
        if (NOVA.Instance != null)
            NOVA.Instance.Say("Cámara abierta. No toques la fuente: usa las pinzas.");
    }
}