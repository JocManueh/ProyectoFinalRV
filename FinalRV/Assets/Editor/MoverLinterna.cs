using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MoverLinterna
{
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
            "Assets/_Project/Scenes/Tests/Test_Cap1.unity");
        if (!scene.isLoaded)
            scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Tests/Test_Cap1.unity", OpenSceneMode.Single);

        // Buscar incluyendo inactivos
        foreach (var root in scene.GetRootGameObjects())
        {
            var linterna = root.name == "Linterna" ? root
                : root.transform.Find("Linterna")?.gameObject;
            if (linterna != null)
            {
                linterna.transform.position = new Vector3(-0.9f, 0.84f, -0.7f);
                Debug.Log("[MoverLinterna] Linterna reposicionada.");
                break;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
