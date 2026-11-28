/*
 * Script de editor: crea y ensambla la escena Test_Cap1 con todas las primitivas,
 * prefabs reutilizados y scripts del Capítulo 1 (Spider-Man).
 * Ejecutar desde Tools > Crear Escena Test_Cap1
 */
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;

public static class CrearEscenaCap1
{
    const string ESCENA_PATH  = "Assets/_Project/Scenes/Tests/Test_Cap1.unity";
    const string PREFABS_PATH = "Assets/_Project/Prefabs/";
    const string SO_PATH      = "Assets/_Project/ScriptableObjects/Cap1/";
    const string MANAGERS_PREFAB = "Assets/Proyecto/Prefabs/_Managers.prefab";
    const string XR_PREFAB    = "Assets/Proyecto/Prefabs/XR Origin (VR) 1.prefab";
    const string AMPOLLA_PREFAB = "Assets/Proyecto/Prefabs/Ampolla.prefab";
    const string NOVA_PREFAB  = "Assets/Proyecto/Prefabs/Nova.prefab";
    const string PANEL_PREFAB = "Assets/Proyecto/Prefabs/PanelUI.prefab";
    const string BOTON_PREFAB = "Assets/Proyecto/Prefabs/BotonLab.prefab";
    const string PALANCA_PREFAB = "Assets/Proyecto/Prefabs/PalancaLab.prefab";
    const string MATRAZ_PREFAB = "Assets/Proyecto/Prefabs/Matraz.prefab";

    [MenuItem("Tools/Crear Escena Test_Cap1")]
    public static void Execute()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Tests");
        Directory.CreateDirectory(PREFABS_PATH);

        // ── Asegurar tags necesarios ─────────────────────────────────────────
        AsegurarTag("Pinzas");

        // ── Crear / abrir escena ─────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Luz direccional ──────────────────────────────────────────────────
        var luzGO = new GameObject("Directional Light");
        var luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.intensity = 1f;
        luzGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Piso ─────────────────────────────────────────────────────────────
        var piso = GameObject.CreatePrimitive(PrimitiveType.Plane);
        piso.name = "Piso";
        piso.transform.localScale = new Vector3(3f, 1f, 3f);
        SetColor(piso, new Color(0.3f, 0.3f, 0.35f));

        // ── Mesa central ─────────────────────────────────────────────────────
        var mesa = CrearCubo("Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 0.8f),
            new Color(0.55f, 0.4f, 0.25f));

