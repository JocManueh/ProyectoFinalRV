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

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }
    public int TotalScore { get; private set; }
    public List<ErrorLog> Log { get; private set; } = new List<ErrorLog>();
    public event Action<int> OnScoreChanged;

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
        TotalScore = Mathf.Max(0, TotalScore + puntos);
        Debug.Log($"[Score] {puntos:+#;-#;0} ({motivo}) => total {TotalScore}");
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void RegisterError(ErrorCode codigo, string detalle = "")
    {
        int pen = PenalizacionDe(codigo);
        Log.Add(new ErrorLog
        {
            codigo = codigo.ToString(),
            detalle = detalle,
            penalizacion = pen,
            segundos = Time.unscaledTime
        });
        Add(-pen, $"{codigo} {detalle}");
    }

    public void ResetAll()
    {
        TotalScore = 0;
        Log.Clear();
        OnScoreChanged?.Invoke(TotalScore);
    }
}