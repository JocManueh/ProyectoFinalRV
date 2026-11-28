/*
 * DESCRIPCIÓN: PanelUI con los 9 puntos del capítulo. Se marca cada uno
 * cuando el Cap1Manager llama a Completar(indice).
 */
using TMPro;
using System.Text;
using UnityEngine;
using System; // Agrega esta línea al inicio del archivo

public class ChapterChecklist : MonoBehaviour
{
    [SerializeField] TMP_Text cuerpo;



    static readonly string[] items =
    {
        "Abre el cómic y lee las 4 tarjetas",
        "Leer la tabla de héroes (3 pistas)",
        "Clasificar los reactivos",
        "Extraer ADN de araña",
        "Obtener tritio con pinzas",
        "Mezclar la fórmula",
        "Estabilizar el reactor",
        "Analizar la muestra",
        "Resolver el evento EMP"
    };

    [SerializeField] bool usarChecklistSpider;
    readonly bool[] hechos = new bool[9];
    int clasificados;
    int reactivos = 6;

    int CantidadPasos => usarChecklistSpider ? 7 : hechos.Length;

    public static string NombrePaso(int indice) => items[indice];

    public void ActualizarClasificados(int cantidad, int total)
    {
        clasificados = cantidad;
        reactivos = total;
        Refrescar();
    }

    void Start() => Refrescar();

    public event Action<int> OnCompletado;

    public void Completar(int idx)
    {
        if (idx < 0 || idx >= hechos.Length || hechos[idx]) return;
        hechos[idx] = true;
        Debug.Log($"[Checklist] ✅ {items[idx]}");
        Refrescar();
        OnCompletado?.Invoke(idx);   // NUEVO
    }

    public bool TodoCompleto()
    {
        for (int i = 0; i < CantidadPasos; i++) if (!hechos[i]) return false;
        return true;
    }

    void Refrescar()
    {
        if (cuerpo == null) return;
        var sb = new StringBuilder("Nivel 1 · Spider-Man\n");
        int actual = SiguientePendiente();
        for (int i = 0; i < CantidadPasos; i++)
        {
            string progreso = usarChecklistSpider && i == 2 ? $" ({clasificados}/{reactivos})" : "";
            string linea = $"{i + 1}. {items[i]}{progreso}";
            if (hechos[i]) linea = $"<u>{linea}</u>";
            else if (i == actual) linea = $"<b>{linea}</b>";
            sb.AppendLine(linea);
        }
        if (usarChecklistSpider && TodoCompleto()) sb.AppendLine("<b>Pulsa ANALIZAR</b>");
        cuerpo.text = sb.ToString();
    }



    public int SiguientePendiente()
    {
        for (int i = 0; i < CantidadPasos; i++) if (!hechos[i]) return i;
        return -1;
    }
}
