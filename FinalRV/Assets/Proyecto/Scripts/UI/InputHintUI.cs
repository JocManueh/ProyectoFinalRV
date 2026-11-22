using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class InputHintUI : MonoBehaviour
{
    [System.Serializable]
    public class Pista
    {
        [TextArea] public string gafas;
        [TextArea] public string simulador;
            [SerializeField] HologramaControles holograma;
    }

    [SerializeField] TMP_Text texto;
    [SerializeField] Pista[] pistas;
    [SerializeField] HologramaControles holograma;

    public static bool HayGafas => XRSettings.isDeviceActive;

    int actual = -1;
    bool ultimoHayGafas;

    void Reset() { pistas = PistasPorDefecto(); }

    void Awake()
    {
        if (pistas == null || pistas.Length == 0) pistas = PistasPorDefecto();
    }

    void Update()
    {
        if (actual < 0) return;
        if (HayGafas != ultimoHayGafas) Refrescar();
    }

    public void Mostrar(int indice)
    {
        actual = indice;
                if (holograma != null) holograma.Apagar();
        Refrescar();
    }

    public void Ocultar()
    {
        actual = -1;
                if (holograma != null) holograma.Apagar();
        if (texto != null) texto.text = "";
    }

    void Refrescar()
    {
        ultimoHayGafas = HayGafas;
        if (texto == null || pistas == null || actual < 0 || actual >= pistas.Length) return;
        texto.text = ultimoHayGafas ? pistas[actual].gafas : pistas[actual].simulador;
    }

    static Pista[] PistasPorDefecto()
    {
        return new[]
        {
            new Pista {
                gafas = "Camina con el stick izquierdo o teletranspórtate. Gira con el stick derecho.",
                simulador = "Camina con WASD y mira con el mouse hasta la marca verde. Gira la vista." },
            new Pista {
                gafas = "Agarra la probeta con el GRIP (botón lateral del mando).",
                simulador = "Apunta a la probeta y mantén la tecla G." },
            new Pista {
                gafas = "Acerca la probeta al matraz y suelta el GRIP.",
                simulador = "Acerca la probeta al matraz y suelta la tecla G." },
            new Pista {
                gafas = "Presiona el botón con el GATILLO (rayo) o tócalo con el dedo.",
                simulador = "Apunta al botón y haz clic." },
            new Pista {
                gafas = "Agarra la bola de la palanca con el GRIP y llévala a la zona verde 2 s.",
                simulador = "Mantén G sobre la palanca y llévala a la zona verde 2 s." }
        };
    }
}