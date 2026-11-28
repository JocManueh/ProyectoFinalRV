// v2
/*
 * Construye en la escena 01_Laboratorio activa:
 *   - Paso 4: TablaHeroes con PanelUI de 4 tarjetas + 3 PistaRevelable invisibles
 *   - Paso 5: 3 bandejas de clasificación con ClassificationTray
 *   - Conecta ambos al Cap1Manager existente
 *
 * Ejecutar desde Tools > Construir Tabla y Bandejas (Lab)
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;

public static class Construir_TablaYBandejas
{
    const string SO_PATH   = "Assets/_Project/ScriptableObjects/Cap1/";
    const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Construir Tabla y Bandejas (Lab)")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("01_Laboratorio");
        if (!scene.isLoaded)
        {
            Debug.LogError("[Construir] La escena 01_Laboratorio no está abierta.");
            return;
        }

        var capSO = AssetDatabase.LoadAssetAtPath<ChapterSO>(SO_PATH + "Chapter_Spider.asset");
        if (capSO == null) { Debug.LogError("[Construir] Chapter_Spider.asset no encontrado."); return; }

        // ── Buscar TablaHeroes existente ──────────────────────────────────────
        var tablaGO = GameObject.Find("TablaHeroes");
        if (tablaGO == null) { Debug.LogError("[Construir] TablaHeroes no encontrado en la escena."); return; }

        var tablaComp = tablaGO.GetComponent<TablaHeroes>();

        // Posición de referencia de la tabla (pared lateral)
        Vector3 tablaPos = tablaGO.transform.position;      // (1.8, 1.35, 4.78)
        Quaternion tablaRot = tablaGO.transform.rotation;   // Y=90

        // ── PASO 4: Construir el panel visual de la tabla ─────────────────────
        // Destruir panel anterior si existe
        var panelViejo = tablaGO.transform.Find("PanelTabla");
        if (panelViejo != null) Object.DestroyImmediate(panelViejo.gameObject);

        // Raíz del panel (canvas world-space)
        var panelRoot = new GameObject("PanelTabla");
        panelRoot.transform.SetParent(tablaGO.transform, false);
        panelRoot.transform.localPosition = Vector3.zero;
        panelRoot.transform.localRotation = Quaternion.identity;
        panelRoot.transform.localScale    = Vector3.one * 0.003f; // escala world-space estándar

        var canvas = panelRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        panelRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
        panelRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var rectCanvas = panelRoot.GetComponent<RectTransform>();
        rectCanvas.sizeDelta = new Vector2(500f, 380f);

        // Fondo del panel
        CrearUIPanel("Fondo", panelRoot.transform,
            new Vector2(500f, 380f), new Vector2(0f, 0f), new Color(0.05f, 0.07f, 0.12f, 0.95f));

        // ── Tarjeta 1: FICHA ──────────────────────────────────────────────────
        var txtFicha = CrearTMPUGUI("TxtFicha", panelRoot.transform,
            new Vector2(460f, 55f), new Vector2(0f, 155f),
            capSO.ficha, 11f, Color.cyan, TextAlignmentOptions.TopLeft);

        // Separador
        CrearUIPanel("Sep1", panelRoot.transform, new Vector2(460f, 2f), new Vector2(0f, 120f),
            new Color(0.3f, 0.5f, 0.8f, 0.8f));

        // ── Tarjeta 2: ORIGEN ─────────────────────────────────────────────────
        var txtOrigen = CrearTMPUGUI("TxtOrigen", panelRoot.transform,
            new Vector2(460f, 80f), new Vector2(0f, 55f),
            capSO.origen, 9.5f, Color.white, TextAlignmentOptions.TopLeft);

        // Separador
        CrearUIPanel("Sep2", panelRoot.transform, new Vector2(460f, 2f), new Vector2(0f, 10f),
            new Color(0.3f, 0.5f, 0.8f, 0.8f));

        // ── Tarjeta 3: CIENCIA REAL ───────────────────────────────────────────
        var txtCiencia = CrearTMPUGUI("TxtCiencia", panelRoot.transform,
            new Vector2(460f, 70f), new Vector2(0f, -60f),
            capSO.cienciaReal, 9f, new Color(0.8f, 1f, 0.8f), TextAlignmentOptions.TopLeft);

        // Separador
        CrearUIPanel("Sep3", panelRoot.transform, new Vector2(460f, 2f), new Vector2(0f, -100f),
            new Color(0.3f, 0.5f, 0.8f, 0.8f));

        // ── Tarjeta 4: PISTAS ─────────────────────────────────────────────────
        // Título de sección
        CrearTMPUGUI("TxtPistasLabel", panelRoot.transform,
            new Vector2(460f, 22f), new Vector2(0f, -115f),
            "PISTAS — toca cada ??? para revelar", 10f,
            new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Left);

        // Texto acumulador de pistas reveladas
        var txtPistas = CrearTMPUGUI("TxtPistas", panelRoot.transform,
            new Vector2(460f, 60f), new Vector2(0f, -155f),
            "???\n???\n???", 10f, Color.white, TextAlignmentOptions.TopLeft);

        // ── 3 PistaRevelable invisibles sobre el texto de pistas ──────────────
        // Cada una cubre una línea del txtPistas (~20px de alto en canvas)
        var pistasRevelables = new PistaRevelable[3];
        float[] pistasY = { -135f, -155f, -175f };

        for (int i = 0; i < 3; i++)
        {
            var pistaGO = new GameObject($"Pista_{i}");
            pistaGO.transform.SetParent(panelRoot.transform, false);

            var rectPista = pistaGO.AddComponent<RectTransform>();
            rectPista.sizeDelta        = new Vector2(460f, 22f);
            rectPista.anchoredPosition = new Vector2(0f, pistasY[i]);

            // Imagen transparente para que el raycast la detecte
            var img = pistaGO.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.01f); // casi invisible pero raycasteable

            // XRSimpleInteractable requiere Collider → usamos BoxCollider en world-space
            // Para UI world-space usamos el GraphicRaycaster del canvas + XRSimpleInteractable
            var interactable = pistaGO.AddComponent<XRSimpleInteractable>();

            // AudioSource para el beep
            var audio = pistaGO.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.playOnAwake  = false;

            var pr = pistaGO.AddComponent<PistaRevelable>();

            // Asignar via SerializedObject
            var so = new SerializedObject(pr);
            so.FindProperty("textoOculto").stringValue = "???";
            so.FindProperty("textoReal").stringValue   = capSO.pistas[i];
            so.FindProperty("etiqueta").objectReferenceValue = txtPistas; // todas apuntan al mismo TMP
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.ApplyModifiedProperties();

            pistasRevelables[i] = pr;
        }

        // ── Asignar todo a TablaHeroes ────────────────────────────────────────
        var soTabla = new SerializedObject(tablaComp);
        soTabla.FindProperty("txtFicha").objectReferenceValue   = txtFicha;
        soTabla.FindProperty("txtOrigen").objectReferenceValue  = txtOrigen;
        soTabla.FindProperty("txtCiencia").objectReferenceValue = txtCiencia;
        soTabla.FindProperty("txtPistas").objectReferenceValue  = txtPistas;
        var propPistas = soTabla.FindProperty("pistasRevelables");
        propPistas.arraySize = 3;
        for (int i = 0; i < 3; i++)
            propPistas.GetArrayElementAtIndex(i).objectReferenceValue = pistasRevelables[i];
        soTabla.ApplyModifiedProperties();

        // Cargar el capítulo en la tabla para que muestre los textos
        tablaComp.CargarCapitulo(capSO);

        Debug.Log("[Construir] ✅ TablaHeroes construida con 4 tarjetas y 3 pistas.");

        // ── PASO 5: Bandejas de clasificación ─────────────────────────────────
        // Posición base: frente a la estantería de ampollas
        // Estantería está en ~(-4, 0.9, -0.4) → bandejas en la mesa cercana
        // Usamos la posición de la mesa de la escena
        var mesaGO = GameObject.Find("table_2") ?? GameObject.Find("CR_Lab2_Desk_01");
        Vector3 mesaPos = mesaGO != null ? mesaGO.transform.position : new Vector3(-2f, 0.85f, 0f);

        // Destruir bandejas anteriores si existen
        foreach (string nb in new[] { "Bandeja_Elemento", "Bandeja_Compuesto", "Bandeja_Biologico" })
        {
            var vieja = GameObject.Find(nb);
            if (vieja != null) Object.DestroyImmediate(vieja);
        }

        ChemicalType[] tipos   = { ChemicalType.Elemento, ChemicalType.Compuesto, ChemicalType.BiologicoORadiactivo };
        string[]  nombresBand  = { "Bandeja_Elemento", "Bandeja_Compuesto", "Bandeja_Biologico" };
        string[]  labelsBand   = { "ELEMENTO", "COMPUESTO", "BIO/RAD" };
        Color[]   coloresBand  = { new Color(0.9f, 0.8f, 0.1f), new Color(0.1f, 0.5f, 0.9f), new Color(0.7f, 0.2f, 0.8f) };

        var bandejas = new ClassificationTray[3];

        for (int i = 0; i < 3; i++)
        {
            // Raíz de la bandeja
            var bandeja = new GameObject(nombresBand[i]);
            bandeja.transform.position = mesaPos + new Vector3(-0.35f + i * 0.35f, 0.05f, 0.3f);

            // Base visual: cubo plano de color
            var base3D = GameObject.CreatePrimitive(PrimitiveType.Cube);
            base3D.name = "Base";
            base3D.transform.SetParent(bandeja.transform, false);
            base3D.transform.localPosition = Vector3.zero;
            base3D.transform.localScale    = new Vector3(0.28f, 0.025f, 0.22f);
            var matBase = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matBase.color = coloresBand[i] * 0.6f;
            base3D.GetComponent<Renderer>().sharedMaterial = matBase;
            Object.DestroyImmediate(base3D.GetComponent<Collider>()); // el socket tiene su propio collider

            // Piloto (esfera indicadora)
            var piloto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piloto.name = "Piloto";
            piloto.transform.SetParent(bandeja.transform, false);
            piloto.transform.localPosition = new Vector3(0f, 0.04f, -0.08f);
            piloto.transform.localScale    = Vector3.one * 0.04f;
            var matPiloto = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            matPiloto.color = Color.gray;
            piloto.GetComponent<Renderer>().sharedMaterial = matPiloto;
            Object.DestroyImmediate(piloto.GetComponent<Collider>());

            // Etiqueta TMP world-space
            var lblGO = new GameObject("Etiqueta");
            lblGO.transform.SetParent(bandeja.transform, false);
            lblGO.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            lblGO.transform.localScale    = Vector3.one * 0.003f;
            var lblCanvas = lblGO.AddComponent<Canvas>();
            lblCanvas.renderMode = RenderMode.WorldSpace;
            var lblRect = lblGO.GetComponent<RectTransform>();
            lblRect.sizeDelta = new Vector2(80f, 20f);
            var lblTxt = new GameObject("Txt");
            lblTxt.transform.SetParent(lblGO.transform, false);
            var lblTxtRect = lblTxt.AddComponent<RectTransform>();
            lblTxtRect.anchorMin = Vector2.zero;
            lblTxtRect.anchorMax = Vector2.one;
            lblTxtRect.offsetMin = lblTxtRect.offsetMax = Vector2.zero;
            var tmp = lblTxt.AddComponent<TextMeshProUGUI>();
            tmp.text      = labelsBand[i];
            tmp.fontSize  = 14f;
            tmp.color     = coloresBand[i];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            // AudioSource
            var audio = bandeja.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.playOnAwake  = false;

            // XRSocketInteractor en la raíz
            var socket = bandeja.AddComponent<XRSocketInteractor>();

            // ClassificationTray
            var tray = bandeja.AddComponent<ClassificationTray>();
            var soTray = new SerializedObject(tray);
            soTray.FindProperty("tipoEsperado").enumValueIndex = (int)tipos[i];
            soTray.FindProperty("piloto").objectReferenceValue = piloto.GetComponent<Renderer>();
            soTray.FindProperty("audioSource").objectReferenceValue = audio;
            soTray.ApplyModifiedProperties();

            bandejas[i] = tray;
        }

        Debug.Log("[Construir] ✅ 3 bandejas de clasificación creadas.");

        // ── Conectar al Cap1Manager ───────────────────────────────────────────
        var cap1GO   = GameObject.Find("Cap1Manager");
        var cap1Comp = cap1GO?.GetComponent<Cap1Manager>();
        if (cap1Comp != null)
        {
            var soCap1 = new SerializedObject(cap1Comp);
            soCap1.FindProperty("tablaHeroes").objectReferenceValue = tablaComp;
            var propBandejas = soCap1.FindProperty("bandejas");
            propBandejas.arraySize = 3;
            for (int i = 0; i < 3; i++)
                propBandejas.GetArrayElementAtIndex(i).objectReferenceValue = bandejas[i];
            soCap1.ApplyModifiedProperties();
            Debug.Log("[Construir] ✅ Cap1Manager actualizado con TablaHeroes y bandejas.");
        }
        else
        {
            Debug.LogWarning("[Construir] Cap1Manager no encontrado — conecta manualmente.");
        }

        // ── Guardar escena ────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();
        Debug.Log("[Construir] ✅ Escena 01_Laboratorio guardada.");
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────

    static GameObject CrearUIPanel(string nombre, Transform padre,
        Vector2 size, Vector2 anchoredPos, Color color)
    {
        var go   = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = anchoredPos;
        var img  = go.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        return go;
    }

    static TextMeshProUGUI CrearTMPUGUI(string nombre, Transform padre,
        Vector2 size, Vector2 anchoredPos, string texto,
        float fontSize, Color color, TextAlignmentOptions align)
    {
        var go   = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = anchoredPos;
        var tmp  = go.AddComponent<TextMeshProUGUI>();
        tmp.text            = texto;
        tmp.fontSize        = fontSize;
        tmp.color           = color;
        tmp.alignment       = align;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        tmp.overflowMode    = TMPro.TextOverflowModes.Ellipsis;
        return tmp;
    }
}
