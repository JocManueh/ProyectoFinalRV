using TMPro;
using UnityEngine;

/// <summary>Muestra el checklist de Flash cuando la cara del reloj apunta hacia la cámara XR.</summary>
public class FlashRelojMuneca : MonoBehaviour
{
    [SerializeField] FlashNivel nivel;
    [SerializeField] Transform camaraXR;
    [SerializeField] Transform normalPantalla;
    [SerializeField] GameObject visual;
    [SerializeField] TMP_Text texto;
    [SerializeField, Range(0f, 1f)] float umbralMostrar = 0.70f;
    [SerializeField, Range(0f, 1f)] float umbralOcultar = 0.55f;
    [SerializeField, Min(0.1f)] float distanciaMaxima = 0.85f;

    void LateUpdate()
    {
        if (nivel == null || camaraXR == null || normalPantalla == null || visual == null || texto == null) return;

        Vector3 haciaCamara = camaraXR.position - normalPantalla.position;
        float umbral = visual.activeSelf ? Mathf.Min(umbralMostrar, umbralOcultar) : umbralMostrar;
        bool visible = haciaCamara.sqrMagnitude <= distanciaMaxima * distanciaMaxima &&
            Vector3.Dot(normalPantalla.forward, haciaCamara.normalized) >= umbral;

        if (visual.activeSelf != visible) visual.SetActive(visible);
        if (!visible) return;

        string contenido = nivel.TextoReloj();
        if (texto.text != contenido) texto.text = contenido;
    }
}
