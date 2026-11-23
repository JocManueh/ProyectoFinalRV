/*
 * Corrige el Canvas del DronXentari:
 * - Saca el CanvasAviso del DronXentari para que no herede su escala 0.15
 * - Lo convierte en hijo directo de la escena con seguimiento por script
 * - Ajusta el RectTransform y fontSize del texto
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class AjustarDron
{
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
            "Assets/_Project/Scenes/Tests/Test_Cap1.unity");
        if (!scene.isLoaded)
            scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Tests/Test_Cap1.unity", OpenSceneMode.Single);

        var dronGO = GameObject.Find("DronXentari");
        if (dronGO == null) { Debug.LogError("DronXentari no encontrado"); return; }

        // Corregir escala del dron
        dronGO.transform.localScale = Vector3.one * 0.12f;

        // Buscar el CanvasAviso
        var canvasAvisoGO = dronGO.transform.Find("CanvasAviso")?.gameObject;
        if (canvasAvisoGO == null) { Debug.LogError("CanvasAviso no encontrado"); return; }

        // Desparentar el canvas para que no herede la escala del dron
        canvasAvisoGO.transform.SetParent(null, true);
        canvasAvisoGO.transform.position = new Vector3(0f, 2.3f, 0f);

        // Escala del canvas en world space: 0.003 es estándar para world-space UI
        canvasAvisoGO.transform.localScale = Vector3.one * 0.003f;

        // Ajustar RectTransform del canvas
        var rectCanvas = canvasAvisoGO.GetComponent<RectTransform>();
        if (rectCanvas != null)
        {
            rectCanvas.sizeDelta = new Vector2(200f, 60f);
        }

        // Ajustar el texto
        var txtAviso = canvasAvisoGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txtAviso != null)
        {
            txtAviso.text     = "! DRON XENTARI DETECTADO !";
            txtAviso.fontSize = 18f;
            txtAviso.color    = Color.red;
            txtAviso.alignment = TextAlignmentOptions.Center;
            var rectTxt = txtAviso.GetComponent<RectTransform>();
            if (rectTxt != null)
            {
                rectTxt.anchorMin = Vector2.zero;
                rectTxt.anchorMax = Vector2.one;
                rectTxt.offsetMin = rectTxt.offsetMax = Vector2.zero;
            }
        }

        // Actualizar la referencia en DronXentari
        var dronComp = dronGO.GetComponent<DronXentari>();
        if (dronComp != null)
        {
            var so = new SerializedObject(dronComp);
            so.FindProperty("canvasAviso").objectReferenceValue =
                canvasAvisoGO.GetComponent<Canvas>();
            so.FindProperty("txtAviso").objectReferenceValue = txtAviso;
            so.ApplyModifiedProperties();
        }

        // Ocultar el canvas (se activa en runtime)
        canvasAvisoGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AjustarDron] ✅ Canvas del Dron corregido.");
    }
}
