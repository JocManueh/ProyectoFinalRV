/*
 * DESCRIPCIÓN: PanelUI con los 9 puntos del capítulo. Se marca cada uno
 * cuando el Cap1Manager llama a Completar(indice).
 */
using TMPro;
using System.Text;
using UnityEngine;

public class ChapterChecklist : MonoBehaviour
{
    [SerializeField] TMP_Text cuerpo;

    static readonly string[] items =
    {
        "Leer el cómic",
        "Leer la tabla de héroes (3 pistas)",
        "Clasificar los reactivos",
        "Extraer ADN de araña",
        "Obtener tritio con pinzas",
        "Mezclar la fórmula",
        "Estabilizar el reactor",
        "Analizar la muestra",
        "Resolver el evento EMP"
    };

    readonly bool[] hechos = new bool[9];

    void Start() => Refrescar();

    public void Completar(int idx)
    {
        if (idx < 0 || idx >= hechos.Length || hechos[idx]) return;
        hechos[idx] = true;
        Debug.Log($"[Checklist] ✅ {items[idx]}");
        Refrescar();
    }

    public bool TodoCompleto()
    {
        foreach (var h in hechos) if (!h) return false;
        return true;
    }

    void Refrescar()
    {
        if (cuerpo == null) return;
        var sb = new StringBuilder("CAPÍTULO 1 — CHECKLIST\n\n");
        for (int i = 0; i < items.Length; i++)
            sb.AppendLine((hechos[i] ? "[X] " : "[  ] ") + items[i]);
        cuerpo.text = sb.ToString();
    }
}
