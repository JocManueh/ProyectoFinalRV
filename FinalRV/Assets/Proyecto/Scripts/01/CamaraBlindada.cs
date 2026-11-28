using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CamaraBlindada : MonoBehaviour
{
    [Header("Tapa")]
    [SerializeField] Animator tapaAnimator;          // si se asigna, usa la animación
    [SerializeField] string estadoTapa = "cubeee";
    [SerializeField] float segundosAnimacion = 1f;   // duración de la animación de la tapa

    [Header("Fallback sin animación")]
    [SerializeField] Transform tapa;
    [SerializeField] Vector3 desplazamientoAbierta = new Vector3(0f, 0.25f, 0f);
    [SerializeField] float segundos = 0.6f;

    [Header("Tritios")]
    [SerializeField] Animator[] tritiosQueSuben;     // Tritio2, Tritio3, Tritio5
    [SerializeField] string estadoSube = "Sube tritio";

    [Header("Otros")]
    [SerializeField] XRGrabInteractable ampolla;     // bloqueada hasta abrir
    [SerializeField] AudioSource audioSource;
    public UnityEvent OnAbierta;

    public bool Abierta { get; private set; }
    Vector3 posCerrada;

    void Awake()
    {
        if (tapa != null) posCerrada = tapa.localPosition;
        if (ampolla != null) ampolla.enabled = false;
    }

    // Conectar al OnPressed del BotonLab
    public void AbrirTapa()
    {
        if (Abierta) return;
        Abierta = true;
        if (audioSource != null) audioSource.PlayOneShot(SimpleTones.Beep(300f, 0.25f));
        StartCoroutine(Abrir());
    }

    IEnumerator Abrir()
    {
        if (tapaAnimator != null)
        {
            tapaAnimator.Play(estadoTapa);
            yield return new WaitForSecondsRealtime(segundosAnimacion);
        }
        else if (tapa != null)
        {
            Vector3 destino = posCerrada + desplazamientoAbierta;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / segundos;
                tapa.localPosition = Vector3.Lerp(posCerrada, destino, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
        }

        foreach (var a in tritiosQueSuben)
            if (a != null) a.Play(estadoSube);

        if (ampolla != null) ampolla.enabled = true;
        OnAbierta?.Invoke();
        NOVA.Instance?.Say("Cámara abierta. No toques la fuente: usa las pinzas.");
    }
}