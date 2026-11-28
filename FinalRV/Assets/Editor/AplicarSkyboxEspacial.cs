#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú Proyecto > Aplicar Skybox Espacial.
/// Busca la imagen "Espacio_Skybox" en el proyecto, crea un material Skybox/Panoramic
/// y lo pone como cielo de la escena abierta. Va dentro de una carpeta llamada Editor.
/// </summary>
public static class AplicarSkyboxEspacial
{
    [MenuItem("Proyecto/Aplicar Skybox Espacial")]
    static void Aplicar()
    {
        var guids = AssetDatabase.FindAssets("Espacio_Skybox t:Texture2D");
        if (guids.Length == 0)
        {
            Debug.LogError("[Skybox] No encuentro la imagen Espacio_Skybox. Arrástrala al Project y vuelve a intentar.");
            return;
        }

        string rutaTex = AssetDatabase.GUIDToAssetPath(guids[0]);
        var imp = AssetImporter.GetAtPath(rutaTex) as TextureImporter;
        if (imp != null)
        {
            imp.textureShape = TextureImporterShape.Texture2D;
            imp.maxTextureSize = 4096;
            imp.mipmapEnabled = false;
            imp.wrapModeU = TextureWrapMode.Repeat;
            imp.wrapModeV = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaTex);

        var shader = Shader.Find("Skybox/Panoramic");
        if (shader == null)
        {
            Debug.LogError("[Skybox] No encuentro el shader Skybox/Panoramic.");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Proyecto/Materials"))
            AssetDatabase.CreateFolder("Assets/Proyecto", "Materials");

        string rutaMat = "Assets/Proyecto/Materials/Skybox_Espacio.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(rutaMat) != null) AssetDatabase.DeleteAsset(rutaMat);

        var mat = new Material(shader);
        mat.SetTexture("_MainTex", tex);
        mat.SetFloat("_Mapping", 1f);      // Latitude Longitude Layout
        mat.SetFloat("_ImageType", 0f);    // 360 grados
        mat.SetFloat("_Exposure", 1.3f);
        mat.SetFloat("_Rotation", 0f);
        mat.EnableKeyword("_MAPPING_LATITUDE_LONGITUDE_LAYOUT");
        mat.DisableKeyword("_MAPPING_6_FRAMES_LAYOUT");
        AssetDatabase.CreateAsset(mat, rutaMat);

        RenderSettings.skybox = mat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        DynamicGI.UpdateEnvironment();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[Skybox] Skybox espacial aplicado. Guarda la escena con Ctrl+S.");
    }
}
#endif
