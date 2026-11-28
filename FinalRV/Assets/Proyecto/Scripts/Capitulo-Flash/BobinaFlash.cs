using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Carga la ampolla y controla los efectos de Tesla y de la ampolla.</summary>
public class BobinaFlash : MonoBehaviour
{
    public XRSocketInteractor socket;
    public XRGrabInteractable condensador;
    public GameObject ampollaCarga;
    public TMP_Text textoCarga;

    [Header("Efectos de Flash")]
    [SerializeField] GameObject efectoVisualBobina;
    [SerializeField] GameObject efectoSonidoBobina;
    [SerializeField] GameObject efectoVisualAmpolla;

    public event System.Action CargaInterrumpida;

    float tiempo;
    bool cargando;
    bool entregada;
    GameObject efectoCargaEnBobina;

    void Awake()
    {
        if (efectoVisualAmpolla != null) efectoVisualAmpolla.SetActive(false);
    }

    public void MostrarEfectosDePaso(bool activo)
    {
        if (efectoVisualBobina != null)
        {
            efectoVisualBobina.SetActive(activo);
            if (activo)
            {
                foreach (var particulas in efectoVisualBobina.GetComponentsInChildren<ParticleSystem>())
                    if (!particulas.isPlaying) particulas.Play(false);
            }
        }

        // La ampolla aún no se entrega durante el paso; su padre está inactivo.
        if (activo && efectoCargaEnBobina == null && efectoVisualAmpolla != null)
        {
            efectoCargaEnBobina = Instantiate(efectoVisualAmpolla, transform, true);
            efectoCargaEnBobina.name = "Efecto de carga durante Tesla";
        }
        if (efectoCargaEnBobina != null) efectoCargaEnBobina.SetActive(activo);
        if (efectoSonidoBobina != null) efectoSonidoBobina.SetActive(activo);
    }

    public void ApagarEfectosAmpolla()
    {
        if (efectoVisualAmpolla != null) efectoVisualAmpolla.SetActive(false);
    }

    void OnDisable() => MostrarEfectosDePaso(false);

    void Start()
    {
        ampollaCarga.SetActive(false);
        textoCarga.text = "Carga 0%";
    }

    public void IniciarCarga()
    {
        if (entregada || cargando) return;

        if (!socket.IsSelecting(condensador))
        {
            textoCarga.text = "Coloca el condensador primero.";
            return;
        }

        tiempo = 0f;
        cargando = true;
        textoCarga.text = "Carga 0%";
    }

    void Update()
    {
        if (!cargando) return;

        if (!socket.IsSelecting(condensador))
        {
            cargando = false;
            textoCarga.text = "Carga 0%";
            CargaInterrumpida?.Invoke();
            return;
        }

        tiempo += Time.unscaledDeltaTime;
        textoCarga.text = $"Carga {Mathf.Clamp01(tiempo / 3f):P0}";

        if (tiempo < 3f) return;

        cargando = false;
        entregada = true;
        ampollaCarga.SetActive(true);
    }
}