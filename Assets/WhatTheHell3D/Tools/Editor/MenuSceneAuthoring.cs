using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Autoría reproducible del menú principal réplica de main_menu.gd:
/// mundo 3D (luna NASA girando, 5 campos de estrellas deterministas,
/// castillo medieval con tintes, antorchas, niebla), cámara con vaivén,
/// audio y UI UGUI estilizada con popups de capítulos y créditos.
/// Ejecutar: WhatTheHell3D &gt; Autoría &gt; Autoría de menú 3D.
/// </summary>
public static class MenuSceneAuthoring
{
    private const string ScenesRoot = "Assets/WhatTheHell3D/Scenes";
    private const string SourceRoot = "Assets/WhatTheHell3D/Art/Source";
    private const string MaterialsRoot = "Assets/WhatTheHell3D/Materials/Menu";
    private static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("WhatTheHell3D/Autoría/Autoría de menú 3D")]
    public static void AuthorMenuScene3DFromMenu()
    {
        AuthorMenuScene3D();
    }

    public static void AuthorMenuScene3D()
    {
        EditorSceneManager.OpenScene($"{ScenesRoot}/MainMenu.unity", OpenSceneMode.Single);
        SceneBootstrap bootstrap = Object.FindFirstObjectByType<SceneBootstrap>(FindObjectsInactive.Include);
        if (bootstrap == null)
        {
            Debug.LogError("[MenuAuthoring] MainMenu no contiene SceneBootstrap.");
            return;
        }

        ClearPreviousContent();
        BuildWorld();
        Camera camera = BuildCameraAndAudio(out AudioSource music, out AudioSource click);
        MenuSceneController controller = BuildUi(bootstrap.gameObject, music, click);
        EditorUtility.SetDirty(controller);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[MenuAuthoring] Menú 3D autorado y guardado.");
    }

    // --------------------------------------------------------------- mundo

    private static void BuildWorld()
    {
        Transform world = new GameObject("MenuWorld").transform;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.088f, 0.104f, 0.133f);

        // Luna direccional (rot −18,−12; #dfe9ff; 0.95; sombras).
        GameObject moonLightGo = new GameObject("MoonLight", typeof(Light));
        Light moonLight = moonLightGo.GetComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = HexColor("dfe9ff");
        moonLight.intensity = 0.95f;
        moonLight.shadows = LightShadows.Soft;
        // En Godot la luz apunta por −Z del nodo; en Unity por +Z.
        // Se compensa el yaw para que ilumine la fachada del castillo (−Z mundo).
        moonLightGo.transform.rotation = Quaternion.Euler(-18f, 168f, 0f);

        // Resplandor de luna (#cfe2ff, 0.9, rango 28).
        AddPointLight(world, "MoonGlow", new Vector3(-10.5f, 13.8f, -22f),
            HexColor("cfe2ff"), 0.9f, 28f);

        // Disco lunar NASA: SphereMesh radius 2.35 → esfera Unity escalada ×4.7.
        // Unlit: la luna del original es emisiva y nunca se ve negra.
        Material moonMat = EnsureMaterial("MenuMoon", Color.white,
            LoadTexture($"{SourceRoot}/environment/moon/moon_nasa_lro_4k.png"), Vector2.one, unlit: true);
        GameObject moonDisc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moonDisc.name = "MoonNASA";
        Object.DestroyImmediate(moonDisc.GetComponent<Collider>());
        moonDisc.transform.SetParent(world, false);
        moonDisc.transform.position = new Vector3(-10.5f, 13.8f, -22f);
        moonDisc.transform.localScale = Vector3.one * 4.7f;
        ApplyMaterial(moonDisc, moonMat);
        moonDisc.AddComponent<MoonSpinner>();

        BuildStarFields(world);

        // Piso 40×0.5×35 en (0,−0.35,−8), color #111820, Lit para que las
        // antorchas creen el charco de luz cálida del original.
        Material floorMat = EnsureMaterial("MenuFloor", HexColor("111820"), null, Vector2.one);
        CreateBox(world, "Floor", new Vector3(0f, -0.35f, -8f), new Vector3(40f, 0.5f, 35f), floorMat);

        BuildCastle(world);
        BuildNature(world);
        BuildTorches(world);
        BuildMist(world);
    }

    private static void BuildStarFields(Transform world)
    {
        Material starMat = EnsureMaterial("MenuStar", new Color(0.97f, 0.98f, 1f), null, Vector2.one, unlit: true);
        Mesh starMesh = GetPrimitiveMesh(PrimitiveType.Sphere);
        int[] counts = { 120, 80, 24, 42, 28 };
        string[] names = { "StarsFarA", "StarsFarB", "StarsCluster", "StarsEdge", "StarsTop" };
        for (int pattern = 0; pattern < counts.Length; pattern++)
        {
            Transform field = new GameObject(names[pattern]).transform;
            field.SetParent(world, false);
            for (int i = 0; i < counts[pattern]; i++)
            {
                GameObject star = new GameObject($"Star{i}");
                star.transform.SetParent(field, false);
                star.transform.position = StarPosition(i, pattern);
                float radius = StarRadius(i, pattern) * 4f; // ×4 de margen para visibilidad sin bloom
                star.transform.localScale = Vector3.one * radius * 2f;
                MeshFilter filter = star.AddComponent<MeshFilter>();
                filter.sharedMesh = starMesh;
                MeshRenderer renderer = star.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = starMat;
            }
        }
    }

    /// <summary>Traducción exacta de _star_position(i, pattern) de Godot (división entera).</summary>
    private static Vector3 StarPosition(int i, int pattern)
    {
        switch (pattern)
        {
            case 0:
                return new Vector3(-22f + (i % 12) * 4f + (i % 3) * 0.35f,
                    8.7f + (i / 12) * 1.15f + (i % 4) * 0.18f,
                    -20f - (i % 5) * 1.4f);
            case 1:
                return new Vector3(-23.5f + ((i * 7) % 24) * 2f + (i % 2) * 0.2f,
                    8.2f + ((i * 5) % 6) * 0.95f + (i % 3) * 0.14f,
                    -23f - (i % 3) * 1.2f);
            case 2:
                return new Vector3(-16f + (i % 6) * 5.8f + (i % 3) * 0.25f,
                    10f + (i / 6) * 1.05f + (i % 4) * 0.15f,
                    -16f - (i % 2) * 2f);
            case 3:
            {
                bool leftSide = i < 21;
                float x = leftSide ? -24f - (i % 7) * 0.5f : 18f + (i % 7) * 0.7f;
                return new Vector3(x,
                    7.8f + (i / 7) * 1.05f + (i % 2) * 0.18f,
                    -21.5f - (i % 3));
            }
            default:
                return new Vector3(-20f + (i % 14) * 3.1f,
                    12f + (i / 14) * 1f,
                    -24f);
        }
    }

    /// <summary>Traducción exacta de _star_radius(i, pattern).</summary>
    private static float StarRadius(int i, int pattern)
    {
        switch (pattern)
        {
            case 0: return 0.018f + (i % 6) * 0.006f;
            case 1: return 0.014f + (i % 4) * 0.004f;
            case 2: return 0.02f + (i % 2) * 0.004f;
            case 3: return 0.012f + (i % 3) * 0.004f;
            default: return 0.011f + (i % 2) * 0.004f;
        }
    }

    private static void BuildCastle(Transform world)
    {
        Transform castle = new GameObject("Castle").transform;
        castle.SetParent(world, false);

        // Alturas objetivo (metros) tomadas de la referencia visual de Godot.
        SpawnNormalized(castle, "medieval_buildings/WallEntrance.fbx", "Entrance", new Vector3(0f, 0f, -9f), 180f, 4.5f, "b9c2ce");
        SpawnNormalized(castle, "medieval_buildings/Door.fbx", "Door", new Vector3(0f, 0.1f, -8.75f), 180f, 2.6f, "7d5635");
        SpawnNormalized(castle, "medieval_buildings/Bridge.fbx", "Bridge", new Vector3(0f, 0f, -2.6f), 0f, 4.0f, "9a7a52", useMaxDimension: true);
        SpawnNormalized(castle, "medieval_buildings/Well.fbx", "Well", new Vector3(-6f, 0f, -2f), 0f, 1.6f, "6e7e8f");

        foreach (float side in new[] { -1f, 1f })
        {
            string suffix = side < 0 ? "L" : "R";
            SpawnNormalized(castle, "medieval_buildings/LargeSquareTower.fbx", $"LargeTower{suffix}", new Vector3(side * 7f, 0f, -10.5f), 180f, 8f, "c6ccd6");
            SpawnNormalized(castle, "medieval_buildings/PointyTower.fbx", $"PointyTower{suffix}", new Vector3(side * 12f, 0f, -13f), 180f, 11f, "b7bec9");
            SpawnNormalized(castle, "medieval_buildings/WatchTowerWRoof.fbx", $"WatchTower{suffix}", new Vector3(side * 13.5f, 0f, -5f), 180f, 6.5f, "d7c7ae");
            SpawnNormalized(castle, "medieval_buildings/Banner.fbx", $"Banner{suffix}", new Vector3(side * 3.8f, 2.8f, -8.55f), 180f, 2.2f, "caa24a");
        }

        foreach (float x in new[] { -10.5f, -8.8f, -5f, -3.2f, 3.2f, 5f, 8.8f, 10.5f })
        {
            SpawnNormalized(castle, "medieval_buildings/WallBricks.fbx", $"WallBrick{x}", new Vector3(x, 0f, -10f), 180f, 3.6f, "b8bfc9");
        }
    }

    private static void SpawnRow(Transform parent, string folder, string fileName, string baseName, Vector3[] positions, float targetHeight, string tint)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            SpawnNormalized(parent, $"{folder}/{fileName}", $"{baseName}{i + 1}", positions[i], 180f, targetHeight, tint);
        }
    }

    private static void BuildNature(Transform world)
    {
        Transform nature = new GameObject("Nature").transform;
        nature.SetParent(world, false);
        const string natureFolder = "environment/nature";
        SpawnRow(nature, natureFolder, "Tree1.fbx", "Tree1", new[]
        {
            new Vector3(-26f, 0f, -16f), new Vector3(-22.5f, 0f, -14.5f),
            new Vector3(23f, 0f, -15f), new Vector3(26.5f, 0f, -13.5f)
        }, 5f, "36563a");
        SpawnRow(nature, natureFolder, "Tree2.fbx", "Tree2", new[]
        {
            new Vector3(-30f, 0f, -12f), new Vector3(-18.5f, 0f, -15.5f),
            new Vector3(18.5f, 0f, -15f), new Vector3(30f, 0f, -12.5f)
        }, 4.2f, "3f6441");
        SpawnRow(nature, natureFolder, "Tree3.fbx", "Tree3", new[]
        {
            new Vector3(-33f, 0f, -8f), new Vector3(-28f, 0f, -18f),
            new Vector3(28f, 0f, -18f), new Vector3(33f, 0f, -8.5f)
        }, 4f, "446b46");
        SpawnRow(nature, natureFolder, "Rock1.fbx", "Rock1", new[]
        {
            new Vector3(-16.5f, 0f, -13.5f), new Vector3(16.5f, 0f, -13.5f),
            new Vector3(-20.5f, 0f, -11f), new Vector3(20.5f, 0f, -11f)
        }, 1f, "5d6470");
        SpawnRow(nature, natureFolder, "Rock4.fbx", "Rock4", new[]
        {
            new Vector3(-13.5f, 0f, -15f), new Vector3(13.5f, 0f, -15f)
        }, 1.1f, "525965");
    }

    private static void BuildTorches(Transform world)
    {
        foreach (Vector3 pos in new[]
        {
            new Vector3(-4.4f, 2.7f, -5f), new Vector3(4.4f, 2.7f, -5f),
            new Vector3(-8f, 3.2f, -9f), new Vector3(8f, 3.2f, -9f)
        })
        {
            AddPointLight(world, "Torch", pos, HexColor("ff8a36"), 5f, 9f);
        }
    }

    private static void BuildMist(Transform world)
    {
        Material mistMat = EnsureMaterial("MenuMist", new Color(0.25f, 0.34f, 0.45f, 0.035f), null, Vector2.one, unlit: true);
        Mesh quad = GetPrimitiveMesh(PrimitiveType.Quad);
        for (int i = 0; i < 7; i++)
        {
            GameObject mist = new GameObject($"Mist{i + 1}");
            mist.transform.SetParent(world, false);
            mist.transform.position = new Vector3(-9f + i * 3f, 0.45f + (i % 2) * 0.2f, -1f - i * 2f);
            mist.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            mist.transform.localScale = new Vector3(13f + i * 2f, 3f, 1f);
            MeshFilter filter = mist.AddComponent<MeshFilter>();
            filter.sharedMesh = quad;
            MeshRenderer renderer = mist.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mistMat;
        }
    }

    // ------------------------------------------------------------ cámara/audio

    private static Camera BuildCameraAndAudio(out AudioSource music, out AudioSource click)
    {
        GameObject cameraGo = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
        cameraGo.tag = "MainCamera";
        Camera cameraComponent = cameraGo.GetComponent<Camera>();
        cameraComponent.fieldOfView = 57f;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = HexColor("02040a");
        cameraGo.transform.position = new Vector3(0f, 5f, 14f);
        cameraGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        cameraGo.AddComponent<MenuCameraSway>();

        Transform audio = new GameObject("MenuAudio").transform;
        music = CreateAudioSource(audio, "MusicSource",
            "Assets/WhatTheHell3D/Audio/Source/legacy_sounds/ambiente 1.mp3", loop: true, volume: 0.32f);
        click = CreateAudioSource(audio, "ClickSource",
            "Assets/WhatTheHell3D/Audio/Source/legacy_sounds/votones menu.mp3", loop: false, volume: 0.63f);
        return cameraComponent;
    }

    // -------------------------------------------------------------------- UI

    private static MenuSceneController BuildUi(GameObject bootstrapGo, AudioSource music, AudioSource click)
    {
        Canvas oldCanvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (oldCanvas != null)
        {
            Object.DestroyImmediate(oldCanvas.gameObject);
        }

        Canvas canvas = NewCanvas();

        // Overlay y viñeta (no bloquean raycast).
        FullRectImage(canvas.transform, "Overlay", new Color(0f, 0f, 0f, 0.18f));
        FullRectImage(canvas.transform, "Vignette", new Color(0f, 0f, 0f, 0.2f));

        // Bloque de título arriba-izquierda (pivot 0 para que anchoredPosition sea el borde izquierdo).
        Text title = CreateText(canvas.transform, "TitleText", "WHAT THE HELL?", 34, HexColor("ffb42b"), FontStyle.Bold);
        AnchorLeft(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -70f), new Vector2(700f, 60f));
        title.alignment = TextAnchor.UpperLeft;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        AddShadow(title, new Vector2(4f, 5f));

        Text subtitle = CreateText(canvas.transform, "SubtitleText", "UNA AVENTURA ENTRE BOSQUES, MINAS Y CASTILLOS", 17, HexColor("d8e3f1"));
        AnchorLeft(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -108f), new Vector2(700f, 32f));
        subtitle.alignment = TextAnchor.UpperLeft;
        subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        AddShadow(subtitle, new Vector2(3f, 3f));

        // Panel principal con los 5 botones (pivot arriba-izquierda, relativo al borde superior).
        GameObject mainPanel = new GameObject("MainButtonsPanel", typeof(RectTransform));
        RectTransform mainRect = (RectTransform)mainPanel.transform;
        SetParent(mainRect, canvas.transform);
        mainRect.pivot = new Vector2(0f, 1f);
        Anchor(mainRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -150f), new Vector2(420f, 280f));

        Button newGame = CreateStyledButton(mainPanel.transform, "NewGameButton", "NUEVO JUEGO", new Vector2(0f, -20f), 24, topAnchor: true);
        Button continueGame = CreateStyledButton(mainPanel.transform, "ContinueButton", "CONTINUAR", new Vector2(0f, -82f), 24, topAnchor: true);
        Button chapters = CreateStyledButton(mainPanel.transform, "ChaptersButton", "CAPÍTULOS", new Vector2(0f, -144f), 24, topAnchor: true);
        Button credits = CreateStyledButton(mainPanel.transform, "CreditsButton", "CRÉDITOS", new Vector2(0f, -206f), 24, topAnchor: true);
        Button quit = CreateStyledButton(mainPanel.transform, "QuitButton", "SALIR", new Vector2(0f, -268f), 24, topAnchor: true);

        // Hint inferior.
        Text hint = CreateText(canvas.transform, "HintLabel",
            "WASD para moverse  -  Espacio para saltar  -  ESC para volver", 19, new Color(0.75f, 0.82f, 0.9f, 0.85f));
        Anchor(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(1100f, 32f));
        hint.alignment = TextAnchor.MiddleCenter;

        // Popup CAPÍTULOS.
        GameObject chaptersPanel = CreatePopupPanel(canvas.transform, "ChaptersPanel", "SELECCIONAR CAPÍTULO", new Vector2(520f, 430f));
        Button chapter1 = CreateChapterButton(chaptersPanel, "ChapterButton1", "CAPÍTULO I\nEL BOSQUE MALDITO", new Vector2(0f, -110f), HexColor("5fc46a"));
        Button chapter2 = CreateChapterButton(chaptersPanel, "ChapterButton2", "CAPÍTULO II\nLAS MINAS DEL INFIERNO", new Vector2(0f, -178f), HexColor("f07b32"));
        Button chapter3 = CreateChapterButton(chaptersPanel, "ChapterButton3", "CAPÍTULO III\nEL CASTILLO DEL DIABLO", new Vector2(0f, -246f), HexColor("ae7cf0"));
        Button chaptersBack = CreateStyledButton(chaptersPanel.transform, "ChaptersBackButton", "VOLVER", new Vector2(0f, -340f));

        // Popup CRÉDITOS.
        GameObject creditsPanel = CreatePopupPanel(canvas.transform, "CreditsPanel", "CRÉDITOS", new Vector2(600f, 560f));
        RectTransform creditsRect = (RectTransform)creditsPanel.transform;
        creditsRect.sizeDelta = new Vector2(600f, 560f);
        AddCreditText(creditsPanel, "WHAT THE HELL? 3D", 22, HexColor("d8e3f1"), -100f);
        AddCreditText(creditsPanel, "Líder de grupo", 15, HexColor("ffc35c"), -138f);
        AddCreditText(creditsPanel, "Marlon", 17, Color.white, -162f);
        AddCreditText(creditsPanel, "Equipo", 15, HexColor("ffc35c"), -190f);
        AddCreditText(creditsPanel, "Adrian   Aldahir   Cristopher   Dylan   Iker", 14, Color.white, -214f);
        AddCreditText(creditsPanel, "Johan   Kevin   Mateo   Steven", 14, Color.white, -236f);
        AddCreditText(creditsPanel, "Desarrollo", 15, HexColor("ffc35c"), -264f);
        AddCreditText(creditsPanel, "Unity 6000 · antes Godot Engine 4", 14, Color.white, -288f);
        AddCreditText(creditsPanel, "Recursos 3D", 15, HexColor("ffc35c"), -316f);
        AddCreditText(creditsPanel, "Platformer Game Kit · Modular Medieval Buildings", 13, Color.white, -340f);
        AddCreditText(creditsPanel, "Gracias por jugar", 15, HexColor("b7c5dc"), -380f);
        Button creditsBack = CreateStyledButton(creditsPanel.transform, "CreditsBackButton", "VOLVER", new Vector2(0f, -450f));

        chaptersPanel.SetActive(false);
        creditsPanel.SetActive(false);

        MenuSceneController controller = bootstrapGo.GetComponent<MenuSceneController>()
            ?? bootstrapGo.AddComponent<MenuSceneController>();
        controller.titleText = title;
        controller.subtitleText = subtitle;
        controller.hintLabel = hint;
        controller.mainButtonsPanel = mainPanel;
        controller.newGameButton = newGame;
        controller.continueButton = continueGame;
        controller.chaptersButton = chapters;
        controller.creditsButton = credits;
        controller.quitButton = quit;
        controller.chaptersPanel = chaptersPanel;
        controller.creditsPanel = creditsPanel;
        controller.chapter1Button = chapter1;
        controller.chapter2Button = chapter2;
        controller.chapter3Button = chapter3;
        controller.chaptersBackButton = chaptersBack;
        controller.creditsBackButton = creditsBack;
        controller.musicSource = music;
        controller.clickSource = click;
        return controller;
    }

    // ------------------------------------------------------------- utilidades

    private static void ClearPreviousContent()
    {
        foreach (string containerName in new[] { "MenuWorld", "MenuAudio" })
        {
            GameObject existing = GameObject.Find(containerName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(camera.gameObject);
        }
    }

    private static Canvas NewCanvas()
    {
        GameObject go = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include) == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        return canvas;
    }

    private static void FullRectImage(Transform parent, string name, Color color)
    {
        Image image = CreateImage(parent, name, color);
        RectTransform rect = (RectTransform)image.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        image.raycastTarget = false;
    }

    private static GameObject CreatePopupPanel(Transform parent, string name, string heading, Vector2 size)
    {
        GameObject panel = UIStyleKit.CreateBorderedPanel(parent, name, UIStyleKit.PopupBg, UIStyleKit.PopupBorder, 4f);
        RectTransform rect = (RectTransform)panel.transform;
        rect.anchorMin = new Vector2(0.62f, 0.5f);
        rect.anchorMax = new Vector2(0.62f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Text title = UIStyleKit.CreateStyledText(panel.transform, "PopupTitle", heading, 24, UIStyleKit.Gold,
            FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(size.x - 40f, 40f));

        return panel;
    }

    private static Button CreateChapterButton(GameObject panel, string name, string label, Vector2 position, Color accent)
    {
        Button button = UIStyleKit.CreateStyledButton(panel.transform, name, label, position,
            new Vector2(440f, 58f), 20,
            new Color(0.07f, 0.08f, 0.11f, 0.95f), accent * 0.65f, Lighten(accent, 0.3f));
        UIButtonHover hover = button.GetComponent<UIButtonHover>();
        hover.hoverBackground = accent * 0.5f;
        hover.hoverBorder = accent;
        hover.hoverText = Lighten(accent, 0.55f);
        return button;
    }

    private static Button CreateStyledButton(Transform parent, string name, string label, Vector2 anchoredPosition,
        int fontSize = 24, bool topAnchor = false)
    {
        return UIStyleKit.CreateStyledButton(parent, name, label, anchoredPosition,
            new Vector2(400f, 54f), fontSize,
            UIStyleKit.BtnNormalBg, UIStyleKit.BtnNormalBorder, UIStyleKit.BtnText,
            alignLeft: true, topAnchor: topAnchor);
    }

    private static Color Lighten(Color color, float amount)
    {
        return new Color(
            Mathf.Clamp01(color.r + amount),
            Mathf.Clamp01(color.g + amount),
            Mathf.Clamp01(color.b + amount),
            color.a);
    }

    private static int creditLineIndex;

    private static void AddCreditText(GameObject panel, string content, int size, Color color, float y)
    {
        creditLineIndex++;
        Text text = CreateText(panel.transform, $"CreditLine{creditLineIndex}", content, size, color);
        Anchor(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(540f, 26f));
        text.alignment = TextAnchor.UpperCenter;
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
        UIStyleKit.AddShadow(text, new Vector2(2f, -2f));
        return text;
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        SetParent((RectTransform)go.transform, parent);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<Image>();
    }

    private static void AddShadow(Text target, Vector2 distance)
    {
        Shadow shadow = target.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = distance;
    }

    private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    /// <summary>Anchor con pivot.x=0 para que anchoredPosition sea el borde izquierdo del rect.</summary>
    private static void AnchorLeft(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.pivot = new Vector2(0f, rect.pivot.y);
        Anchor(rect, anchorMin, anchorMax, position, size);
    }

    private static void SetParent(RectTransform rect, Transform parent)
    {
        rect.SetParent(parent, false);
    }

    private static void RemoveExistingChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    /// <summary>
    /// Instancia un modelo y lo escala para que su altura (o dimensión mayor)
    /// mida exactamente targetSize en metros, sin importar las unidades del archivo.
    /// </summary>
    private static void SpawnNormalized(Transform parent, string relativePath, string name, Vector3 position,
        float yaw, float targetSize, string tint, bool useMaxDimension = false)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{SourceRoot}/{relativePath}");
        if (asset == null)
        {
            Debug.LogWarning($"[MenuAuthoring] Modelo no encontrado: {relativePath}");
            return;
        }

        GameObject piece = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (piece == null)
        {
            piece = Object.Instantiate(asset);
        }

        foreach (Collider collider in piece.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        // Medir bounds con la escala importada intacta.
        Bounds bounds = ComputeWorldBounds(piece);
        float current = useMaxDimension
            ? Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z))
            : bounds.size.y;
        float factor = current > 1e-5f ? targetSize / current : 1f;

        // El prefab conserva sus escalas internas (los FBX en cm traen ×100 en la raíz):
        // se envuelve en un contenedor y se escala el contenedor, nunca el prefab.
        piece.name = "Model";
        GameObject wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent, false);
        wrapper.transform.position = position;
        wrapper.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        wrapper.transform.localScale = Vector3.one * factor;
        piece.transform.SetParent(wrapper.transform, false);

        Material material = EnsureMaterial($"MenuTint_{tint}", HexColor(tint), null, Vector2.one);
        foreach (MeshRenderer renderer in piece.GetComponentsInChildren<MeshRenderer>(true))
        {
            int count = Mathf.Max(1, renderer.sharedMaterials.Length);
            Material[] slots = new Material[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = material;
            }

            renderer.sharedMaterials = slots;
        }
    }

    private static Bounds ComputeWorldBounds(GameObject root)
    {
        // Renderer.bounds es poco fiable antes del primer render (puede devolver
        // valores ×100). Se calcula el AABB exacto desde los meshes locales.
        bool any = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null)
            {
                continue;
            }

            Matrix4x4 localToWorld = filter.transform.localToWorldMatrix;
            Bounds meshBounds = filter.sharedMesh.bounds;
            Vector3 center = meshBounds.center;
            Vector3 extents = meshBounds.extents;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = localToWorld.MultiplyPoint3x4(new Vector3(
                    center.x + ((i & 1) == 0 ? -extents.x : extents.x),
                    center.y + ((i & 2) == 0 ? -extents.y : extents.y),
                    center.z + ((i & 4) == 0 ? -extents.z : extents.z)));
                if (!any)
                {
                    bounds = new Bounds(corner, Vector3.zero);
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(corner);
                }
            }
        }

        return bounds;
    }

    private static void AttachMesh(GameObject go, Mesh mesh, Material material)
    {
        MeshFilter filter = go.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = go.AddComponent<MeshFilter>();
        }

        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<MeshRenderer>();
        }

        int count = Mathf.Max(1, mesh.subMeshCount);
        Material[] slots = new Material[count];
        for (int i = 0; i < count; i++)
        {
            slots[i] = material;
        }

        renderer.sharedMaterials = slots;
    }

    private static void CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        Object.DestroyImmediate(box.GetComponent<Collider>());
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = scale;
        ApplyMaterial(box, material);
    }

    private static void AddPointLight(Transform parent, string baseName, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightGo = new GameObject(baseName, typeof(Light));
        lightGo.transform.SetParent(parent, false);
        lightGo.transform.position = position;
        Light light = lightGo.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static AudioSource CreateAudioSource(Transform parent, string name, string clipPath, bool loop, float volume)
    {
        GameObject go = new GameObject(name, typeof(AudioSource));
        go.transform.SetParent(parent, false);
        AudioSource source = go.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.volume = volume;
        source.spatialBlend = 0f;
        if (!string.IsNullOrEmpty(clipPath))
        {
            source.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
        }

        return source;
    }

    private static Mesh GetModelMesh(string modelPath)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        return asset != null ? asset.GetComponentInChildren<MeshFilter>()?.sharedMesh : null;
    }

    private static Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject temp = GameObject.CreatePrimitive(type);
        Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temp);
        return mesh;
    }

    private static Texture2D LoadTexture(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void ApplyMaterial(GameObject target, Material material)
    {
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    private static Material EnsureMaterial(string name, Color color, Texture2D texture, Vector2 tiling, bool unlit = false)
    {
        if (!AssetDatabase.IsValidFolder(MaterialsRoot))
        {
            Directory.CreateDirectory(MaterialsRoot);
            AssetDatabase.Refresh();
        }

        string path = $"{MaterialsRoot}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        string desiredShader = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
        if (material == null)
        {
            Shader shader = Shader.Find(desiredShader);
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader.name != desiredShader)
        {
            Shader shader = Shader.Find(desiredShader);
            if (shader != null) material.shader = shader;
        }

        material.color = color;
        if (texture != null)
        {
            material.mainTexture = texture;
            material.mainTextureScale = tiling;
            if (unlit)
            {
                // Unlit no usa _BaseColor tint salvo para alpha; dejar blanco para ver textura pura.
                material.color = Color.white;
            }
        }

        EditorUtility.SetDirty(material);
        return material;
    }
}
