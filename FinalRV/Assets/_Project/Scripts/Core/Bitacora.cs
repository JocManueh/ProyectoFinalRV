/*
 * DESCRIPCIÓN: Lista en memoria de fórmulas descubiertas durante el capítulo.
 * Se consulta al final para el informe de evaluación.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bitacora : MonoBehaviour
{
    public static Bitacora Instance { get; private set; }

    readonly List<string> entradas = new List<string>();
    public IReadOnlyList<string> Entradas => entradas;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void Registrar(string formula)
    {
        entradas.Add($"[{Time.unscaledTime:F1}s] {formula}");
        Debug.Log($"[Bitacora] Guardado: {formula}");
    }

    public string ObtenerResumen()
    {
        if (entradas.Count == 0) return "Sin entradas.";
        return string.Join("\n", entradas);
    }
}
