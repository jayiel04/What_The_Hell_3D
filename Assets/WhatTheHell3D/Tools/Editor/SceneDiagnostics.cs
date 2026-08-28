using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneDiagnostics
{
    public static void Diagnose()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CampaignLevel01.unity", OpenSceneMode.Single);

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name.StartsWith("CoinPickup") || go.name.StartsWith("Checkpoint") || go.name.StartsWith("Goal"))
            {
                string parts = string.Empty;
                foreach (Component component in go.GetComponents<Component>())
                {
                    parts += component == null ? "[NULL] " : component.GetType().Name + " ";
                }

                Debug.Log($"[Diag2] {go.name}: {parts}");
            }
        }

        Debug.Log($"[Diag2] CheckpointRuntime count: {Resources.FindObjectsOfTypeAll<CheckpointRuntime>().Length}");
    }
}
