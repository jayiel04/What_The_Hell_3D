using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Herramientas de autoría ejecutables desde Unity Editor (batch mode incluido).
/// Crea contenido serializado persistente en las escenas: Canvas/UGUI, EventSystem,
/// fuentes de audio, NavMeshSurface horneada y prefabs de proyectil.
/// </summary>
public static class CampaignAuthoringTools
{
    private const string ScenesRoot = "Assets/WhatTheHell3D/Scenes";
    private const string PrefabsRoot = "Assets/WhatTheHell3D/Prefabs";
    private static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("WhatTheHell3D/Autoría/Configurar UI, audio y NavMesh de todas las escenas")]
    public static void AuthorAllFromMenu()
    {
        AuthorAll();
    }

    public static void AuthorAll()
    {
        EnsureFolders();
        GameObject projectilePrefab = CreateWitchProjectilePrefabAsset();
        AudioMixer mixer = CreateAudioMixerAsset();

        MenuSceneAuthoring.AuthorMenuScene3D();
        IntroSceneAuthoring.AuthorIntroScene3D();
        AuthorVictoryScene();
        foreach (string level in new[] { "CampaignLevel01", "CampaignLevel02", "CampaignLevel03" })
        {
            AuthorCampaignLevel(level, projectilePrefab, mixer);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Authoring] Autoría completa: UI UGUI, audio y NavMesh configurados en las seis escenas.");
    }

    // ------------------------------------------------------------------ menú

    // La autoría del menú 3D vive en MenuSceneAuthoring.cs (Fase 10).

    // ----------------------------------------------------------------- intro

    // La autoría de la intro 3D vive en IntroSceneAuthoring.cs (Fase 9).

    // --------------------------------------------------------------- victoria

    private static void AuthorVictoryScene()
    {
        OpenScene("Victory");
        VictorySceneController controller = FindSingle<VictorySceneController>();
        if (controller == null)
        {
            Debug.LogError("[Authoring] Victory no contiene VictorySceneController.");
            return;
        }

        Canvas canvas = EnsureCanvas("UICanvas", new Color(0.05f, 0.1f, 0.06f, 0.96f));
        Text title = CreateText(canvas.transform, "TitleText", "¡VICTORIA!", 64, new Color(1f, 0.85f, 0.25f), FontStyle.Bold);
        Anchor(title.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(900f, 90f));
        title.alignment = TextAnchor.MiddleCenter;

        Text body = CreateText(canvas.transform, "BodyText", "La campaña de What the Hell? 3D ha terminado.", 24, Color.white);
        Anchor(body.rectTransform, new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), Vector2.zero, new Vector2(900f, 50f));
        body.alignment = TextAnchor.MiddleCenter;

        Button menu = CreateButton(canvas.transform, "MenuButton", "Volver al menú", new Vector2(0f, -180f));
        ((RectTransform)menu.transform).anchorMin = new Vector2(0.5f, 0.5f);
        ((RectTransform)menu.transform).anchorMax = new Vector2(0.5f, 0.5f);

        controller.titleText = title;
        controller.bodyText = body;
        controller.menuButton = menu;
        EditorUtility.SetDirty(controller);

        AudioSource narration = GameObject.Find("VictoryVoiceSource")?.GetComponent<AudioSource>();
        if (narration == null)
        {
            narration = new GameObject("VictoryVoiceSource").AddComponent<AudioSource>();
        }

        narration.playOnAwake = false;
        narration.spatialBlend = 0f;
        narration.clip = LoadClip("Audio/Source/dialogue/victory_narration.mp3");
        if (narration.clip != null)
        {
            narration.Play();
        }

        SaveOpenScene("Victory");
    }

    // ------------------------------------------------------------ campaña 3D

    private static void AuthorCampaignLevel(string sceneName, GameObject projectilePrefab, AudioMixer mixer)
    {
        OpenScene(sceneName);
        CampaignLevelRuntime runtime = FindSingle<CampaignLevelRuntime>();
        CampaignHudController hud = FindSingle<CampaignHudController>();
        PauseController pause = FindSingle<PauseController>();
        CampaignAudioDirector audioDirector = FindSingle<CampaignAudioDirector>();
        PlayerController player = FindSingle<PlayerController>();

        if (runtime == null || hud == null || pause == null || audioDirector == null || player == null)
        {
            Debug.LogError($"[Authoring] {sceneName} incompleta: runtime={runtime != null}, hud={hud != null}, pause={pause != null}, audio={audioDirector != null}, player={player != null}.");
            return;
        }

        AuthorHud(hud);
        AuthorPausePanel(pause);
        AuthorAudio(audioDirector, player, mixer, sceneName);
        AuthorNavMesh(sceneName);

        AssignProjectilePrefabToWitches(projectilePrefab);

        SaveOpenScene(sceneName);
    }

    public static void AuthorHud(CampaignHudController hud)
    {
        Canvas canvas = EnsureCanvas("HUDCanvas", null);
        Text objectiveTitle = CreateText(canvas.transform, "ObjectiveTitle", "", 26, new Color(1f, 0.82f, 0.3f), FontStyle.Bold);
        Anchor(objectiveTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -46f), new Vector2(500f, 34f));
        objectiveTitle.alignment = TextAnchor.UpperLeft;

        Text objective = CreateText(canvas.transform, "ObjectiveText", "", 17, Color.white);
        Anchor(objective.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -84f), new Vector2(620f, 30f));
        objective.alignment = TextAnchor.UpperLeft;

        Text healthLabel = CreateText(canvas.transform, "HealthLabel", "SALUD", 16, Color.white);
        Anchor(healthLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -124f), new Vector2(220f, 24f));
        healthLabel.alignment = TextAnchor.UpperLeft;

        Image barBackground = CreateImage(canvas.transform, "HealthBarBackground", new Color(0.65f, 0.08f, 0.08f));
        Anchor(barBackground.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -152f), new Vector2(220f, 14f));
        Image fill = CreateImage(barBackground.transform, "HealthBarFill", new Color(0.15f, 0.8f, 0.25f));
        RectTransform fillRect = (RectTransform)fill.transform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;

        Text coins = CreateText(canvas.transform, "CoinsLabel", "", 16, Color.white);
        Anchor(coins.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-294f, -24f), new Vector2(270f, 28f));
        coins.alignment = TextAnchor.UpperRight;

        Text key = CreateText(canvas.transform, "KeyLabel", "", 16, Color.white);
        Anchor(key.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-294f, -56f), new Vector2(270f, 28f));
        key.alignment = TextAnchor.UpperRight;

        Text checkpoint = CreateText(canvas.transform, "CheckpointLabel", "", 16, Color.white);
        Anchor(checkpoint.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-294f, -88f), new Vector2(270f, 28f));
        checkpoint.alignment = TextAnchor.UpperRight;

        Text hint = CreateText(canvas.transform, "HintLabel", "", 14, Color.white);
        Anchor(hint.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-294f, 26f), new Vector2(270f, 24f));
        hint.alignment = TextAnchor.LowerRight;

        hud.objectiveTitle = objectiveTitle;
        hud.objectiveText = objective;
        hud.healthLabel = healthLabel;
        hud.healthFill = fill;
        hud.coinsLabel = coins;
        hud.keyLabel = key;
        hud.checkpointLabel = checkpoint;
        hud.hintLabel = hint;
        EditorUtility.SetDirty(hud);
    }

    public static void AuthorPausePanel(PauseController pause)
    {
        Canvas canvas = EnsureCanvas("HUDCanvas", null);
        Transform existing = canvas.transform.Find("PausePanel");
        if (existing != null)
        {
            DestroyPersistent(existing.gameObject);
        }

        GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)panel.transform;
        SetParent(rect, canvas.transform);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        Text title = CreateText(panel.transform, "PauseTitle", "PAUSA", 36, Color.white, FontStyle.Bold);
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(400f, 60f));
        title.alignment = TextAnchor.MiddleCenter;

        Button resume = CreateButton(panel.transform, "ResumeButton", "Continuar", new Vector2(0f, -60f));
        ((RectTransform)resume.transform).anchorMin = new Vector2(0.5f, 0.5f);
        ((RectTransform)resume.transform).anchorMax = new Vector2(0.5f, 0.5f);
        Button restart = CreateButton(panel.transform, "RestartButton", "Reiniciar checkpoint", new Vector2(0f, -118f));
        ((RectTransform)restart.transform).anchorMin = new Vector2(0.5f, 0.5f);
        ((RectTransform)restart.transform).anchorMax = new Vector2(0.5f, 0.5f);
        Button menu = CreateButton(panel.transform, "MenuButton", "Volver al menú", new Vector2(0f, -176f));
        ((RectTransform)menu.transform).anchorMin = new Vector2(0.5f, 0.5f);
        ((RectTransform)menu.transform).anchorMax = new Vector2(0.5f, 0.5f);

        pause.pausePanel = panel;
        pause.resumeButton = resume;
        pause.restartButton = restart;
        pause.menuButton = menu;
        panel.SetActive(false);
        EditorUtility.SetDirty(pause);
    }

    public static void AuthorAudio(CampaignAudioDirector director, PlayerController player, AudioMixer mixer, string sceneName)
    {
        AudioSource ambient = FindOrCreateAudioSource("AmbienceSource", mixer, "Ambience");
        ambient.loop = true;
        ambient.playOnAwake = false;
        ambient.volume = 0.35f;
        ambient.spatialBlend = 0f;

        AudioSource music = FindOrCreateAudioSource("MusicSource", mixer, "Music");
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0.55f;
        music.spatialBlend = 0f;

        AudioSource sfx = FindOrCreateAudioSource("SfxSource", mixer, "SFX");
        sfx.loop = false;
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;

        // En Godot cada nivel solo reproduce su música de nivel (level 1/2/3.mp3).
        // "ambiente 1.mp3" es la música del menú, no del nivel; si se asigna aquí
        // suenan dos canciones a la vez. Para los niveles lo dejamos sin ambient.
        director.ambientClip = null;
        director.musicClip = LoadClip($"Audio/Source/legacy_sounds/{MusicForLevel(sceneName)}");
        director.ambientSource = ambient;
        director.musicSource = music;
        director.sfxSource = sfx;

        AudioSource combat = player.GetComponent<AudioSource>();
        if (combat == null)
        {
            combat = player.gameObject.AddComponent<AudioSource>();
        }

        combat.playOnAwake = false;
        combat.spatialBlend = 0f;
        player.combatAudioSource = combat;
        player.attackClip = LoadClip("Audio/Source/legacy_sounds/espada 1.mp3");
        player.parryClip = LoadClip("Audio/Source/sfx/bell.mp3");
        player.hurtClip = LoadClip("Audio/Source/legacy_sounds/daño.mp3");

        EditorUtility.SetDirty(director);
        EditorUtility.SetDirty(player);
    }

    private static string MusicForLevel(string sceneName)
    {
        switch (sceneName)
        {
            case "CampaignLevel02": return "level 2.mp3";
            case "CampaignLevel03": return "level 3.mp3";
            default: return "level 1.mp3";
        }
    }

    public static void AuthorNavMesh(string sceneName)
    {
        NavMeshSurface surface = UnityEngine.Object.FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            GameObject root = new GameObject("NavMeshSurfaceRoot");
            surface = root.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();

        if (surface.navMeshData != null)
        {
            string navMeshFolder = "Assets/WhatTheHell3D/NavMesh";
            if (!AssetDatabase.IsValidFolder(navMeshFolder))
            {
                Directory.CreateDirectory(navMeshFolder);
                AssetDatabase.Refresh();
            }

            string assetPath = $"{navMeshFolder}/{sceneName}_NavMeshData.asset";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(surface.navMeshData), assetPath);
            surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(assetPath);
            EditorUtility.SetDirty(surface);
        }

        Debug.Log($"[Authoring] NavMesh horneado en {sceneName} (asset externo).");
    }

    public static void AssignProjectilePrefabToWitches(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        EnemyController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int assigned = 0;
        foreach (EnemyController enemy in enemies)
        {
            if (enemy.kind == CampaignEnemyKind.Witch && enemy.projectilePrefab == null)
            {
                enemy.projectilePrefab = prefab;
                EditorUtility.SetDirty(enemy);
                assigned++;
            }
        }

        if (assigned > 0)
        {
            Debug.Log($"[Authoring] Prefab de proyectil asignado a {assigned} brujas.");
        }
    }

    // ------------------------------------------------------------- utilidades

    /// <summary>Wrappers públicos para otras herramientas de autoría (p. ej. IntroSceneAuthoring).</summary>
    public static Canvas EnsureCanvasPublic(string name, Color? background) => EnsureCanvas(name, background);
    public static Text CreateTextPublic(Transform parent, string name, string content, int size, Color color,
        FontStyle style = FontStyle.Normal) => CreateText(parent, name, content, size, color, style);
    public static Image CreateImagePublic(Transform parent, string name, Color color) => CreateImage(parent, name, color);
    public static Button CreateButtonPublic(Transform parent, string name, string label, Vector2 anchoredPosition)
        => CreateButton(parent, name, label, anchoredPosition);
    public static void AnchorPublic(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        => Anchor(rect, anchorMin, anchorMax, position, size);

    [MenuItem("WhatTheHell3D/Autoría/Autoría de niveles de campaña (HUD, pausa, audio, NavMesh)")]
    public static void AuthorCampaignLevelsFromMenu()
    {
        AuthorCampaignLevels();
    }

    /// <summary>Punto de entrada batch para re-autor únicamente los tres niveles.</summary>
    public static void AuthorCampaignLevels()
    {
        EnsureFolders();
        GameObject projectilePrefab = CreateWitchProjectilePrefabAsset();
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/WhatTheHell3D/Audio/WhatTheHellMixer.mixer")
                           ?? CreateAudioMixerAsset();
        foreach (string level in new[] { "CampaignLevel01", "CampaignLevel02", "CampaignLevel03" })
        {
            AuthorCampaignLevel(level, projectilePrefab, mixer);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Authoring] Autoría de niveles completada.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(PrefabsRoot))
        {
            Directory.CreateDirectory(PrefabsRoot);
            AssetDatabase.Refresh();
        }
    }

    private static T FindSingle<T>() where T : MonoBehaviour
    {
        return UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    }

    private static void OpenScene(string name)
    {
        EditorSceneManager.OpenScene($"{ScenesRoot}/{name}.unity", OpenSceneMode.Single);
    }

    private static void SaveOpenScene(string name)
    {
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Authoring] Escena guardada: {name}");
    }

    private static void DestroyPersistent(GameObject target)
    {
        UnityEngine.Object.DestroyImmediate(target);
    }

    private static Canvas EnsureCanvas(string name, Color? background)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Canvas found = existing.GetComponent<Canvas>();
            if (found != null && background.HasValue)
            {
                Image bg = existing.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = background.Value;
                }
            }

            return found;
        }

        GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        EnsureEventSystem();

        if (background.HasValue)
        {
            Image bg = CreateImage(canvas.transform, "Background", background.Value);
            RectTransform bgRect = (RectTransform)bg.transform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (FindSingle<EventSystem>() != null)
        {
            return;
        }

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static void SetParent(RectTransform rect, Transform parent)
    {
        rect.SetParent(parent, false);
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        SetParent((RectTransform)go.transform, parent);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<Image>();
    }

    private static Text CreateText(Transform parent, string name, string content, int size, Color color,
        FontStyle style = FontStyle.Normal)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        SetParent((RectTransform)go.transform, parent);
        Text text = go.GetComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = (RectTransform)go.transform;
        SetParent(rect, parent);
        rect.sizeDelta = new Vector2(280f, 48f);
        rect.anchoredPosition = anchoredPosition;
        go.GetComponent<Image>().color = new Color(0.18f, 0.2f, 0.27f, 0.95f);
        ColorBlock colors = go.GetComponent<Button>().colors;
        colors.highlightedColor = new Color(0.28f, 0.32f, 0.45f);
        colors.pressedColor = new Color(0.12f, 0.13f, 0.19f);
        go.GetComponent<Button>().colors = colors;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        SetParent((RectTransform)labelGo.transform, rect);
        RectTransform labelRect = (RectTransform)labelGo.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text labelText = labelGo.GetComponent<Text>();
        labelText.font = DefaultFont;
        labelText.text = label;
        labelText.fontSize = 20;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        return go.GetComponent<Button>();
    }

    private static void RemoveExistingChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }
    }

    private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static AudioSource FindOrCreateAudioSource(string name, AudioMixer mixer, string groupName)
    {
        GameObject holder = GameObject.Find(name);
        if (holder == null)
        {
            holder = new GameObject(name);
        }

        AudioSource source = holder.GetComponent<AudioSource>();
        if (source == null)
        {
            source = holder.AddComponent<AudioSource>();
        }

        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups(groupName);
            if (groups.Length > 0)
            {
                source.outputAudioMixerGroup = groups[groups.Length - 1];
            }
        }

        return source;
    }

    private static AudioClip LoadClip(string relativePath)
    {
        string path = $"Assets/WhatTheHell3D/{relativePath}";
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static GameObject CreateWitchProjectilePrefabAsset()
    {
        string path = $"{PrefabsRoot}/WitchProjectile.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject("WitchProjectile", typeof(SphereCollider), typeof(Rigidbody),
            typeof(WitchProjectileRuntime));
        SphereCollider collider = root.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.28f;
        Rigidbody body = root.GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UnityEngine.Object.DestroyImmediate(mesh.GetComponent<Collider>());
        mesh.transform.SetParent(root.transform, false);
        mesh.transform.localScale = Vector3.one * 0.56f;
        MeshRenderer renderer = mesh.GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.65f, 0.2f, 0.9f);
        renderer.sharedMaterial = material;
        AssetDatabase.CreateAsset(material, $"{PrefabsRoot}/WitchProjectileMaterial.mat");

        root.tag = "Projectile";
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        Debug.Log("[Authoring] Prefab WitchProjectile creado.");
        return saved;
    }

    private static AudioMixer CreateAudioMixerAsset()
    {
        const string path = "Assets/WhatTheHell3D/Audio/WhatTheHellMixer.mixer";
        AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
        if (existing != null)
        {
            return existing;
        }

        try
        {
            System.Reflection.Assembly editorAssembly = typeof(Editor).Assembly;
            Type controllerType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerController");
            if (controllerType == null)
            {
                throw new InvalidOperationException("No se encontró AudioMixerController.");
            }

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            object controller = controllerType.GetMethod("CreateMixerControllerAtPath", flags)?
                .Invoke(null, new object[] { path });
            if (controller == null)
            {
                throw new InvalidOperationException("CreateMixerControllerAtPath devolvió null.");
            }

            System.Reflection.MethodInfo createGroup = controllerType.GetMethod("CreateNewGroup", flags);
            foreach (string groupName in new List<string> { "Master", "Music", "Ambience", "SFX" })
            {
                createGroup?.Invoke(controller, new object[] { groupName, false });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Authoring] AudioMixer creado con grupos Master, Music, Ambience y SFX.");
            return (AudioMixer)controller;
        }
        catch (Exception exception)
        {
            string details = exception is System.Reflection.TargetInvocationException && exception.InnerException != null
                ? $"{exception.InnerException.GetType().Name}: {exception.InnerException.Message}\n{exception.InnerException.StackTrace}"
                : exception.Message;
            Debug.LogWarning($"[Authoring] No se pudo crear el AudioMixer automáticamente: {details}");
            return null;
        }
    }
}
