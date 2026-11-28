/*
 * Crea los ChemicalSO de Alcohol, Detergente y Sal,
 * añade XRGrabInteractable + ChemicalContainer a los 3 modelos,
 * y conecta sustanciasEsperadas[] en el DNAExtractor.
 *
 * Ejecutar: Tools > Configurar Modelos Extractor (Lab)
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class Configurar_ModelosExtractor
{
    const string SO_PATH = "Assets/_Project/ScriptableObjects/Cap1/";

    [MenuItem("Tools/Configurar Modelos Extractor (Lab)")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("01_Laboratorio");
        if (!scene.isLoaded)
        {
            Debug.LogError("[ConfigExtractor] Abre 01_Laboratorio primero.");
            return;
        }

        // ── 1. Crear / actualizar ChemicalSOs ─────────────────────────────────

        var soAlcohol    = ObtenerOCrear<ChemicalSO>("Chemical_Alcohol.asset");
        var soDetergente = ObtenerOCrear<ChemicalSO>("Chemical_Detergente.asset");
        var soSal        = ObtenerOCrear<ChemicalSO>("Chemical_Sal.asset");
        var soADN        = AssetDatabase.LoadAssetAtPath<ChemicalSO>(
                               SO_PATH + "Chemical_ADN_Arana.asset");

        // Alcohol
        var saAlcohol = new SerializedObject(soAlcohol);
        saAlcohol.FindProperty("id").stringValue          = "alcohol";
        saAlcohol.FindProperty("displayName").stringValue = "Alcohol frío";
        saAlcohol.FindProperty("symbol").stringValue      = "C₂H₅OH";
        saAlcohol.FindProperty("type").enumValueIndex     = (int)ChemicalType.Compuesto;
        saAlcohol.FindProperty("hazardNote").stringValue  = "Alcohol etílico frío. Precipita el ADN.";
        SetColor(saAlcohol, new Color(0.85f, 0.95f, 1f));
        saAlcohol.FindProperty("requiresProcessing").boolValue = false;
        saAlcohol.ApplyModifiedProperties();

        // Detergente
        var saDetergente = new SerializedObject(soDetergente);
        saDetergente.FindProperty("id").stringValue          = "detergente";
        saDetergente.FindProperty("displayName").stringValue = "Detergente (SDS)";
        saDetergente.FindProperty("symbol").stringValue      = "SDS";
        saDetergente.FindProperty("type").enumValueIndex     = (int)ChemicalType.Compuesto;
        saDetergente.FindProperty("hazardNote").stringValue  = "Dodecilsulfato sódico. Rompe membranas celulares.";
        SetColor(saDetergente, new Color(0.3f, 0.7f, 1f));
        saDetergente.FindProperty("requiresProcessing").boolValue = false;
        saDetergente.ApplyModifiedProperties();

        // Sal
        var saSal = new SerializedObject(soSal);
        saSal.FindProperty("id").stringValue          = "sal";
        saSal.FindProperty("displayName").stringValue = "Sal (NaCl)";
        saSal.FindProperty("symbol").stringValue      = "NaCl";
        saSal.FindProperty("type").enumValueIndex     = (int)ChemicalType.Compuesto;
        saSal.FindProperty("hazardNote").stringValue  = "Cloruro sódico. Precipita proteínas.";
        SetColor(saSal, new Color(1f, 1f, 0.9f));
        saSal.FindProperty("requiresProcessing").boolValue = false;
        saSal.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        Debug.Log("[ConfigExtractor] ChemicalSOs creados/actualizados.");

        // ── 2. Configurar modelos en la escena ────────────────────────────────
        ConfigurarModelo("alcoholBottle",        soAlcohol,    new Color(0.85f, 0.95f, 1f));
        ConfigurarModelo("Cloralex El rendidor", soDetergente, new Color(0.3f,  0.7f,  1f));
        ConfigurarModelo("SaltShaker",           soSal,        new Color(1f,    1f,    0.9f));

        // ── 3. Conectar sustanciasEsperadas[] en DNAExtractor ─────────────────
        var extractorGO   = GameObject.Find("DNAExtractor");
        var extractorComp = extractorGO?.GetComponent<DNAExtractor>();
        if (extractorComp == null)
        {
            Debug.LogError("[ConfigExtractor] DNAExtractor no encontrado.");
            return;
        }

        var soExt = new SerializedObject(extractorComp);
        var propSust = soExt.FindProperty("sustanciasEsperadas");
        propSust.arraySize = 4;
        // [0] = null → se valida por requiresProcessing (araña)
        propSust.GetArrayElementAtIndex(0).objectReferenceValue = soADN;   // araña tiene soADN con requiresProcessing=true
        propSust.GetArrayElementAtIndex(1).objectReferenceValue = soDetergente;
        propSust.GetArrayElementAtIndex(2).objectReferenceValue = soSal;
        propSust.GetArrayElementAtIndex(3).objectReferenceValue = soAlcohol;
        soExt.ApplyModifiedProperties();

        Debug.Log("[ConfigExtractor] DNAExtractor.sustanciasEsperadas conectado.");

        // ── Guardar ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();
        Debug.Log("[ConfigExtractor] ✅ Todo configurado y guardado.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void ConfigurarModelo(string nombre, ChemicalSO so, Color colorEtiqueta)
    {
        // Buscar incluyendo espacios al final del nombre
        var go = GameObject.Find(nombre)
              ?? GameObject.Find(nombre.TrimEnd());
        if (go == null)
        {
            Debug.LogWarning($"[ConfigExtractor] No encontrado: '{nombre}'");
            return;
        }

        // ── Rigidbody ─────────────────────────────────────────────────────────
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.3f;

        // ── Collider: si no tiene ninguno, añadir BoxCollider ─────────────────
        if (go.GetComponentInChildren<Collider>() == null)
        {
            var box = go.AddComponent<BoxCollider>();
            // Ajustar al bounds del objeto
            var bounds = CalcularBoundsLocales(go);
            box.center = bounds.center - go.transform.position;
            box.size   = bounds.size;
        }

        // ── XRGrabInteractable ────────────────────────────────────────────────
        var grab = go.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = go.AddComponent<XRGrabInteractable>();

        // ── ChemicalContainer ─────────────────────────────────────────────────
        var cont = go.GetComponent<ChemicalContainer>();
        if (cont == null) cont = go.AddComponent<ChemicalContainer>();

        var soCont = new SerializedObject(cont);
        soCont.FindProperty("sustancia").objectReferenceValue = so;
        soCont.FindProperty("volumen").intValue = 10;
        soCont.ApplyModifiedProperties();

        Debug.Log($"[ConfigExtractor] '{nombre}' configurado con {so.displayName}.");
    }

    static Bounds CalcularBoundsLocales(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.1f);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    static T ObtenerOCrear<T>(string nombre) where T : ScriptableObject
    {
        string ruta = SO_PATH + nombre;
        var existente = AssetDatabase.LoadAssetAtPath<T>(ruta);
        if (existente != null) return existente;
        var nuevo = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(nuevo, ruta);
        return nuevo;
    }

    static void SetColor(SerializedObject so, Color c)
    {
        var prop = so.FindProperty("color");
        if (prop == null) return;
        prop.colorValue = c;
    }
}
