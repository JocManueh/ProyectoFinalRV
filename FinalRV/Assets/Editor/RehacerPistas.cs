/*
 * Rehace las 3 PistaRevelable en TablaHeroes/PanelTabla.
 * Cada pista tiene:
 *   - Su propio TMP_Text visible con "???"
 *   - BoxCollider en world-space para que XRI lo detecte
 *   - XRSimpleInteractable
 * Al tocarla cambia su propio texto a la pista real.
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class RehacerPistas
{
    const string SO_PATH = "Assets/_Project/ScriptableObjects/Cap1/";

    [MenuItem("Tools/Rehacer Pistas Revelables (Lab)")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("01_Laboratorio");
        if (!scene.isLoaded)
        {
            Debug.LogError("[RehacerPistas] Abre la escena 01_Laboratorio primero.");
            return;
        }

        var capSO = AssetDatabase.LoadAssetAtPath<ChapterSO>(SO_PATH + "Chapter_Spider.asset");
        if (capSO == null || capSO.pistas == null || capSO.pistas.Length < 3)
        {
            Debug.LogError("[RehacerPistas] Chapter_Spider.asset no tiene 3 pistas. Ejecuta primero CrearDatosSpider.");
            return;
        }

        // Buscar el canvas del panel
        var panelTabla = GameObject.Find("TablaHeroes/PanelTabla");
        if (panelTabla == null)
        {
            Debug.LogError("[RehacerPistas] No se encontró TablaHeroes/PanelTabla.");
            return;
        }

        // ── Eliminar pistas antiguas ──────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            var vieja = panelTabla.transform.Find($"Pista_{i}");
            if (vieja != null) Object.DestroyImmediate(vieja.gameObject);
        }

        // ── Eliminar TxtPistas antiguo (ya no lo necesitamos como acumulador) ─
        // Lo dejamos pero lo ocultamos; TablaHeroes lo actualizará igual
        var txtPistasViejo = panelTabla.transform.Find("TxtPistas");
        // No lo borramos, TablaHeroes lo sigue usando como resumen

        // ── Crear 3 pistas nuevas ─────────────────────────────────────────────
        // Posiciones en canvas-pixels (panel 500×380, centro en 0,0)
        // Zona de pistas: desde y=-120 hacia abajo, cada pista ocupa 30px
        float[] pistasY    = { -125f, -158f, -191f };
        Color   colorPista = new Color(1f, 0.85f, 0.2f); // amarillo

        var nuevasPistas = new PistaRevelable[3];

        for (int i = 0; i < 3; i++)
        {
            // ── Raíz de la pista ──────────────────────────────────────────────
            var pistaGO = new GameObject($"Pista_{i}");
            pistaGO.transform.SetParent(panelTabla.transform, false);

            var rect = pistaGO.AddComponent<RectTransform>();
            rect.sizeDelta        = new Vector2(440f, 26f);
            rect.anchoredPosition = new Vector2(-10f, pistasY[i]);

            // ── Fondo semitransparente para indicar que es tocable ────────────
            var img = pistaGO.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 0.4f);
            img.raycastTarget = true; // necesario para GraphicRaycaster

            // ── TMP_Text propio de esta pista ─────────────────────────────────
            var txtGO = new GameObject("Txt");
            txtGO.transform.SetParent(pistaGO.transform, false);
            var txtRect = txtGO.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(6f, 2f);
            txtRect.offsetMax = new Vector2(-6f, -2f);

            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "???";
            tmp.fontSize  = 11f;
            tmp.color     = colorPista;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false; // el padre recibe el raycast

            // ── BoxCollider en world-space para XRI ───────────────────────────
            // El canvas tiene escala 0.003 → 440px × 26px × 10px en canvas = 1.32m × 0.078m × 0.03m
            var box = pistaGO.AddComponent<BoxCollider>();
            box.size   = new Vector3(440f, 26f, 10f);
            box.center = Vector3.zero;

            // ── AudioSource ───────────────────────────────────────────────────
            var audio = pistaGO.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;
            audio.playOnAwake  = false;

            // ── XRSimpleInteractable ──────────────────────────────────────────
            pistaGO.AddComponent<XRSimpleInteractable>();

            // ── PistaRevelable ────────────────────────────────────────────────
            var pr = pistaGO.AddComponent<PistaRevelable>();
            var so = new SerializedObject(pr);
            so.FindProperty("textoOculto").stringValue              = "???";
            so.FindProperty("textoReal").stringValue                = capSO.pistas[i];
            so.FindProperty("etiqueta").objectReferenceValue        = tmp;
            so.FindProperty("audioSource").objectReferenceValue     = audio;
            so.ApplyModifiedProperties();

            nuevasPistas[i] = pr;
        }

        // ── Actualizar TablaHeroes con las nuevas pistas ──────────────────────
        var tablaComp = GameObject.Find("TablaHeroes")?.GetComponent<TablaHeroes>();
        if (tablaComp != null)
        {
            var soTabla = new SerializedObject(tablaComp);
            var propPistas = soTabla.FindProperty("pistasRevelables");
            propPistas.arraySize = 3;
            for (int i = 0; i < 3; i++)
                propPistas.GetArrayElementAtIndex(i).objectReferenceValue = nuevasPistas[i];
            soTabla.ApplyModifiedProperties();

            // Recargar el capítulo para que los textos queden en "???"
            tablaComp.CargarCapitulo(capSO);
            Debug.Log("[RehacerPistas] TablaHeroes actualizada con las nuevas pistas.");
        }

        // ── Guardar ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();
        Debug.Log("[RehacerPistas] ✅ Pistas rehachas y escena guardada.");
    }
}
