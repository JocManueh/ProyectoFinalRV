/*
 * Construye el minijuego de Extracción de ADN en 01_Laboratorio:
 *
 *  1. Añade ChemicalContainer (requiresProcessing=true) al Spider Mecha
 *  2. Crea 4 sockets en orden sobre el Rack con sus reactivos agarrables:
 *       Socket_0 = Araña (Spider Mecha)
 *       Socket_1 = Detergente (ampolla Salina como proxy)
 *       Socket_2 = Sal       (ampolla Salina como proxy)
 *       Socket_3 = Alcohol   (ampolla Salina como proxy)
 *  3. Crea la varilla XRGrabInteractable
 *  4. Crea 4 pilotos (esferas) indicadores de progreso
 *  5. Conecta todo al DNAExtractor existente
 *
 * Ejecutar: Tools > Construir Extractor ADN (Lab)
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;

public static class Construir_Extractor
{
    const string SO_PATH     = "Assets/_Project/ScriptableObjects/Cap1/";
    const string AMPOLLA_PREFAB = "Assets/Proyecto/Prefabs/Ampolla.prefab";

    [MenuItem("Tools/Construir Extractor ADN (Lab)")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("01_Laboratorio");
        if (!scene.isLoaded)
        {
            Debug.LogError("[Extractor] Abre 01_Laboratorio primero.");
            return;
        }

        // ── Cargar SOs ────────────────────────────────────────────────────────
        var soADN      = Load<ChemicalSO>("Chemical_ADN_Arana.asset");
        var soSalina   = Load<ChemicalSO>("Chemical_Salina.asset");
        var ampPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(AMPOLLA_PREFAB);

        if (soADN == null)   { Debug.LogError("[Extractor] Chemical_ADN_Arana.asset no encontrado. Ejecuta CrearDatosSpider primero."); return; }
        if (soSalina == null){ Debug.LogError("[Extractor] Chemical_Salina.asset no encontrado."); return; }

        // ── 1. Spider Mecha: añadir ChemicalContainer ─────────────────────────
        var spiderGO = GameObject.Find("Spider Mecha ");
        if (spiderGO == null) spiderGO = GameObject.Find("Spider Mecha");
        if (spiderGO == null) { Debug.LogError("[Extractor] Spider Mecha no encontrado."); return; }

        var contSpider = spiderGO.GetComponent<ChemicalContainer>();
        if (contSpider == null) contSpider = spiderGO.AddComponent<ChemicalContainer>();

        var soContSpider = new SerializedObject(contSpider);
        soContSpider.FindProperty("sustancia").objectReferenceValue = soADN;
        soContSpider.FindProperty("volumen").intValue = 10;
        soContSpider.ApplyModifiedProperties();

        // requiresProcessing está en el ChemicalSO — asegurarlo
        var soADN_SO = new SerializedObject(soADN);
        soADN_SO.FindProperty("requiresProcessing").boolValue = true;
        soADN_SO.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();

        Debug.Log("[Extractor] ChemicalContainer añadido a Spider Mecha.");

        // ── Posición de referencia: Rack ──────────────────────────────────────
        var rackGO = GameObject.Find("Rack");
        Vector3 rackPos = rackGO != null
            ? rackGO.transform.position
            : new Vector3(-1.89f, 1.29f, 0.73f);

        // Los 4 sockets se colocan en fila sobre el rack
        // Separación de 0.18m entre sockets
        string[] nombresSocket  = { "Socket_Arana", "Socket_Detergente", "Socket_Sal", "Socket_Alcohol" };
        string[] labelsSocket   = { "ARAÑA", "DETERGENTE", "SAL", "ALCOHOL" };
        Color[]  coloresSocket  = {
            new Color(0.2f, 0.7f, 0.2f),
            new Color(0.2f, 0.5f, 0.9f),
            new Color(0.9f, 0.9f, 0.9f),
            new Color(0.5f, 0.8f, 1.0f)
        };

        // Reactivos proxy: ampollas con sus SOs
        // Detergente, Sal y Alcohol usan Salina como proxy visual (mismo prefab)
        // El socket_0 acepta el Spider Mecha directamente
        ChemicalSO[] soReactivos = { null, soSalina, soSalina, soSalina };

        // Destruir sockets anteriores si existen
        foreach (var n in nombresSocket)
        {
            var viejo = GameObject.Find(n);
            if (viejo != null) Object.DestroyImmediate(viejo);
        }

        var socketsCreados   = new XRSocketInteractor[4];
        var pilotosCreados   = new Renderer[4];

        for (int i = 0; i < 4; i++)
        {
            // ── Raíz del socket ───────────────────────────────────────────────
            var socketGO = new GameObject(nombresSocket[i]);
            socketGO.transform.position = rackPos + new Vector3(-0.27f + i * 0.18f, 0.08f, 0f);

            // ── Base visual (cilindro pequeño = "tubo") ───────────────────────
            var tubo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tubo.name = "Tubo";
            tubo.transform.SetParent(socketGO.transform, false);
            tubo.transform.localPosition = Vector3.zero;
            tubo.transform.localScale    = new Vector3(0.06f, 0.05f, 0.06f);
            var matTubo = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matTubo.color = coloresSocket[i] * 0.5f;
            tubo.GetComponent<Renderer>().sharedMaterial = matTubo;
            Object.DestroyImmediate(tubo.GetComponent<Collider>());

            // ── Piloto (esfera indicadora) ─────────────────────────────────────
            var piloto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piloto.name = "Piloto";
            piloto.transform.SetParent(socketGO.transform, false);
            piloto.transform.localPosition = new Vector3(0f, 0.07f, 0f);
            piloto.transform.localScale    = Vector3.one * 0.025f;
            var matPiloto = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matPiloto.color = Color.gray;
            piloto.GetComponent<Renderer>().sharedMaterial = matPiloto;
            Object.DestroyImmediate(piloto.GetComponent<Collider>());
            pilotosCreados[i] = piloto.GetComponent<Renderer>();

            // ── Etiqueta TMP ──────────────────────────────────────────────────
            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(socketGO.transform, false);
            lblGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            lblGO.transform.localScale    = Vector3.one * 0.003f;
            var lblCanvas = lblGO.AddComponent<Canvas>();
            lblCanvas.renderMode = RenderMode.WorldSpace;
            var lblRect = lblGO.GetComponent<RectTransform>();
            lblRect.sizeDelta = new Vector2(60f, 16f);
            var lblTxtGO = new GameObject("Txt");
            lblTxtGO.transform.SetParent(lblGO.transform, false);
            var lblTxtRect = lblTxtGO.AddComponent<RectTransform>();
            lblTxtRect.anchorMin = Vector2.zero;
            lblTxtRect.anchorMax = Vector2.one;
            lblTxtRect.offsetMin = lblTxtRect.offsetMax = Vector2.zero;
            var tmp = lblTxtGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = labelsSocket[i];
            tmp.fontSize  = 10f;
            tmp.color     = coloresSocket[i];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            // ── AudioSource ───────────────────────────────────────────────────
            var audio = socketGO.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.playOnAwake  = false;

            // ── XRSocketInteractor ────────────────────────────────────────────
            var socket = socketGO.AddComponent<XRSocketInteractor>();
            socketsCreados[i] = socket;

            // ── Reactivo proxy agarrable (excepto socket_0 que usa Spider Mecha) ──
            if (i > 0 && ampPrefab != null && soReactivos[i] != null)
            {
                var reactivo = (GameObject)PrefabUtility.InstantiatePrefab(ampPrefab);
                reactivo.name = $"Reactivo_{labelsSocket[i]}";
                reactivo.transform.position = rackPos + new Vector3(-0.27f + i * 0.18f, 0.2f, 0f);
                reactivo.transform.localScale = Vector3.one * 0.08f;

                var cont = reactivo.GetComponent<ChemicalContainer>();
                if (cont != null)
                {
                    var soCont = new SerializedObject(cont);
                    soCont.FindProperty("sustancia").objectReferenceValue = soReactivos[i];
                    soCont.FindProperty("volumen").intValue = 10;
                    soCont.ApplyModifiedProperties();
                    cont.Refrescar();
                }
            }
        }

        // ── 3. Varilla ────────────────────────────────────────────────────────
        var varillaVieja = GameObject.Find("Varilla_Extractor");
        if (varillaVieja != null) Object.DestroyImmediate(varillaVieja);

        var varillaGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        varillaGO.name = "Varilla_Extractor";
        varillaGO.transform.position   = rackPos + new Vector3(0.3f, 0.15f, 0f);
        varillaGO.transform.localScale = new Vector3(0.015f, 0.1f, 0.015f);
        var matVarilla = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matVarilla.color = new Color(0.8f, 0.8f, 0.8f);
        varillaGO.GetComponent<Renderer>().sharedMaterial = matVarilla;
        var rbVarilla = varillaGO.AddComponent<Rigidbody>();
        rbVarilla.mass = 0.1f;
        var varillaGrab = varillaGO.AddComponent<XRGrabInteractable>();

        // Etiqueta varilla
        var lblVarilla = new GameObject("Label");
        lblVarilla.transform.SetParent(varillaGO.transform, false);
        lblVarilla.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        lblVarilla.transform.localScale    = Vector3.one * 0.06f;
        var cvVarilla = lblVarilla.AddComponent<Canvas>();
        cvVarilla.renderMode = RenderMode.WorldSpace;
        var rvVarilla = lblVarilla.GetComponent<RectTransform>();
        rvVarilla.sizeDelta = new Vector2(60f, 16f);
        var tvGO = new GameObject("Txt");
        tvGO.transform.SetParent(lblVarilla.transform, false);
        var tvRect = tvGO.AddComponent<RectTransform>();
        tvRect.anchorMin = Vector2.zero; tvRect.anchorMax = Vector2.one;
        tvRect.offsetMin = tvRect.offsetMax = Vector2.zero;
        var tvTmp = tvGO.AddComponent<TextMeshProUGUI>();
        tvTmp.text = "VARILLA"; tvTmp.fontSize = 10f;
        tvTmp.color = Color.white; tvTmp.alignment = TextAlignmentOptions.Center;

        Debug.Log("[Extractor] Varilla creada.");

        // ── 4. AudioSource en DNAExtractor ────────────────────────────────────
        var extractorGO   = GameObject.Find("DNAExtractor");
        var extractorComp = extractorGO?.GetComponent<DNAExtractor>();
        if (extractorComp == null)
        {
            Debug.LogError("[Extractor] DNAExtractor no encontrado en la escena.");
            return;
        }

        var audioExtractor = extractorGO.GetComponent<AudioSource>();
        if (audioExtractor == null) audioExtractor = extractorGO.AddComponent<AudioSource>();
        audioExtractor.spatialBlend = 1f;
        audioExtractor.playOnAwake  = false;

        // ── 5. Conectar todo al DNAExtractor ──────────────────────────────────
        var soExt = new SerializedObject(extractorComp);

        var propSockets = soExt.FindProperty("sockets");
        propSockets.arraySize = 4;
        for (int i = 0; i < 4; i++)
            propSockets.GetArrayElementAtIndex(i).objectReferenceValue = socketsCreados[i];

        soExt.FindProperty("varilla").objectReferenceValue      = varillaGrab;
        soExt.FindProperty("adnSO").objectReferenceValue        = soADN;
        soExt.FindProperty("ampollaPrefab").objectReferenceValue = ampPrefab;
        soExt.FindProperty("audioSource").objectReferenceValue  = audioExtractor;

        // puntoSpawn ya existe (DNAExtractor/SpawnADN)
        var spawnGO = extractorGO.transform.Find("SpawnADN");
        if (spawnGO != null)
            soExt.FindProperty("puntoSpawn").objectReferenceValue = spawnGO;

        var propPilotos = soExt.FindProperty("pilotosOrden");
        propPilotos.arraySize = 4;
        for (int i = 0; i < 4; i++)
            propPilotos.GetArrayElementAtIndex(i).objectReferenceValue = pilotosCreados[i];

        soExt.ApplyModifiedProperties();

        Debug.Log("[Extractor] DNAExtractor conectado con 4 sockets, varilla y pilotos.");

        // ── Guardar ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Extractor] ✅ Minijuego de extracción de ADN construido y guardado.");
    }

    static T Load<T>(string nombre) where T : UnityEngine.Object
        => AssetDatabase.LoadAssetAtPath<T>(SO_PATH + nombre);
}