        // ── _Managers ────────────────────────────────────────────────────────
        var managersPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MANAGERS_PREFAB);
        if (managersPrefab != null)
            PrefabUtility.InstantiatePrefab(managersPrefab);
        else
            Debug.LogWarning("[CrearEscenaCap1] No se encontró _Managers.prefab");

        // ── XR Origin ────────────────────────────────────────────────────────
        var xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_PREFAB);
        if (xrPrefab != null)
        {
            var xr = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab);
            xr.name = "XR Origin (VR)";
            xr.transform.position = new Vector3(0f, 0f, -2f);
        }
        else Debug.LogWarning("[CrearEscenaCap1] No se encontró XR Origin prefab");

        // ── NOVA ─────────────────────────────────────────────────────────────
        var novaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NOVA_PREFAB);
        if (novaPrefab != null)
        {
            var nova = (GameObject)PrefabUtility.InstantiatePrefab(novaPrefab);
            nova.transform.position = new Vector3(0f, 1.6f, 1.5f);
        }

        // ── Cargar SOs ───────────────────────────────────────────────────────
        var soSalina  = AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_Salina.asset");
        var soADN     = AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_ADN_Arana.asset");
        var soTritio  = AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_Tritio.asset");
        var soMercurio= AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_Mercurio.asset");
        var soCloro   = AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_Cloro.asset");
        var soCafeina = AssetDatabase.LoadAssetAtPath<ChemicalSO>(SO_PATH + "Chemical_Cafeina.asset");
        var receta    = AssetDatabase.LoadAssetAtPath<RecipeSO>(SO_PATH + "Recipe_Spider.asset");
        var capitulo  = AssetDatabase.LoadAssetAtPath<ChapterSO>(SO_PATH + "Chapter_Spider.asset");

        // ── PASO 2: Estantería con 6 ampollas ────────────────────────────────
        var estanteria = CrearCubo("Estanteria", new Vector3(-3f, 0.6f, 0f),
            new Vector3(0.3f, 1.2f, 2.5f), new Color(0.6f, 0.5f, 0.35f));

        var ampollaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AMPOLLA_PREFAB);
        ChemicalSO[] quimicos = { soSalina, soSalina, soADN, soTritio, soMercurio, soCloro };
        // soSalina x2 (llenas), soADN (requiresProcessing), soTritio, soMercurio, soCloro
        // Las 2 últimas (índice 4 y 5) se dejan vacías para probar E1
        bool[] vacias = { false, false, false, false, true, true };

        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = new Vector3(-2.7f, 1.0f + i * 0.18f, -0.9f + i * 0.36f);
            GameObject ampolla;
            if (ampollaPrefab != null)
            {
                ampolla = (GameObject)PrefabUtility.InstantiatePrefab(ampollaPrefab);
                ampolla.transform.position = pos;
            }
            else
            {
                ampolla = CrearCubo($"Ampolla_{i}", pos, new Vector3(0.06f, 0.15f, 0.06f), Color.white);
            }
            ampolla.name = $"Ampolla_{i}";

            var cont = ampolla.GetComponent<ChemicalContainer>();
            if (cont != null && i < quimicos.Length && quimicos[i] != null)
            {
                cont.sustancia = quimicos[i];
                cont.volumen   = vacias[i] ? 0 : 10;
                cont.Refrescar();
            }
        }

        // ── PASO 3: Lector de cómics ─────────────────────────────────────────
        var socketComicGO = new GameObject("SocketComic");
        socketComicGO.transform.position = new Vector3(1.5f, 0.85f, 0f);
        var socketComic = socketComicGO.AddComponent<XRSocketInteractor>();
        var audioSocketComic = socketComicGO.AddComponent<AudioSource>();
        audioSocketComic.spatialBlend = 1f;
        var comicReader = socketComicGO.AddComponent<ComicReader>();

        // Cómic: cubo plano agarrable
        var comicGO = CrearCubo("Comic_AmazingFantasy15",
            new Vector3(1.5f, 0.85f, 0.5f), new Vector3(0.15f, 0.2f, 0.02f),
            new Color(0.9f, 0.2f, 0.1f));
        comicGO.AddComponent<XRGrabInteractable>();
        // Etiqueta
        AgregarEtiqueta(comicGO, "Amazing Fantasy #15", new Vector3(0f, 0.12f, 0.02f));

        // ── PASO 4: Tabla de héroes ───────────────────────────────────────────
        var tablaGO = new GameObject("TablaHeroes");
        tablaGO.transform.position = new Vector3(2f, 1.4f, 0f);
        tablaGO.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        var panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_PREFAB);
        GameObject tablaPanel;
        if (panelPrefab != null)
        {
            tablaPanel = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab);
            tablaPanel.transform.SetParent(tablaGO.transform, false);
        }
        else
        {
            tablaPanel = CrearCubo("TablaPanel", Vector3.zero, new Vector3(0.8f, 0.6f, 0.02f),
                new Color(0.1f, 0.1f, 0.2f));
            tablaPanel.transform.SetParent(tablaGO.transform, false);
        }

        var tablaHeroes = tablaGO.AddComponent<TablaHeroes>();

        // 3 pistas revelables como cubos pequeños tocables
        var pistasRevelables = new PistaRevelable[3];
        string[] textosPistas = capitulo != null ? capitulo.pistas : new[] { "???", "???", "???" };
        for (int i = 0; i < 3; i++)
        {
            var pistaGO = CrearCubo($"Pista_{i}", new Vector3(1.8f + i * 0.3f, 1.2f, 0f),
                new Vector3(0.12f, 0.06f, 0.02f), new Color(0.2f, 0.5f, 0.8f));
            pistaGO.AddComponent<XRSimpleInteractable>();
            var audioP = pistaGO.AddComponent<AudioSource>();
            audioP.spatialBlend = 1f;
            var pr = pistaGO.AddComponent<PistaRevelable>();
            AgregarEtiqueta(pistaGO, "???", new Vector3(0f, 0.04f, 0.015f));
            pistasRevelables[i] = pr;
        }

        // Asignar pistas a TablaHeroes via SerializedObject
        var soTabla = new SerializedObject(tablaHeroes);
        var propPistas = soTabla.FindProperty("pistasRevelables");
        propPistas.arraySize = 3;
        for (int i = 0; i < 3; i++)
            propPistas.GetArrayElementAtIndex(i).objectReferenceValue = pistasRevelables[i];
        soTabla.ApplyModifiedProperties();

        // ── PASO 5: Bandejas de clasificación ────────────────────────────────
        ChemicalType[] tipos = { ChemicalType.Elemento, ChemicalType.Compuesto, ChemicalType.BiologicoORadiactivo };
        string[] nombresBandejas = { "Bandeja_Elemento", "Bandeja_Compuesto", "Bandeja_Biologico" };
        Color[] coloresBandejas = { new Color(0.8f, 0.8f, 0.2f), new Color(0.2f, 0.6f, 0.9f), new Color(0.8f, 0.3f, 0.8f) };
        var bandejas = new ClassificationTray[3];

        for (int i = 0; i < 3; i++)
        {
            var bandeja = CrearCubo(nombresBandejas[i],
                new Vector3(-0.6f + i * 0.6f, 0.82f, 0.5f),
                new Vector3(0.4f, 0.05f, 0.3f), coloresBandejas[i]);
            var socketB = bandeja.AddComponent<XRSocketInteractor>();
            var audioB  = bandeja.AddComponent<AudioSource>();
            audioB.spatialBlend = 1f;
            var pilotoB = CrearCubo("Piloto", new Vector3(0f, 0.05f, 0f),
                new Vector3(0.05f, 0.05f, 0.05f), Color.gray);
            pilotoB.transform.SetParent(bandeja.transform, false);
            var tray = bandeja.AddComponent<ClassificationTray>();
            var soTray = new SerializedObject(tray);
            soTray.FindProperty("tipoEsperado").enumValueIndex = (int)tipos[i];
            soTray.FindProperty("piloto").objectReferenceValue = pilotoB.GetComponent<Renderer>();
            soTray.FindProperty("audioSource").objectReferenceValue = audioB;
            soTray.ApplyModifiedProperties();
            AgregarEtiqueta(bandeja, nombresBandejas[i].Replace("Bandeja_", ""), new Vector3(0f, 0.06f, 0f));
            bandejas[i] = tray;
        }

        // ── PASO 6: Extractor de ADN ─────────────────────────────────────────
        var extractorGO = new GameObject("DNAExtractor");
        extractorGO.transform.position = new Vector3(-1.5f, 0.85f, 0.5f);
        var extractor = extractorGO.AddComponent<DNAExtractor>();

        // 4 sockets en orden
        string[] nombresSocket = { "Socket_Arana", "Socket_Detergente", "Socket_Sal", "Socket_Alcohol" };
        Color[] coloresSocket  = { Color.green, Color.cyan, Color.white, new Color(0.5f, 0.8f, 1f) };
        var socketsADN = new XRSocketInteractor[4];
        var pilotosADN = new Renderer[4];
        for (int i = 0; i < 4; i++)
        {
            var sGO = CrearCubo(nombresSocket[i],
                new Vector3(-1.7f + i * 0.15f, 0.9f, 0.5f),
                new Vector3(0.1f, 0.1f, 0.1f), coloresSocket[i]);
            socketsADN[i] = sGO.AddComponent<XRSocketInteractor>();
            var pilotoADN = CrearCubo("Piloto", new Vector3(0f, 0.08f, 0f),
                new Vector3(0.04f, 0.04f, 0.04f), Color.gray);
            pilotoADN.transform.SetParent(sGO.transform, false);
            pilotosADN[i] = pilotoADN.GetComponent<Renderer>();
            AgregarEtiqueta(sGO, nombresSocket[i].Replace("Socket_", ""), new Vector3(0f, 0.1f, 0f));
        }

        // Varilla
        var varillaGO = CrearCubo("Varilla",
            new Vector3(-1.5f, 0.95f, 0.7f), new Vector3(0.03f, 0.2f, 0.03f),
            new Color(0.7f, 0.7f, 0.7f));
        var varillaGrab = varillaGO.AddComponent<XRGrabInteractable>();

        // Punto de spawn del ADN
        var spawnADN = new GameObject("SpawnADN");
        spawnADN.transform.position = new Vector3(-1.5f, 1.1f, 0.5f);
        spawnADN.transform.SetParent(extractorGO.transform, false);

        // Asignar via SerializedObject
        var soExt = new SerializedObject(extractor);
        var propSockets = soExt.FindProperty("sockets");
        propSockets.arraySize = 4;
        for (int i = 0; i < 4; i++)
            propSockets.GetArrayElementAtIndex(i).objectReferenceValue = socketsADN[i];
        soExt.FindProperty("varilla").objectReferenceValue = varillaGrab;
        soExt.FindProperty("adnSO").objectReferenceValue = soADN;
        soExt.FindProperty("ampollaPrefab").objectReferenceValue = ampollaPrefab;
        soExt.FindProperty("puntoSpawn").objectReferenceValue = spawnADN.transform;
        var propPilotosADN = soExt.FindProperty("pilotosOrden");
        propPilotosADN.arraySize = 4;
        for (int i = 0; i < 4; i++)
            propPilotosADN.GetArrayElementAtIndex(i).objectReferenceValue = pilotosADN[i];
        soExt.ApplyModifiedProperties();

        // ── PASO 7: Cámara de tritio ──────────────────────────────────────────
        var camaraGO = CrearCubo("CamaraTritio",
            new Vector3(0.5f, 0.85f, 0.8f), new Vector3(0.3f, 0.25f, 0.25f),
            new Color(0.3f, 0.3f, 0.3f));
        var tapaGO = CrearCubo("Tapa",
            new Vector3(0f, 0.13f, 0f), new Vector3(0.3f, 0.02f, 0.25f),
            new Color(0.5f, 0.5f, 0.5f));
        tapaGO.transform.SetParent(camaraGO.transform, false);

        // Ampolla de tritio dentro
        GameObject ampollaTritiioGO;
        if (ampollaPrefab != null)
        {
            ampollaTritiioGO = (GameObject)PrefabUtility.InstantiatePrefab(ampollaPrefab);
            ampollaTritiioGO.transform.position = new Vector3(0.5f, 0.9f, 0.8f);
        }
        else
        {
            ampollaTritiioGO = CrearCubo("Ampolla_Tritio",
                new Vector3(0.5f, 0.9f, 0.8f), new Vector3(0.06f, 0.15f, 0.06f), Color.green);
        }
        ampollaTritiioGO.name = "Ampolla_Tritio";
        var contTritio = ampollaTritiioGO.GetComponent<ChemicalContainer>();
        if (contTritio != null && soTritio != null)
        {
            contTritio.sustancia = soTritio;
            contTritio.volumen   = 10;
            contTritio.Refrescar();
        }
        var grabTritio = ampollaTritiioGO.GetComponent<XRGrabInteractable>()
                         ?? ampollaTritiioGO.AddComponent<XRGrabInteractable>();

        // Pinzas (2 cubos delgados con tag "Pinzas")
        for (int i = 0; i < 2; i++)
        {
            var pinza = CrearCubo($"Pinza_{i}",
                new Vector3(0.7f + i * 0.08f, 0.85f, 0.8f),
                new Vector3(0.02f, 0.15f, 0.02f), new Color(0.8f, 0.8f, 0.2f));
            pinza.tag = "Pinzas";
            pinza.AddComponent<XRGrabInteractable>();
        }

        var pilotoCamara = CrearCubo("Piloto",
            new Vector3(0f, 0.14f, -0.14f), new Vector3(0.04f, 0.04f, 0.04f), Color.white);
        pilotoCamara.transform.SetParent(camaraGO.transform, false);
        var audioCamara = camaraGO.AddComponent<AudioSource>();
        audioCamara.spatialBlend = 1f;

        var chamber = camaraGO.AddComponent<TritiumChamber>();
        var soChamber = new SerializedObject(chamber);
        soChamber.FindProperty("ampollaTritiio").objectReferenceValue = grabTritio;
        soChamber.FindProperty("tapa").objectReferenceValue = tapaGO.transform;
        soChamber.FindProperty("piloto").objectReferenceValue = pilotoCamara.GetComponent<Renderer>();
        soChamber.FindProperty("audioSource").objectReferenceValue = audioCamara;
        soChamber.ApplyModifiedProperties();

        // ── PASO 8: Matraz de mezclas ─────────────────────────────────────────
        var matrazPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MATRAZ_PREFAB);
        GameObject matrazGO;
        if (matrazPrefab != null)
        {
            matrazGO = (GameObject)PrefabUtility.InstantiatePrefab(matrazPrefab);
            matrazGO.transform.position = new Vector3(0f, 0.85f, 0f);
        }
        else
        {
            matrazGO = CrearCubo("Matraz", new Vector3(0f, 0.85f, 0f),
                new Vector3(0.15f, 0.2f, 0.15f), new Color(0.7f, 0.9f, 1f));
        }
        matrazGO.name = "Matraz_Mezcla";

        // Buscar MixingFlask en el matraz (puede estar en un hijo)
        var mixingFlask = matrazGO.GetComponentInChildren<MixingFlask>();
        if (mixingFlask == null) mixingFlask = matrazGO.AddComponent<MixingFlask>();

        // Líquido visual (cilindro hijo)
        var liquidoGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        liquidoGO.name = "Liquido";
        liquidoGO.transform.SetParent(matrazGO.transform, false);
        liquidoGO.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        liquidoGO.transform.localScale    = new Vector3(0.08f, 0.01f, 0.08f);
        SetColor(liquidoGO, Color.clear);
        Object.DestroyImmediate(liquidoGO.GetComponent<Collider>());

        var soFlask = new SerializedObject(mixingFlask);
        soFlask.FindProperty("receta").objectReferenceValue = receta;
        soFlask.FindProperty("liquidoVisual").objectReferenceValue = liquidoGO.GetComponent<Renderer>();
        soFlask.ApplyModifiedProperties();

        // ── PASO 9: Reactor (temperatura) ────────────────────────────────────
        var palancaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PALANCA_PREFAB);
        GameObject palancaGO;
        if (palancaPrefab != null)
        {
            palancaGO = (GameObject)PrefabUtility.InstantiatePrefab(palancaPrefab);
            palancaGO.transform.position = new Vector3(-0.8f, 0.85f, -0.3f);
        }
        else
        {
            palancaGO = CrearCubo("PalancaReactor", new Vector3(-0.8f, 0.85f, -0.3f),
                new Vector3(0.1f, 0.3f, 0.1f), new Color(0.4f, 0.4f, 0.8f));
        }
        palancaGO.name = "PalancaReactor";
        var palancaComp = palancaGO.GetComponentInChildren<PalancaLab>();

        var reactorGO = new GameObject("ReactorController");
        reactorGO.transform.position = new Vector3(-0.8f, 1.1f, -0.3f);
        var reactorCtrl = reactorGO.AddComponent<ReactorController>();

        // Termómetro visual
        var termoGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        termoGO.name = "Termometro";
        termoGO.transform.position = new Vector3(-0.8f, 1.0f, -0.1f);
        termoGO.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        SetColor(termoGO, Color.blue);

        // Texto temperatura
        var txtTempGO = new GameObject("TxtTemperatura");
        txtTempGO.transform.position = new Vector3(-0.8f, 1.3f, -0.1f);
        var txtTemp = txtTempGO.AddComponent<TextMeshPro>();
        txtTemp.text = "20.0°C";
        txtTemp.fontSize = 0.15f;
        txtTemp.alignment = TextAlignmentOptions.Center;
        txtTempGO.AddComponent<Billboard>();

        var soReactor = new SerializedObject(reactorCtrl);
        if (palancaComp != null)
            soReactor.FindProperty("palanca").objectReferenceValue = palancaComp;
        soReactor.FindProperty("receta").objectReferenceValue = receta;
        soReactor.FindProperty("termometroRenderer").objectReferenceValue = termoGO.GetComponent<Renderer>();
        soReactor.FindProperty("txtTemperatura").objectReferenceValue = txtTemp;
        soReactor.ApplyModifiedProperties();

        // ── PASO 10: Botón ANALIZAR ───────────────────────────────────────────
        var botonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BOTON_PREFAB);
        GameObject botonAnalizarGO;
        if (botonPrefab != null)
        {
            botonAnalizarGO = (GameObject)PrefabUtility.InstantiatePrefab(botonPrefab);
            botonAnalizarGO.transform.position = new Vector3(0.6f, 0.85f, -0.3f);
        }
        else
        {
            botonAnalizarGO = CrearCubo("BotonAnalizar",
                new Vector3(0.6f, 0.85f, -0.3f), new Vector3(0.1f, 0.08f, 0.1f),
                new Color(0.2f, 0.8f, 0.2f));
        }
        botonAnalizarGO.name = "BotonAnalizar";
        AgregarEtiqueta(botonAnalizarGO, "ANALIZAR", new Vector3(0f, 0.06f, 0f));
        var botonAnalizarComp = botonAnalizarGO.GetComponentInChildren<BotonLab>();

        // Piloto resultado
        var pilotoResultado = CrearCubo("PilotoResultado",
            new Vector3(0.6f, 0.95f, -0.3f), new Vector3(0.05f, 0.05f, 0.05f), Color.gray);

        // Texto resultado
        var txtResGO = new GameObject("TxtResultado");
        txtResGO.transform.position = new Vector3(0.6f, 1.05f, -0.3f);
        var txtRes = txtResGO.AddComponent<TextMeshPro>();
        txtRes.text = "---";
        txtRes.fontSize = 0.12f;
        txtRes.alignment = TextAlignmentOptions.Center;
        txtResGO.AddComponent<Billboard>();

        var validatorGO = new GameObject("RecipeValidator");
        validatorGO.transform.position = new Vector3(0.6f, 0.85f, -0.3f);
        var validator = validatorGO.AddComponent<RecipeValidator>();
        var soValidator = new SerializedObject(validator);
        soValidator.FindProperty("matraz").objectReferenceValue = mixingFlask;
        soValidator.FindProperty("reactor").objectReferenceValue = reactorCtrl;
        soValidator.FindProperty("receta").objectReferenceValue = receta;
        soValidator.FindProperty("pilotoResultado").objectReferenceValue = pilotoResultado.GetComponent<Renderer>();
        soValidator.FindProperty("txtResultado").objectReferenceValue = txtRes;
        soValidator.ApplyModifiedProperties();

        // ── PASO 11: Evento EMP ───────────────────────────────────────────────
        // Luz de emergencia (roja)
        var luzEmergGO = new GameObject("LuzEmergencia");
        luzEmergGO.transform.position = new Vector3(0f, 3f, 0f);
        var luzEmerg = luzEmergGO.AddComponent<Light>();
        luzEmerg.type = LightType.Point;
        luzEmerg.color = Color.red;
        luzEmerg.intensity = 2f;
        luzEmerg.enabled = false;

        // Linterna
        var linternaGO = new GameObject("Linterna");
        linternaGO.transform.position = new Vector3(0f, 0.85f, -0.8f);
        var linternaBody = CrearCubo("Cuerpo",
            Vector3.zero, new Vector3(0.05f, 0.05f, 0.15f), new Color(0.3f, 0.3f, 0.3f));
        linternaBody.transform.SetParent(linternaGO.transform, false);
        var luzLinternaGO = new GameObject("LuzSpot");
        luzLinternaGO.transform.SetParent(linternaGO.transform, false);
        luzLinternaGO.transform.localPosition = new Vector3(0f, 0f, 0.08f);
        var luzLinterna = luzLinternaGO.AddComponent<Light>();
        luzLinterna.type = LightType.Spot;
        luzLinterna.intensity = 3f;
        luzLinterna.spotAngle = 30f;
        luzLinterna.enabled = false;
        linternaGO.AddComponent<XRGrabInteractable>();
        linternaGO.SetActive(false);

        // Botón generador
        GameObject botonGenGO;
        if (botonPrefab != null)
        {
            botonGenGO = (GameObject)PrefabUtility.InstantiatePrefab(botonPrefab);
            botonGenGO.transform.position = new Vector3(-1f, 0.85f, -0.8f);
        }
        else
        {
            botonGenGO = CrearCubo("BotonGenerador",
                new Vector3(-1f, 0.85f, -0.8f), new Vector3(0.1f, 0.08f, 0.1f), Color.red);
        }
        botonGenGO.name = "BotonGenerador";
        AgregarEtiqueta(botonGenGO, "GENERADOR", new Vector3(0f, 0.06f, 0f));
        var botonGenComp = botonGenGO.GetComponentInChildren<BotonLab>();

        var eventDirGO = new GameObject("EventDirector");
        var eventDir = eventDirGO.AddComponent<EventDirector>();
        var soEventDir = new SerializedObject(eventDir);
        var propLuces = soEventDir.FindProperty("lucesNormales");
        propLuces.arraySize = 1;
        propLuces.GetArrayElementAtIndex(0).objectReferenceValue = luz;
        soEventDir.FindProperty("luzEmergencia").objectReferenceValue = luzEmerg;
        soEventDir.FindProperty("linternaGO").objectReferenceValue = linternaGO;
        soEventDir.FindProperty("luzLinterna").objectReferenceValue = luzLinterna;
        if (botonGenComp != null)
            soEventDir.FindProperty("botonGenerador").objectReferenceValue = botonGenComp;
        soEventDir.ApplyModifiedProperties();

        // ── PASO 12: Dron Xentari ─────────────────────────────────────────────
        var dronGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dronGO.name = "DronXentari";
        dronGO.transform.position = new Vector3(0f, 2f, 0f);
        dronGO.transform.localScale = Vector3.one * 0.15f;
        SetColor(dronGO, Color.red);
        var luzDron = dronGO.AddComponent<Light>();
        luzDron.type = LightType.Point;
        luzDron.color = Color.red;
        luzDron.intensity = 1.5f;
        luzDron.range = 3f;

        // Canvas de aviso world-space
        var canvasAvisoGO = new GameObject("CanvasAviso");
        canvasAvisoGO.transform.SetParent(dronGO.transform, false);
        canvasAvisoGO.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        var canvasAviso = canvasAvisoGO.AddComponent<Canvas>();
        canvasAviso.renderMode = RenderMode.WorldSpace;
        canvasAvisoGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        var rectAviso = canvasAvisoGO.GetComponent<RectTransform>();
        rectAviso.sizeDelta = new Vector2(0.5f, 0.15f);
        var txtAvisoGO = new GameObject("TxtAviso");
        txtAvisoGO.transform.SetParent(canvasAvisoGO.transform, false);
        var txtAviso = txtAvisoGO.AddComponent<TextMeshProUGUI>();
        txtAviso.text = "⚠ DRON XENTARI ⚠";
        txtAviso.fontSize = 18f;
        txtAviso.color = Color.red;
        txtAviso.alignment = TextAlignmentOptions.Center;
        var rectTxtAviso = txtAvisoGO.GetComponent<RectTransform>();
        rectTxtAviso.anchorMin = Vector2.zero;
        rectTxtAviso.anchorMax = Vector2.one;
        rectTxtAviso.offsetMin = rectTxtAviso.offsetMax = Vector2.zero;

        var dronComp = dronGO.AddComponent<DronXentari>();
        var soDron = new SerializedObject(dronComp);
        soDron.FindProperty("luzDron").objectReferenceValue = luzDron;
        soDron.FindProperty("canvasAviso").objectReferenceValue = canvasAviso;
        soDron.FindProperty("txtAviso").objectReferenceValue = txtAviso;
        soDron.ApplyModifiedProperties();

        // ── PASO 13: Quiz ─────────────────────────────────────────────────────
        var quizGO = new GameObject("QuizPanel");
        quizGO.transform.position = new Vector3(0f, 1.5f, 1.8f);
        quizGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Panel visual
        var quizPanel = CrearCubo("FondoQuiz", Vector3.zero,
            new Vector3(0.8f, 0.5f, 0.02f), new Color(0.05f, 0.05f, 0.15f));
        quizPanel.transform.SetParent(quizGO.transform, false);

        // Texto pregunta
        var txtPregGO = new GameObject("TxtPregunta");
        txtPregGO.transform.SetParent(quizGO.transform, false);
        txtPregGO.transform.localPosition = new Vector3(0f, 0.15f, -0.015f);
        var txtPreg = txtPregGO.AddComponent<TextMeshPro>();
        txtPreg.text = "Pregunta...";
        txtPreg.fontSize = 0.07f;
        txtPreg.alignment = TextAlignmentOptions.Center;
        txtPreg.rectTransform.sizeDelta = new Vector2(0.7f, 0.1f);

        // Texto feedback
        var txtFbGO = new GameObject("TxtFeedback");
        txtFbGO.transform.SetParent(quizGO.transform, false);
        txtFbGO.transform.localPosition = new Vector3(0f, -0.2f, -0.015f);
        var txtFb = txtFbGO.AddComponent<TextMeshPro>();
        txtFb.text = "";
        txtFb.fontSize = 0.06f;
        txtFb.alignment = TextAlignmentOptions.Center;
        txtFb.rectTransform.sizeDelta = new Vector2(0.7f, 0.08f);

        // 3 botones de opción
        var botonesQuiz = new BotonLab[3];
        var txtOpciones = new TMP_Text[3];
        Color[] coloresBtn = { new Color(0.2f, 0.5f, 0.9f), new Color(0.2f, 0.7f, 0.3f), new Color(0.8f, 0.4f, 0.1f) };
        for (int i = 0; i < 3; i++)
        {
            GameObject btnGO;
            if (botonPrefab != null)
            {
                btnGO = (GameObject)PrefabUtility.InstantiatePrefab(botonPrefab);
                btnGO.transform.SetParent(quizGO.transform, false);
                btnGO.transform.localPosition = new Vector3(-0.25f + i * 0.25f, -0.08f, -0.015f);
            }
            else
            {
                btnGO = CrearCubo($"BtnOpcion_{i}",
                    new Vector3(-0.25f + i * 0.25f, -0.08f, -0.015f),
                    new Vector3(0.2f, 0.06f, 0.02f), coloresBtn[i]);
                btnGO.transform.SetParent(quizGO.transform, false);
            }
            btnGO.name = $"BtnOpcion_{i}";
            botonesQuiz[i] = btnGO.GetComponentInChildren<BotonLab>();

            var txtOpGO = new GameObject($"TxtOpcion_{i}");
            txtOpGO.transform.SetParent(btnGO.transform, false);
            txtOpGO.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            var txtOp = txtOpGO.AddComponent<TextMeshPro>();
            txtOp.text = $"Opción {i + 1}";
            txtOp.fontSize = 0.05f;
            txtOp.alignment = TextAlignmentOptions.Center;
            txtOp.rectTransform.sizeDelta = new Vector2(0.18f, 0.05f);
            txtOpciones[i] = txtOp;
        }

        var quizComp = quizGO.AddComponent<QuizPanel>();
        var soQuiz = new SerializedObject(quizComp);
        soQuiz.FindProperty("txtPregunta").objectReferenceValue = txtPreg;
        soQuiz.FindProperty("txtFeedback").objectReferenceValue = txtFb;
        var propBotones = soQuiz.FindProperty("botonesOpciones");
        propBotones.arraySize = 3;
        var propTxtOpc = soQuiz.FindProperty("txtOpciones");
        propTxtOpc.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            propBotones.GetArrayElementAtIndex(i).objectReferenceValue = botonesQuiz[i];
            propTxtOpc.GetArrayElementAtIndex(i).objectReferenceValue = txtOpciones[i];
        }
        // Preguntas del quiz
        var propPreguntas = soQuiz.FindProperty("preguntas");
        propPreguntas.arraySize = 3;
        string[][] opcionesQuiz =
        {
            new[] { "Transferencia horizontal de genes", "Fotosíntesis", "Fisión nuclear" },
            new[] { "Carbono-14", "Tritio (³H)", "Uranio-235" },
            new[] { "20–25 °C", "35–40 °C", "60–70 °C" }
        };
        string[] preguntasTexto =
        {
            "¿Qué proceso biológico real inspiró la transferencia de ADN arácnido?",
            "¿Qué isótopo radiactivo se usa como vector en la síntesis?",
            "¿Cuál es el rango de temperatura ideal para el suero arácnido?"
        };
        int[] correctas = { 0, 1, 1 };
        for (int i = 0; i < 3; i++)
        {
            var pElem = propPreguntas.GetArrayElementAtIndex(i);
            pElem.FindPropertyRelative("pregunta").stringValue = preguntasTexto[i];
            pElem.FindPropertyRelative("indiceCorrecta").intValue = correctas[i];
            pElem.FindPropertyRelative("puntosCorrecta").intValue = 15;
            var propOpc = pElem.FindPropertyRelative("opciones");
            propOpc.arraySize = 3;
            for (int j = 0; j < 3; j++)
                propOpc.GetArrayElementAtIndex(j).stringValue = opcionesQuiz[i][j];
        }
        soQuiz.ApplyModifiedProperties();
        quizGO.SetActive(false); // se activa cuando la validación es correcta

        // ── PASO 14: Checklist del capítulo ───────────────────────────────────
        var checklistGO = new GameObject("ChapterChecklist");
        checklistGO.transform.position = new Vector3(2.5f, 1.4f, 0f);
        checklistGO.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        var checklistPanel = CrearCubo("FondoChecklist", Vector3.zero,
            new Vector3(0.5f, 0.7f, 0.02f), new Color(0.05f, 0.1f, 0.05f));
        checklistPanel.transform.SetParent(checklistGO.transform, false);
        var txtCheckGO = new GameObject("TxtChecklist");
        txtCheckGO.transform.SetParent(checklistGO.transform, false);
        txtCheckGO.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        var txtCheck = txtCheckGO.AddComponent<TextMeshPro>();
        txtCheck.text = "";
        txtCheck.fontSize = 0.04f;
        txtCheck.alignment = TextAlignmentOptions.TopLeft;
        txtCheck.rectTransform.sizeDelta = new Vector2(0.45f, 0.65f);
        var checklistComp = checklistGO.AddComponent<ChapterChecklist>();
        var soChecklist = new SerializedObject(checklistComp);
        soChecklist.FindProperty("cuerpo").objectReferenceValue = txtCheck;
        soChecklist.ApplyModifiedProperties();

        // ── Cap1Manager (director) ────────────────────────────────────────────
        var cap1GO = new GameObject("Cap1Manager");
        var cap1 = cap1GO.AddComponent<Cap1Manager>();
        var soCap1 = new SerializedObject(cap1);
        soCap1.FindProperty("capitulo").objectReferenceValue = capitulo;
        soCap1.FindProperty("tablaHeroes").objectReferenceValue = tablaHeroes;
        soCap1.FindProperty("socketComic").objectReferenceValue = comicReader.GetComponent<XRSocketInteractor>();
        soCap1.FindProperty("dnAExtractor").objectReferenceValue = extractor;

        soCap1.FindProperty("matraz").objectReferenceValue = mixingFlask;
        soCap1.FindProperty("reactor").objectReferenceValue = reactorCtrl;
        soCap1.FindProperty("validator").objectReferenceValue = validator;

        soCap1.FindProperty("dron").objectReferenceValue = dronComp;

        soCap1.FindProperty("checklist").objectReferenceValue = checklistComp;
        if (botonAnalizarComp != null)
            soCap1.FindProperty("botonAnalizar").objectReferenceValue = botonAnalizarComp;
        var propBandejas = soCap1.FindProperty("bandejas");
        propBandejas.arraySize = 3;
        for (int i = 0; i < 3; i++)
            propBandejas.GetArrayElementAtIndex(i).objectReferenceValue = bandejas[i];
        soCap1.ApplyModifiedProperties();

        // Asignar ChapterManager y ChapterSO al ComicReader
        var soComic = new SerializedObject(comicReader);
        soComic.FindProperty("capitulo").objectReferenceValue = capitulo;
        soComic.FindProperty("tabla").objectReferenceValue = tablaHeroes;
        soComic.FindProperty("audioSource").objectReferenceValue = audioSocketComic;
        soComic.ApplyModifiedProperties();

        // ── Guardar escena ────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, ESCENA_PATH);
        AssetDatabase.Refresh();
        Debug.Log($"[CrearEscenaCap1] ✅ Escena guardada en {ESCENA_PATH}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject CrearCubo(string nombre, Vector3 pos, Vector3 escala, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.position = pos;
        go.transform.localScale = escala;
        SetColor(go, color);
        return go;
    }

    static void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") 
                               ?? Shader.Find("Standard"));
        mat.color = color;
        r.sharedMaterial = mat;
    }

    static void AsegurarTag(string tag)
    {
        // Añade el tag al TagManager si no existe
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[CrearEscenaCap1] Tag '{tag}' creado.");
    }

    static void AgregarEtiqueta(GameObject padre, string texto, Vector3 offsetLocal)
    {
        var go = new GameObject("Etiqueta_" + texto);
        go.transform.SetParent(padre.transform, false);
        go.transform.localPosition = offsetLocal;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = 0.08f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.06f);
        go.AddComponent<Billboard>();
    }
}
