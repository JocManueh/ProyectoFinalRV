/*
 * DESCRIPCIÓN: Administra el puntaje del jugador y lleva un registro detallado de cada 
 * error cometido (código, motivo y segundo exacto), restando puntos según la gravedad 
 * de la falla para generar el informe de evaluación final.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ErrorCode
{
    E1_Vacio,
    E2_SinProcesar,
    E3_Incorrecto,
    E4_Cantidad,
    E5_Orden,
    E6_FueraDeRango,
    E7_Radiacion,
    E8_Tiempo
}

[Serializable]
public class ErrorLog
{
    public string codigo;
    public string detalle;
    public int penalizacion;
    public float segundos; // segundos desde que arrancó la app
}

/// <summary>Guarda los puntos, el tiempo jugado y los errores de un capítulo.</summary>
public sealed class ResultadoCapitulo
{
    public int Puntos { get; }
    public float SegundosJugados { get; }
    public string Motivos { get; }

    public ResultadoCapitulo(int puntos, float segundosJugados, string motivos)
    {
        Puntos = puntos;
        SegundosJugados = Mathf.Max(0f, segundosJugados);
        Motivos = motivos;
    }
}

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }
    public int TotalScore { get; private set; }
    public List<ErrorLog> Log { get; private set; } = new List<ErrorLog>();
    public event Action<int> OnScoreChanged;
    public event Action<ErrorLog> OnErrorRegistrado;
    public ResultadoCapitulo ResultadoSpider { get; private set; }
    public ResultadoCapitulo ResultadoFlash { get; private set; }

    bool registrandoSpider;
    int puntosSpider;
    int primerErrorSpider;

    public void IniciarSpider()
    {
        registrandoSpider = true;
        puntosSpider = 0;
        primerErrorSpider = Log.Count;
        ResultadoSpider = null;
        ResultadoFlash = null;
    }

    public void GuardarSpider(float segundos)
    {
        if (!registrandoSpider) return;
        registrandoSpider = false;
        var motivos = new Dictionary<string, int>();
        for (int i = primerErrorSpider; i < Log.Count; i++)
        {
            ErrorLog error = Log[i];
            string motivo = MotivoSpider(error);
            motivos.TryGetValue(motivo, out int cantidad);
            motivos[motivo] = cantidad + 1;
        }

        var resumen = new System.Text.StringBuilder();
        foreach (var motivo in motivos)
        {
            if (resumen.Length > 0) resumen.Append(" · ");
            resumen.Append(motivo.Key);
            if (motivo.Value > 1) resumen.Append($" ×{motivo.Value}");
        }
        ResultadoSpider = new ResultadoCapitulo(puntosSpider, segundos, resumen.ToString());
    }

    static string MotivoSpider(ErrorLog error)
    {
        switch (error.codigo)
        {
            case nameof(ErrorCode.E1_Vacio): return "Ampolla vacía";
            case nameof(ErrorCode.E2_SinProcesar): return "Reactivo sin procesar";
            case nameof(ErrorCode.E3_Incorrecto): return "Reactivo o clasificación incorrectos";
            case nameof(ErrorCode.E4_Cantidad): return "Cantidad incorrecta";
            case nameof(ErrorCode.E5_Orden): return "Orden incorrecto";
            case nameof(ErrorCode.E6_FueraDeRango): return "Reactor fuera de rango";
            case nameof(ErrorCode.E7_Radiacion): return "Tritio manipulado sin pinzas";
            default: return string.IsNullOrWhiteSpace(error.detalle) ? error.codigo : error.detalle;
        }
    }

    public void GuardarFlash(ResultadoCapitulo resultado) => ResultadoFlash = resultado;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static int PenalizacionDe(ErrorCode codigo)
    {
        switch (codigo)
        {
            case ErrorCode.E1_Vacio:
            case ErrorCode.E2_SinProcesar:
            case ErrorCode.E3_Incorrecto:
            case ErrorCode.E4_Cantidad:
                return 10; // errores leves
            case ErrorCode.E5_Orden:
            case ErrorCode.E6_FueraDeRango:
            case ErrorCode.E7_Radiacion:
                return 25; // errores críticos
            default:
                return 0; // E8 (tiempo) lo maneja el capítulo
        }
    }

    public void Add(int puntos, string motivo)
    {
        if (registrandoSpider)
        {
            int nuevo = Mathf.Max(0, puntosSpider + puntos);
            puntos = nuevo - puntosSpider;
            puntosSpider = nuevo;
        }
        TotalScore = Mathf.Max(0, TotalScore + puntos);
        Debug.Log($"[Score] {puntos:+#;-#;0} ({motivo}) => total {TotalScore}");
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void RegisterError(ErrorCode codigo, string detalle = "")
    {
        int pen = PenalizacionDe(codigo);
        var entrada = new ErrorLog
        {
            codigo = codigo.ToString(),
            detalle = detalle,
            penalizacion = pen,
            segundos = Time.unscaledTime
        };
        Log.Add(entrada);
        Add(-pen, $"{codigo} {detalle}");
        OnErrorRegistrado?.Invoke(entrada);
    }

    public void ResetAll()
    {
        TotalScore = 0;
        Log.Clear();
        registrandoSpider = false;
        puntosSpider = 0;
        primerErrorSpider = 0;
        ResultadoSpider = null;
        ResultadoFlash = null;
        OnScoreChanged?.Invoke(TotalScore);
    }
}