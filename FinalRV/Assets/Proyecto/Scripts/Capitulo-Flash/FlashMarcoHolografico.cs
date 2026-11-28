using UnityEngine;
using UnityEngine.UI;

/// <summary>Dibuja un marco cian con esquinas recortadas sobre el fondo translúcido de Flash.</summary>
public class FlashMarcoHolografico : MaskableGraphic
{
    [SerializeField] Color borde = new Color(0.18f, 0.88f, 1f, 0.9f);
    [SerializeField, Min(2f)] float recorte = 20f;
    [SerializeField, Min(0.5f)] float grosor = 2f;

    protected override void OnPopulateMesh(VertexHelper malla)
    {
        malla.Clear();
        Rect r = GetPixelAdjustedRect();
        if (r.width < 2f || r.height < 2f) return;

        float c = Mathf.Min(recorte, Mathf.Min(r.width, r.height) * 0.16f);
        var contorno = new[]
        {
            new Vector2(r.xMin + c, r.yMin), new Vector2(r.xMax - c, r.yMin),
            new Vector2(r.xMax, r.yMin + c), new Vector2(r.xMax, r.yMax - c),
            new Vector2(r.xMax - c, r.yMax), new Vector2(r.xMin + c, r.yMax),
            new Vector2(r.xMin, r.yMax - c), new Vector2(r.xMin, r.yMin + c)
        };

        malla.AddVert(r.center, color, Vector2.zero);
        foreach (Vector2 punto in contorno) malla.AddVert(punto, color, Vector2.zero);
        Color halo = new Color(borde.r, borde.g, borde.b, 0.12f);
        for (int i = 0; i < contorno.Length; i++)
        {
            int siguiente = (i + 1) % contorno.Length;
            malla.AddTriangle(0, 1 + siguiente, 1 + i);
            Vector2 a = contorno[i];
            Vector2 b = contorno[siguiente];
            Linea(malla, a, b, grosor * 3f, halo);
            Linea(malla, a, b, grosor, borde);

            float tramo = Mathf.Min(0.3f, c * 1.3f / Vector2.Distance(a, b));
            Linea(malla, a, Vector2.Lerp(a, b, tramo), grosor * 2f, borde);
            Linea(malla, Vector2.Lerp(a, b, 1f - tramo), b, grosor * 2f, borde);
        }

        Color guia = new Color(borde.r, borde.g, borde.b, 0.16f);
        float margen = c * 0.75f;
        for (int i = 1; i < 8; i++)
        {
            float y = r.yMin + r.height * i / 8f;
            Linea(malla, new Vector2(r.xMin + margen, y), new Vector2(r.xMax - margen, y),
                grosor * 0.25f, guia);
        }
    }

    static void Linea(VertexHelper malla, Vector2 a, Vector2 b, float ancho, Color tinte)
    {
        Vector2 direccion = (b - a).normalized;
        Vector2 lado = new Vector2(-direccion.y, direccion.x) * (ancho * 0.5f);
        int n = malla.currentVertCount;
        malla.AddVert(a - lado, tinte, Vector2.zero);
        malla.AddVert(a + lado, tinte, Vector2.zero);
        malla.AddVert(b + lado, tinte, Vector2.zero);
        malla.AddVert(b - lado, tinte, Vector2.zero);
        malla.AddTriangle(n, n + 1, n + 2);
        malla.AddTriangle(n + 2, n + 3, n);
    }
}

