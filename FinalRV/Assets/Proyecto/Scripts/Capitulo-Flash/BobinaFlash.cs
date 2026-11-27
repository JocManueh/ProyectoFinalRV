// Inicia la carga al hundir el botón físico y entrega la ampolla
// si el condensador permanece en el socket durante tres segundos.
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BobinaFlash : MonoBehaviour
{
    public XRSocketInteractor socket;
    public XRGrabInteractable condensador;
    public GameObject ampollaCarga;
    public TMP_Text textoCarga;

    public event System.Action CargaInterrumpida;

    float tiempo;
    bool cargando;
    bool entregada;

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