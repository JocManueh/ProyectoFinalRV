/*
 * Ajuste final de layout para Test_Cap1.
 * Ejecutar desde Tools > Ajustar Escena Test_Cap1
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class AjustarEscenaCap1
{
    [MenuItem("Tools/Ajustar Escena Test_Cap1")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
            "Assets/_Project/Scenes/Tests/Test_Cap1.unity");
        if (!scene.isLoaded)
            scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Tests/Test_Cap1.unity", OpenSceneMode.Single);

        // ── Mesa ──────────────────────────────────────────────────────────────
        Mover("Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 1.0f));

        // ── Ampollas: escala 0.15 y posición en estantería ────────────────────
        // El prefab Ampolla tiene escala 0.5 en su raíz → a 0.15 queda ~15cm
        Vector3[] posAmpollas =
        {
            new Vector3(-2.55f, 0.88f, -0.15f),
            new Vector3(-2.55f, 0.88f,  0.05f),
            new Vector3(-2.55f, 0.88f,  0.25f),
            new Vector3(-2.35f, 0.88f, -0.15f),
            new Vector3(-2.35f, 0.88f,  0.05f),
            new Vector3(-2.35f, 0.88f,  0.25f),
        };
        for (int i = 0; i < 6; i++)
        {
            var a = GameObject.Find($"Ampolla_{i}");
            if (a == null) continue;
            a.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            a.transform.position   = posAmpollas[i];
            // Reducir font size de etiquetas hijas
            foreach (var t in a.GetComponentsInChildren<TMP_Text>(true))
            { t.fontSize = 2f; t.enableAutoSizing = false; }
        }

        // ── Estantería ────────────────────────────────────────────────────────
        Mover("Estanteria", new Vector3(-2.45f, 0.5f, 0.05f), new Vector3(0.25f, 1.0f, 0.65f));

        // ── Matraz al centro de la mesa ───────────────────────────────────────
        Mover("Matraz_Mezcla", new Vector3(0f, 0.82f, 0f));

        // ── Palanca reactor ───────────────────────────────────────────────────
        Mover("PalancaReactor",    new Vector3(-0.6f, 0.82f, -0.35f));
        Mover("ReactorController", new Vector3(-0.6f, 0.82f, -0.35f));
        Mover("Termometro",        new Vector3(-0.6f, 1.0f,  -0.15f),
                                   new Vector3(0.04f, 0.12f,  0.04f));
        Mover("TxtTemperatura",    new Vector3(-0.6f, 1.22f, -0.15f));
        SetFontSize("TxtTemperatura", 0.12f);

        // ── Botón ANALIZAR ────────────────────────────────────────────────────
        Mover("BotonAnalizar",   new Vector3(0.6f, 0.82f, -0.35f));
        Mover("PilotoResultado", new Vector3(0.6f, 0.95f, -0.35f),
                                 new Vector3(0.04f, 0.04f, 0.04f));
        Mover("TxtResultado",    new Vector3(0.6f, 1.08f, -0.35f));
        SetFontSize("TxtResultado", 0.1f);

        // ── Bandejas de clasificación ─────────────────────────────────────────
        string[] bandejas = { "Bandeja_Elemento", "Bandeja_Compuesto", "Bandeja_Biologico" };
        for (int i = 0; i < bandejas.Length; i++)
        {
            Mover(bandejas[i], new Vector3(-0.4f + i * 0.4f, 0.82f, 0.38f),
                               new Vector3(0.32f, 0.04f, 0.22f));
            var b = GameObject.Find(bandejas[i]);
            if (b != null)
                foreach (var t in b.GetComponentsInChildren<TMP_Text>(true))
                    t.fontSize = 0.08f;
        }

        // ── Extractor de ADN ──────────────────────────────────────────────────
        string[] socketsADN = { "Socket_Arana", "Socket_Detergente", "Socket_Sal", "Socket_Alcohol" };
        for (int i = 0; i < socketsADN.Length; i++)
        {
            Mover(socketsADN[i], new Vector3(-2.1f + i * 0.15f, 0.88f, 0.35f),
                                 new Vector3(0.08f, 0.08f, 0.08f));
            var s = GameObject.Find(socketsADN[i]);
            if (s != null)
                foreach (var t in s.GetComponentsInChildren<TMP_Text>(true))
                    t.fontSize = 0.1f;
        }
        Mover("Varilla", new Vector3(-1.65f, 0.92f, 0.35f),
                         new Vector3(0.025f, 0.15f, 0.025f));

        // ── Cámara de tritio ──────────────────────────────────────────────────
        Mover("CamaraTritio", new Vector3(0.55f, 0.84f, 0.35f),
                              new Vector3(0.2f, 0.18f, 0.18f));
        var ampollaT = GameObject.Find("Ampolla_Tritio");
        if (ampollaT != null)
        {
            ampollaT.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            ampollaT.transform.position   = new Vector3(0.55f, 0.95f, 0.35f);
            foreach (var t in ampollaT.GetComponentsInChildren<TMP_Text>(true))
            { t.fontSize = 2f; t.enableAutoSizing = false; }
        }
        Mover("Pinza_0", new Vector3(0.75f, 0.86f, 0.3f),
                         new Vector3(0.015f, 0.12f, 0.015f));
        Mover("Pinza_1", new Vector3(0.75f, 0.86f, 0.4f),
                         new Vector3(0.015f, 0.12f, 0.015f));

        // ── Socket cómic y cómic ──────────────────────────────────────────────
        Mover("SocketComic",          new Vector3(1.3f, 0.84f, 0f));
        Mover("Comic_AmazingFantasy15", new Vector3(1.3f, 0.9f, 0.35f),
                                        new Vector3(0.12f, 0.16f, 0.015f));
        var comic = GameObject.Find("Comic_AmazingFantasy15");
        if (comic != null)
            foreach (var t in comic.GetComponentsInChildren<TMP_Text>(true))
                t.fontSize = 0.06f;

        // ── Pistas revelables ─────────────────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            Mover($"Pista_{i}", new Vector3(1.65f + i * 0.18f, 1.12f, 0f),
                                new Vector3(0.1f, 0.05f, 0.015f));
            var p = GameObject.Find($"Pista_{i}");
            if (p != null)
                foreach (var t in p.GetComponentsInChildren<TMP_Text>(true))
                    t.fontSize = 0.06f;
        }

        // ── TablaHeroes ───────────────────────────────────────────────────────
        var tablaGO = GameObject.Find("TablaHeroes");
        if (tablaGO != null)
        {
            tablaGO.transform.position = new Vector3(1.8f, 1.35f, 0f);
            tablaGO.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // ── ChapterChecklist ──────────────────────────────────────────────────
        var checklistGO = GameObject.Find("ChapterChecklist");
        if (checklistGO != null)
        {
            checklistGO.transform.position = new Vector3(1.8f, 1.35f, 0.9f);
            checklistGO.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // ── QuizPanel (oculto hasta validación correcta) ──────────────────────
        var quizGO = GameObject.Find("QuizPanel");
        if (quizGO != null)
        {
            quizGO.transform.position = new Vector3(0f, 1.5f, 1.5f);
            quizGO.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            quizGO.SetActive(false);
        }

        // ── Dron Xentari: ocultar en edit mode ───────────────────────────────
        var dronGO = GameObject.Find("DronXentari");
        if (dronGO != null)
        {
            dronGO.transform.position   = new Vector3(0f, 2.5f, 0f);
            dronGO.transform.localScale = Vector3.one * 0.12f;
            dronGO.SetActive(false); // se activa en runtime por DronXentari.Activar()
        }

        // ── Linterna y botón generador ────────────────────────────────────────
        Mover("Linterna",       new Vector3(-0.9f, 0.84f, -0.7f));
        Mover("BotonGenerador", new Vector3(-1.15f, 0.84f, -0.7f));

        // ── NOVA ──────────────────────────────────────────────────────────────
        var nova = GameObject.Find("Nova");
        if (nova != null) nova.transform.position = new Vector3(0f, 1.55f, 1.2f);

        // ── Guardar ───────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();
        Debug.Log("[AjustarEscenaCap1] ✅ Layout final aplicado y escena guardada.");
    }

    static void Mover(string nombre, Vector3 pos, Vector3? escala = null)
    {
        var go = GameObject.Find(nombre);
        if (go == null) { Debug.LogWarning($"[Ajuste] No encontrado: {nombre}"); return; }
        go.transform.position = pos;
        if (escala.HasValue) go.transform.localScale = escala.Value;
    }

    static void SetFontSize(string nombre, float size)
    {
        var go = GameObject.Find(nombre);
        if (go == null) return;
        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null) tmp.fontSize = size;
    }
}
