/*
 * DESCRIPCIÓN: Valida el contenido del MixingFlask contra la RecipeSO del capítulo.
 * Resultado: Correcto / Inestable / Fallido. Actualiza piloto y emite beep.
 * Se llama desde el BotonLab "ANALIZAR".
 */
using System;
using UnityEngine;
using TMPro;

public enum ResultadoValidacion { Correcto, Inestable, Fallido }

public class RecipeValidator : MonoBehaviour
{
    [SerializeField] MixingFlask matraz;
    [SerializeField] ReactorController reactor;
    [SerializeField] RecipeSO receta;
    [SerializeField] Renderer pilotoResultado;
    [SerializeField] TMP_Text txtResultado;
    [SerializeField] AudioSource audioSource;

    public event Action<ResultadoValidacion> OnValidado;
    public ResultadoValidacion UltimoResultado { get; private set; }

    public void Validate()
    {
        if (receta == null)
        {
            Debug.LogWarning("[RecipeValidator] Sin receta asignada.");
            return;
        }

        // El matraz rechaza al insertar; aquí solo comprobamos que se haya llenado por completo.
        if (matraz != null && receta.steps.Count > 0 && !matraz.Completa)
        {
            Debug.Log("[E4] Faltan ampollas al confirmar la mezcla");
            if (ScoreSystem.Instance != null)
                ScoreSystem.Instance.RegisterError(ErrorCode.E4_Cantidad, "Faltan ampollas");
            if (NOVA.Instance != null)
                NOVA.Instance.Say("Faltan ampollas en el matraz.");
            return; // no continuar con la validación
        }

        var resultado = EvalRecipe();
        UltimoResultado = resultado;

        string msg;
        Color color;
        float freq;

        switch (resultado)
        {
            case ResultadoValidacion.Correcto:
                msg   = "✅ CORRECTO";
                color = Color.green;
                freq  = 880f;
                Debug.Log("[RecipeValidator] ✅ Receta correcta.");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say("¡Fórmula correcta! El suero arácnido está listo.");
                if (Bitacora.Instance != null)
                    Bitacora.Instance.Registrar($"Suero Arácnido — {receta.displayName}");
                break;

            case ResultadoValidacion.Inestable:
                msg   = "⚠ INESTABLE";
                color = Color.yellow;
                freq  = 440f;
                Debug.Log("[RecipeValidator] ⚠ Receta inestable (temperatura fuera de rango).");
                if (ScoreSystem.Instance != null)
                    ScoreSystem.Instance.RegisterError(ErrorCode.E6_FueraDeRango, "Reactor inestable al validar");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say("La mezcla es inestable. Ajusta la temperatura del reactor.");
                break;

            default:
                msg   = "✗ FALLIDO";
                color = Color.red;
                freq  = 220f;
                Debug.Log("[RecipeValidator] ✗ Receta fallida.");
                if (NOVA.Instance != null)
                    NOVA.Instance.Say("Fórmula incorrecta. Revisa los reactivos y el orden.");
                break;
        }

        if (pilotoResultado != null) pilotoResultado.material.color = color;
        if (txtResultado    != null) txtResultado.text = msg;
        if (audioSource     != null) audioSource.PlayOneShot(SimpleTones.Beep(freq, 0.3f));
        if (HapticFeedback.Instance != null)
        {
            if (resultado == ResultadoValidacion.Correcto) HapticFeedback.Instance.Suave();
            else HapticFeedback.Instance.Error();
        }

        OnValidado?.Invoke(resultado);
    }

    ResultadoValidacion EvalRecipe()
    {
        // Verificar que el reactor esté estable
        if (reactor != null && !reactor.Estable)
            return ResultadoValidacion.Inestable;

        // Verificar que el matraz tenga los ingredientes correctos
        if (matraz == null) return ResultadoValidacion.Fallido;

        var mezcla = matraz.ObtenerMezcla();
        if (mezcla == null || mezcla.Count == 0) return ResultadoValidacion.Fallido;

        // Verificar orden y cantidad
        if (mezcla.Count != receta.steps.Count) return ResultadoValidacion.Fallido;

        for (int i = 0; i < receta.steps.Count; i++)
        {
            if (mezcla[i].chemical != receta.steps[i].chemical)   return ResultadoValidacion.Fallido;
            if (mezcla[i].ampollas != receta.steps[i].ampollas)   return ResultadoValidacion.Fallido;
        }

        return ResultadoValidacion.Correcto;
    }
}
