#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menú Proyecto > Crear Nave Enemiga (v2).
/// Platillo de combate: plato ancho, borde, aro de luces rojas, núcleo rojo, cúpula y antenas.
/// Guarda el prefab en Assets/Proyecto/Prefabs/Nave_Enemiga.prefab y deja una copia en la escena.
/// Si ya hay una Nave_Enemiga en la escena, la reemplaza.
/// Va dentro de una carpeta llamada Editor.
/// </summary>
public static class CrearNave
{
    const string CarpetaMat = "Assets/Proyecto/Materials";
    const string CarpetaPrefab = "Assets/Proyecto/Prefabs";

    [MenuItem("Proyecto/Crear Nave Enemiga")]
    static void Crear()
    {
        AsegurarCarpeta("Assets/Proyecto", "Materials");
        AsegurarCarpeta("Assets/Proyecto", "Prefabs");

        // Borra la nave anterior de la escena (si existe)
        var vieja = GameObject.Find("Nave_Enemiga");
        while (vieja != null)
        {
            Object.DestroyImmediate(vieja);
            vieja = GameObject.Find("Nave_Enemiga");
        }

        Material casco = Mat("Nave_Casco", new Color(0.07f, 0.08f, 0.10f), Color.black, 1.0f, 0.75f);
        Material placa = Mat("Nave_Placa", new Color(0.14f, 0.15f, 0.18f), Color.black, 1.0f, 0.55f);
        Material cupula = Mat("Nave_Cupula", new Color(0.03f, 0.05f, 0.07f), new Color(0.0f, 0.35f, 0.45f), 0.9f, 0.95f);
        Material rojo = Mat("Nave_LuzRoja", new Color(0.4f, 0.02f, 0.02f), new Color(5f, 0.12f, 0.04f), 0.2f, 0.3f);

        var raiz = new GameObject("Nave_Enemiga");
        var t = raiz.transform;

        // Cuerpo
        Pieza(t, "Disco", PrimitiveType.Sphere, new Vector3(0, 0, 0), new Vector3(30, 2.4f, 30), casco, Quaternion.identity);
        Pieza(t, "PlacaSuperior", PrimitiveType.Cylinder, new Vector3(0, 1.0f, 0), new Vector3(24, 0.25f, 24), placa, Quaternion.identity);
        Pieza(t, "PlacaInferior", PrimitiveType.Cylinder, new Vector3(0, -1.2f, 0), new Vector3(16, 0.5f, 16), placa, Quaternion.identity);
        Pieza(t, "Cupula", PrimitiveType.Sphere, new Vector3(0, 1.7f, 0), new Vector3(8, 3.6f, 8), cupula, Quaternion.identity);
        Pieza(t, "Nucleo", PrimitiveType.Sphere, new Vector3(0, -2.4f, 0), new Vector3(9, 3f, 9), rojo, Quaternion.identity);

        // Aro de luces rojas alargadas alrededor del borde
        const int n = 24;
        for (int i = 0; i < n; i++)
        {
            float a = i * Mathf.PI * 2f / n;
            var pos = new Vector3(Mathf.Cos(a) * 14.2f, 0.0f, Mathf.Sin(a) * 14.2f);
            var rot = Quaternion.Euler(0f, -(a * Mathf.Rad2Deg + 90f), 0f);
            Pieza(t, "AroLuz_" + i, PrimitiveType.Cube, pos, new Vector3(2.6f, 0.35f, 0.6f), rojo, rot);
        }

        // Antenas con punta roja
        for (int i = 0; i < 4; i++)
        {
            float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
            var basePos = new Vector3(Mathf.Cos(a) * 6.5f, 2.6f, Mathf.Sin(a) * 6.5f);
            Pieza(t, "Antena_" + i, PrimitiveType.Cylinder, basePos + Vector3.up * 1.2f, new Vector3(0.25f, 1.6f, 0.25f), placa, Quaternion.identity);
            Pieza(t, "PuntaAntena_" + i, PrimitiveType.Sphere, basePos + Vector3.up * 2.9f, Vector3.one * 0.7f, rojo, Quaternion.identity);
        }

        // Luz roja que ilumina hacia abajo y alrededor
        var luzObj = new GameObject("Luz_Roja");
        luzObj.transform.SetParent(t, false);
        luzObj.transform.localPosition = new Vector3(0, -5f, 0);
        var luz = luzObj.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(1f, 0.12f, 0.06f);
        luz.range = 90f;
        luz.intensity = 18f;

        raiz.transform.position = new Vector3(0, 14, 45);

        string ruta = CarpetaPrefab + "/Nave_Enemiga.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(raiz, ruta, InteractionMode.UserAction);
        Selection.activeGameObject = raiz;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(ruta));
        Debug.Log("[CrearNave] Nave v2 creada y guardada en " + ruta);
    }

    static void Pieza(Transform padre, string nombre, PrimitiveType tipo, Vector3 pos, Vector3 escala, Material mat, Quaternion rot)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static Material Mat(string nombre, Color baseColor, Color emision, float metalico, float suavidad)
    {
        string ruta = CarpetaMat + "/" + nombre + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(ruta) != null) AssetDatabase.DeleteAsset(ruta);

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.SetColor("_BaseColor", baseColor);
        m.SetColor("_Color", baseColor);
        m.SetFloat("_Metallic", metalico);
        m.SetFloat("_Smoothness", suavidad);

        if (emision.maxColorComponent > 0.001f)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emision);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        AssetDatabase.CreateAsset(m, ruta);
        return m;
    }

    static void AsegurarCarpeta(string padre, string hijo)
    {
        if (!AssetDatabase.IsValidFolder(padre + "/" + hijo))
            AssetDatabase.CreateFolder(padre, hijo);
    }
}
#endif
