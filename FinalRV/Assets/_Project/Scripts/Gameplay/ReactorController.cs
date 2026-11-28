/*
 * DESCRIPCIÓN: Mapea el valor de una PalancaLab (0–100 %) a temperatura (20–80 °C).
 * Considera "estable" cuando permanece dentro del rango de la receta durante
 * segundosEstable segundos. Registra E6 si se sale del rango durante la estabilización.
 */
using System;
using UnityEngine;
using TMPro;

public class ReactorController : MonoBehaviour
{
    [Header("Palanca")]
    [SerializeField] PalancaLab palanca;

    [Header("Receta (rango de temperatura)")]
    [SerializeField] RecipeSO receta;

    [Header("Visual")]
    [SerializeField] Renderer termometroRenderer; // cilindro que cambia de color
    [SerializeField] TMP_Text txtTemperatura;

    [Header("Rango físico del reactor")]
    [SerializeField] float tempMin = 20f;
    [SerializeField] float tempMax = 80f;

    public float TemperaturaActual { get; private set; }
    public bool Estable { get; private set; }

    [SerializeField] bool esperarMezcla;
    bool mezclaLista;

    public event Action OnEstable;

    public void HabilitarEstabilizacion() => mezclaLista = true;

    float tiempoEnRango;
    bool yaAviso;

    void Update()
    {
        if (palanca == null) return;

        // Mapear 0–100 % → tempMin–tempMax
        TemperaturaActual = Mathf.Lerp(tempMin, tempMax, palanca.valor / 100f);

        ActualizarVisual();

        if (receta == null || (esperarMezcla && !mezclaLista)) return;

        bool enRango = TemperaturaActual >= receta.temperaturaRango.x &&
                       TemperaturaActual <= receta.temperaturaRango.y;

        if (enRango)
        {
            tiempoEnRango += Time.unscaledDeltaTime;
            yaAviso = false;

            if (!Estable && tiempoEnRango >= receta.segundosEstable)
            {
                Estable = true;
                Debug.Log($"[Reactor] ✅ Temperatura estable: {TemperaturaActual:F1}°C durante {receta.segundosEstable}s");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say($"Temperatura estabilizada a {TemperaturaActual:F1}°C. Reactor listo.");
                if (HapticFeedback.Instance != null) HapticFeedback.Instance.Suave();
                OnEstable?.Invoke();
            }
        }
        else
        {
            tiempoEnRango = 0f;
            Estable = false;

            if (!yaAviso && TemperaturaActual > 0f)
            {
                yaAviso = true;
                Debug.Log($"[E6] Temperatura fuera de rango: {TemperaturaActual:F1}°C " +
                          $"(rango {receta.temperaturaRango.x}–{receta.temperaturaRango.y}°C)");
                if (ScoreSystem.Instance != null)
                    ScoreSystem.Instance.RegisterError(ErrorCode.E6_FueraDeRango,
                        $"Temp {TemperaturaActual:F1}°C");
            }
        }
    }

    void ActualizarVisual()
    {
        if (txtTemperatura != null)
            txtTemperatura.text = $"{TemperaturaActual:F1}°C";

        if (termometroRenderer != null)
        {
            // Frío = azul, ideal = verde, caliente = rojo
            float t = Mathf.InverseLerp(tempMin, tempMax, TemperaturaActual);
            Color c = t < 0.5f
                ? Color.Lerp(Color.blue, Color.green, t * 2f)
                : Color.Lerp(Color.green, Color.red, (t - 0.5f) * 2f);
            termometroRenderer.material.color = c;
        }
    }
}
