using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Captura PNGs reales de las escenas (batchmode con GPU) para verificación visual.
/// Uso: -executeMethod SceneScreenshot.CaptureMenu  (o CaptureSceneWithArg + -customScene)
/// Renderiza la cámara principal a un RenderTexture 1280×720 incluyendo la UI
/// (cambia temporalmente el canvas a ScreenSpaceCamera) y guarda el PNG.
/// </summary>
public static class SceneScreenshot
{
    private const string OutputDir = "/tmp/opencode/shots";

    public static void CaptureMenu()
    {
        Capture("MainMenu", "menu");
    }

    public static void CaptureIntro()
    {
        Capture("Intro", "intro");
    }

    public static void CaptureSceneWithArg()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        string scene = "MainMenu";
        string output = "scene";
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-customScene") scene = args[i + 1];
            if (args[i] == "-customOutput") output = args[i + 1];
        }

        Capture(scene, output);
    }

    private static void Capture(string sceneName, string outputName)
    {
        EditorSceneManager.OpenScene($"{SceneScreenshotPaths.ScenesRoot}/{sceneName}.unity", OpenSceneMode.Single);
        CaptureCurrent(outputName);
    }

    public static void CaptureCurrent(string outputName)
    {


        Camera camera = Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            Debug.LogError("[Screenshot] No hay cámara en la escena.");
            EditorApplication_Exit(1);
            return;
        }

        // Forzar un frame de simulación para que los scripts de posición (sway) actúen.
        for (int i = 0; i < 5; i++)
        {
            EditorApplication_Step();
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        RenderMode[] previous = new RenderMode[canvases.Length];
        for (int i = 0; i < canvases.Length; i++)
        {
            previous[i] = canvases[i].renderMode;
            canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
            canvases[i].worldCamera = camera;
            canvases[i].planeDistance = 0.5f;
            Debug.Log($"[Screenshot] canvas '{canvases[i].name}' enabled={canvases[i].enabled} goActive={canvases[i].gameObject.activeInHierarchy} renderers={canvases[i].GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Length}");
        }

        // Bombear el player loop tras activar/cambiar canvases para que la UI
        // nunca renderizada construya su geometría antes del render.
        for (int i = 0; i < 8; i++)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }

        Canvas.ForceUpdateCanvases();

        int width = 1280;
        int height = 720;
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        camera.Render();

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = previousActive;
        camera.targetTexture = null;

        byte[] png = texture.EncodeToPNG();
        Directory.CreateDirectory(OutputDir);
        string path = $"{OutputDir}/{outputName}.png";
        File.WriteAllBytes(path, png);
        Debug.Log($"[Screenshot] Guardado: {path}");

        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].renderMode = previous[i];
            canvases[i].worldCamera = null;
        }

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(texture);
        EditorApplication_Exit(0);
    }

    private static void EditorApplication_Step()
    {
        // Simular un tick de Update/LateUpdate en editor para scripts de cámara.
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    }

    private static void EditorApplication_Exit(int code)
    {
        EditorApplication.Exit(code);
    }
}

internal static class SceneScreenshotPaths
{
    public const string ScenesRoot = "Assets/Scenes";
}

public static class PauseShot
{
    public static void Capture()
    {
        EditorSceneManager.OpenScene($"{SceneScreenshotPaths.ScenesRoot}/CampaignLevel01.unity", OpenSceneMode.Single);
        var pause = Object.FindFirstObjectByType<PauseController>(FindObjectsInactive.Include);
        if (pause != null && pause.pausePanel != null)
        {
            pause.pausePanel.SetActive(true);
        }

        SceneScreenshot.CaptureCurrent("pause");
    }
}

public static class VictoryShot
{
    public static void Capture()
    {
        EditorSceneManager.OpenScene($"{SceneScreenshotPaths.ScenesRoot}/Victory.unity", OpenSceneMode.Single);
        SceneScreenshot.CaptureCurrent("victory");
    }
}
