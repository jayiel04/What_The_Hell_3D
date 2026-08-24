using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Autoría reproducible de la cinemática de intro 3D (Fase 9).
/// Replica el escenario, luces, audio y timeline de la escena de Godot
/// scenes/cinematics/intro/intro_story_3d.tscn.
/// Ejecutar: WhatTheHell3D > Autoría > Autoría de intro 3D (o batch: IntroSceneAuthoring.AuthorIntroScene3D)
/// </summary>
public static class IntroSceneAuthoring
{
    private const string ScenesRoot = "Assets/WhatTheHell3D/Scenes";
    private const string ModelsRoot = "Assets/WhatTheHell3D/Art/Source";
    private const string MaterialsRoot = "Assets/WhatTheHell3D/Materials/Intro";

    private struct IntroTree
    {
        public Vector3 position;
        public float scale;

        public IntroTree(Vector3 position, float scale)
        {
            this.position = position;
            this.scale = scale;
        }
    }

    private struct IntroWall
    {
        public Vector3 position;
        public float yaw;

        public IntroWall(Vector3 position, float yaw)
        {
            this.position = position;
            this.yaw = yaw;
        }
    }

    // Posiciones extraídas del .tscn original de Godot.
    private static readonly Vector3[] GravePositions =
    {
        new Vector3(8.44f, 0f, -0.50f), new Vector3(5.47f, 0f, -11.92f), new Vector3(4.81f, 0f, -3.95f),
        new Vector3(7.16f, 0f, -7.06f), new Vector3(6.85f, 0f, -12.58f), new Vector3(4.35f, 0f, -15.02f),
        new Vector3(5.12f, 0f, -11.71f), new Vector3(6.89f, 0f, -12.22f), new Vector3(5.17f, 0f, -0.39f),
        new Vector3(4.66f, 0f, -10.91f), new Vector3(7.03f, 0f, 2.29f), new Vector3(5.22f, 0f, 5.14f),
        new Vector3(-3.08f, 0f, 3.61f), new Vector3(2.46f, 0f, 2.81f), new Vector3(-2.10f, 0f, 1.91f),
        new Vector3(-4.45f, 0f, -6.31f), new Vector3(-2.39f, 0f, -0.73f), new Vector3(2.22f, 0f, 6.69f),
        new Vector3(-4.01f, 0f, -9.17f), new Vector3(3.55f, 0f, -1.99f), new Vector3(-2.58f, 0f, 6.76f),
        new Vector3(-4.32f, 0f, -11.10f), new Vector3(2.54f, 0f, -0.24f), new Vector3(2.28f, 0f, -13.21f),
        new Vector3(-3.96f, 0f, -0.78f), new Vector3(-2.27f, 0f, -15.53f), new Vector3(-2.31f, 0f, -12.14f),
        new Vector3(-4.27f, 0f, -13.92f)
    };

    private static readonly IntroTree[] TreePlacements =
    {
        new IntroTree(new Vector3(-12.38f, 0f, 2.47f), 1.60f), new IntroTree(new Vector3(-7.53f, 0f, 5.81f), 1.18f),
        new IntroTree(new Vector3(-14.46f, 0f, -33.19f), 1.65f), new IntroTree(new Vector3(-8.72f, 0f, -2.70f), 0.99f),
        new IntroTree(new Vector3(-14.63f, 0f, -12.31f), 1.23f), new IntroTree(new Vector3(-11.34f, 0f, -1.50f), 1.01f),
        new IntroTree(new Vector3(-14.36f, 0f, -23.67f), 1.50f), new IntroTree(new Vector3(-12.69f, 0f, 1.39f), 1.55f),
        new IntroTree(new Vector3(-6.12f, 0f, -4.46f), 1.69f), new IntroTree(new Vector3(-4.60f, 0f, -4.24f), 1.09f),
        new IntroTree(new Vector3(-16.06f, 0f, -30.40f), 0.96f), new IntroTree(new Vector3(-9.52f, 0f, -9.69f), 0.99f),
        new IntroTree(new Vector3(-16.04f, 0f, -1.76f), 1.32f), new IntroTree(new Vector3(-16.30f, 0f, -29.26f), 0.93f),
        new IntroTree(new Vector3(-7.50f, 0f, 6.34f), 1.70f), new IntroTree(new Vector3(-13.96f, 0f, -12.72f), 1.74f),
        new IntroTree(new Vector3(-5.99f, 0f, -10.32f), 1.15f), new IntroTree(new Vector3(-14.14f, 0f, -9.47f), 1.42f),
        new IntroTree(new Vector3(-10.33f, 0f, -4.36f), 1.04f), new IntroTree(new Vector3(-6.09f, 0f, -13.53f), 0.92f),
        new IntroTree(new Vector3(-12.68f, 0f, -6.78f), 1.72f), new IntroTree(new Vector3(-5.52f, 0f, -4.69f), 1.56f),
        new IntroTree(new Vector3(-16.26f, 0f, -20.74f), 0.94f), new IntroTree(new Vector3(-15.04f, 0f, -6.00f), 1.29f),
        new IntroTree(new Vector3(-10.91f, 0f, -10.09f), 1.00f), new IntroTree(new Vector3(-6.97f, 0f, -6.85f), 1.42f),
        new IntroTree(new Vector3(-13.46f, 0f, -5.13f), 1.07f), new IntroTree(new Vector3(-13.23f, 0f, -10.62f), 0.95f),
        new IntroTree(new Vector3(-8.07f, 0f, -8.49f), 1.40f), new IntroTree(new Vector3(-15.39f, 0f, -29.83f), 1.78f),
        new IntroTree(new Vector3(-11.75f, 0f, 3.83f), 1.66f), new IntroTree(new Vector3(-4.81f, 0f, 9.66f), 1.22f),
        new IntroTree(new Vector3(-12.83f, 0f, -15.14f), 1.16f), new IntroTree(new Vector3(-9.74f, 0f, -8.54f), 0.91f),
        new IntroTree(new Vector3(-14.03f, 0f, -25.94f), 1.62f), new IntroTree(new Vector3(-13.07f, 0f, 0.97f), 1.18f),
        new IntroTree(new Vector3(-12.61f, 0f, -4.87f), 1.79f), new IntroTree(new Vector3(-16.31f, 0f, -31.78f), 1.37f),
        new IntroTree(new Vector3(-10.22f, 0f, -13.32f), 1.11f)
    };

    private static readonly IntroWall[] WallPlacements =
    {
        new IntroWall(new Vector3(1.60f, 0f, -18.00f), 0f), new IntroWall(new Vector3(5.80f, 0f, -18.00f), 0f),
        new IntroWall(new Vector3(8.80f, 0f, -18.00f), 0f), new IntroWall(new Vector3(-13.00f, 0f, -18.00f), 0f),
        new IntroWall(new Vector3(-8.80f, 0f, -18.00f), 0f), new IntroWall(new Vector3(-5.80f, 0f, -18.00f), 0f),
        new IntroWall(new Vector3(13.00f, 0f, -18.00f), 0f), new IntroWall(new Vector3(13.00f, 0f, -22.20f), 90f),
        new IntroWall(new Vector3(13.00f, 0f, -26.40f), 90f), new IntroWall(new Vector3(13.00f, 0f, -30.60f), 90f),
        new IntroWall(new Vector3(13.00f, 0f, -31.80f), 90f), new IntroWall(new Vector3(-13.00f, 0f, -22.20f), 90f),
        new IntroWall(new Vector3(-13.00f, 0f, -26.40f), 90f), new IntroWall(new Vector3(-13.00f, 0f, -30.60f), 90f),
        new IntroWall(new Vector3(-13.00f, 0f, -31.80f), 90f), new IntroWall(new Vector3(-13.00f, 0f, -36.00f), 90f),
        new IntroWall(new Vector3(-8.80f, 0f, -36.00f), 0f), new IntroWall(new Vector3(-4.60f, 0f, -36.00f), 0f),
        new IntroWall(new Vector3(-0.40f, 0f, -36.00f), 0f), new IntroWall(new Vector3(3.80f, 0f, -36.00f), 0f),
        new IntroWall(new Vector3(8.00f, 0f, -36.00f), 0f), new IntroWall(new Vector3(8.80f, 0f, -36.00f), 0f),
        new IntroWall(new Vector3(13.00f, 0f, -36.00f), 90f), new IntroWall(new Vector3(-1.60f, 0f, -18.00f), 0f)
    };

    [MenuItem("WhatTheHell3D/Autoría/Autoría de intro 3D")]
    public static void AuthorIntroScene3DFromMenu()
    {
        AuthorIntroScene3D();
    }

    public static void AuthorIntroScene3D()
    {
        EditorSceneManager.OpenScene($"{ScenesRoot}/Intro.unity", OpenSceneMode.Single);
        SceneBootstrap bootstrap = Object.FindFirstObjectByType<SceneBootstrap>(FindObjectsInactive.Include);
        if (bootstrap == null)
        {
            Debug.LogError("[IntroAuthoring] La escena Intro no contiene SceneBootstrap.");
            return;
        }

        ClearPreviousContent();

        // ---------------------------------------------------------- materiales
        Material cobble = EnsureMaterial("IntroGround", new Color(0.62f, 0.62f, 0.64f),
            LoadTexture($"{ModelsRoot}/textures/cobblestone_floor/cobblestone_diff.jpg"), new Vector2(40f, 240f));
        Material stone = EnsureMaterial("IntroStone", new Color(0.55f, 0.56f, 0.60f), null, Vector2.one);
        Material statueMat = EnsureMaterial("IntroStatue", Color.white,
            LoadTexture($"{ModelsRoot}/models/statue/statue_color.jpg"), Vector2.one);
        // Colores reales del Tree1.mtl de Godot: hojas Kd(0.52,0.64,0.18) y tronco Kd(0.20,0.09,0.04).
        Material leaves = EnsureMaterial("IntroLeaves", new Color(0.52f, 0.64f, 0.18f), null, Vector2.one);
        Material trunk = EnsureMaterial("IntroTrunk", new Color(0.20f, 0.09f, 0.04f), null, Vector2.one);
        Material graveMat = EnsureMaterial("IntroGrave", Color.white,
            LoadTexture($"{ModelsRoot}/graveyard/textures/stone1_albedo.png"), Vector2.one);
        Material wallStone = EnsureMaterial("IntroWallStone", Color.white,
            LoadTexture($"{ModelsRoot}/models/stone_wall/wall_stone.jpg"), new Vector2(2f, 2f));

        // -------------------------------------------------------------- mundo
        Transform world = NewContainer("World");

        // Piso (Godot: caja escalada 50×0.2×300 en y=-0.11,z=-14 con adoquín).
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        Object.DestroyImmediate(ground.GetComponent<Collider>());
        ground.transform.SetParent(world, false);
        ground.transform.position = new Vector3(0f, -0.22f, -14f);
        ground.transform.localScale = new Vector3(100f, 0.44f, 600f);
        ApplyMaterial(ground, cobble);

        // Backdrop de colinas (Godot: quad en y=22,z=-58 mirando a cámara).
        GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = "Backdrop";
        Object.DestroyImmediate(backdrop.GetComponent<Collider>());
        backdrop.transform.SetParent(world, false);
        backdrop.transform.position = new Vector3(0f, 22f, -58f);
        backdrop.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        backdrop.transform.localScale = new Vector3(160f, 44f, 1f);
        Material backdropMat = EnsureMaterial("IntroBackdrop", Color.white,
            LoadTexture($"{ModelsRoot}/environments/intro3d/backdrop_hills.png"), Vector2.one, unlit: true);
        ApplyMaterial(backdrop, backdropMat);

        BuildStatue(world, statueMat);
        BuildGate(world, stone);
        BuildCastle(world);
        BuildGraveyard(world, graveMat);
        BuildForest(world, trunk, leaves);
        BuildCastleWall(world, wallStone);

        // Caballero cinemático (el modelo glTF se instancia en runtime a escala 0.5).
        GameObject playerHolder = new GameObject("Player");
        playerHolder.transform.SetParent(world, false);
        playerHolder.transform.position = Vector3.zero;

        // ------------------------------------------------------------ luces
        GameObject moonGo = new GameObject("MoonLight", typeof(Light));
        Light moon = moonGo.GetComponent<Light>();
        moon.type = LightType.Directional;
        moon.color = new Color(0.87f, 0.91f, 1f);
        moon.intensity = 0f;
        moon.shadows = LightShadows.Soft;
        // Compensa la convención de dirección de luz (Godot −Z vs Unity +Z)
        // para que ilumine la fachada de la puerta/castillo.
        moonGo.transform.rotation = Quaternion.Euler(45f, 155f, 0f);

        // ------------------------------------------------------------ cámara
        GameObject cameraGo = new GameObject("MainCamera", typeof(Camera), typeof(AudioListener));
        cameraGo.tag = "MainCamera";
        Camera cameraComponent = cameraGo.GetComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
        cameraComponent.fieldOfView = 70f;
        cameraGo.transform.position = new Vector3(0f, 3.5f, 7f);
        // En Godot las cámaras miran hacia −Z; en Unity el forward es +Z,
        // así que se gira 180° para que vea puerta/castillo/estatua (z negativos).
        cameraGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Ambiente y niebla (Godot Environment: fog 0.006, ambient 0.35/0.42/0.38 @0.55).
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.19f, 0.23f, 0.21f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.006f;
        RenderSettings.fogColor = new Color(0.5f, 0.55f, 0.5f);

        // ------------------------------------------------------------- audio
        Transform audio = NewContainer("Audio");
        AudioSource ambience = CreateAudioSource(audio, "Ambience",
            "Assets/WhatTheHell3D/Audio/Source/ambience/wind.mp3", loop: true, volume: 0.8f);
        AudioSource bell = CreateAudioSource(audio, "Bell",
            $"Assets/WhatTheHell3D/Audio/Source/sfx/bell.mp3", loop: true, volume: 0.4f);
        AudioSource breath = CreateAudioSource(audio, "Breath",
            "Assets/WhatTheHell3D/Audio/Source/sfx/breath.mp3", loop: false, volume: 0.9f);
        AudioSource footsteps = CreateAudioSource(audio, "Footsteps",
            "Assets/WhatTheHell3D/Audio/Source/sfx/footsteps_stone.mp3", loop: false, volume: 0.5f);
        AudioSource voice = CreateAudioSource(audio, "Voice", null, loop: false, volume: 1f);

        // ---------------------------------------------------------------- UI
        // Fondo null: el canvas NO debe tapar el mundo 3D (solo subtítulos, fade y skip).
        Canvas canvas = CampaignAuthoringTools.EnsureCanvasPublic("UICanvas", null);
        Transform staleBackground = canvas.transform.Find("Background");
        if (staleBackground != null)
        {
            Object.DestroyImmediate(staleBackground.gameObject);
        }

        Text subtitle = CampaignAuthoringTools.CreateTextPublic(canvas.transform, "SubtitleText", "", 30, Color.white);
        CampaignAuthoringTools.AnchorPublic(subtitle.rectTransform,
            new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(900f, 120f));
        subtitle.alignment = TextAnchor.MiddleCenter;

        Image fade = CampaignAuthoringTools.CreateImagePublic(canvas.transform, "FadeImage", new Color(0f, 0f, 0f, 1f));
        RectTransform fadeRect = (RectTransform)fade.transform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        fade.raycastTarget = false;
        fade.transform.SetAsLastSibling();

        Button skip = CampaignAuthoringTools.CreateButtonPublic(canvas.transform, "SkipButton", "Enter para omitir", Vector2.zero);
        RectTransform skipRect = (RectTransform)skip.transform;
        skipRect.anchorMin = new Vector2(1f, 0f);
        skipRect.anchorMax = new Vector2(1f, 0f);
        skipRect.anchoredPosition = new Vector2(-170f, 44f);
        skip.transform.SetAsLastSibling();

        // --------------------------------------------------------- director
        IntroCutsceneDirector director = bootstrap.gameObject.GetComponent<IntroCutsceneDirector>();
        if (director == null)
        {
            director = bootstrap.gameObject.AddComponent<IntroCutsceneDirector>();
        }

        director.player = playerHolder.transform;
        director.moonLight = moon;
        director.gateGlow = GameObject.Find("World/Gate/Glow").GetComponent<Light>();
        director.castleLight = GameObject.Find("World/Castle/CastleLight").GetComponent<Light>();
        director.cutsceneCamera = cameraComponent;
        director.subtitleText = subtitle;
        director.fadeImage = fade;
        director.skipButton = skip;
        director.ambience = ambience;
        director.bell = bell;
        director.breath = breath;
        director.footsteps = footsteps;
        director.voice = voice;
        director.characterModelPath = "Characters/Knight_Male.gltf";
        director.nextScenePath = $"{ScenesRoot}/CampaignLevel01.unity";
        director.lines = BuildLines();
        EditorUtility.SetDirty(director);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[IntroAuthoring] Cinemática de intro 3D autorada y guardada.");
    }

    private static void ClearPreviousContent()
    {
        foreach (string containerName in new[] { "World", "MoonLight", "Audio", "IntroVoiceSource" })
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

        // Eliminar restos del controlador de intro antiguo si existiera.
        GameObject legacy = GameObject.Find("CampaignScene");
        if (legacy != null)
        {
            foreach (MonoBehaviour behaviour in legacy.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null)
                {
                    Object.DestroyImmediate(behaviour);
                }
            }
        }
    }

    private static Transform NewContainer(string name)
    {
        GameObject go = new GameObject(name);
        return go.transform;
    }

    private static void BuildStatue(Transform world, Material material)
    {
        // statue.obj mide ~132 m de nativo: se normaliza a 3 m de alto.
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsRoot}/models/statue/statue.obj");
        if (asset == null)
        {
            Debug.LogWarning("[IntroAuthoring] statue.obj no encontrado.");
            return;
        }

        GameObject statue = PrefabUtility.InstantiatePrefab(asset) as GameObject
            ?? Object.Instantiate(asset);
        statue.name = "Statue";
        foreach (Collider col in statue.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(col);
        }

        statue.transform.SetParent(world, false);
        statue.transform.position = new Vector3(3.2f, 0f, -7f);
        Bounds bounds = ComputeWorldBounds(statue);
        float factor = bounds.size.y > 1e-5f ? 3f / bounds.size.y : 1f;
        statue.transform.localScale = Vector3.one * factor;
        foreach (MeshRenderer renderer in statue.GetComponentsInChildren<MeshRenderer>(true))
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void BuildGate(Transform world, Material stone)
    {
        GameObject gate = new GameObject("Gate");
        gate.transform.SetParent(world, false);
        gate.transform.position = new Vector3(0f, 0f, -18f);
        gate.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        CreateBox(gate.transform, "PillarL", new Vector3(0f, 1.75f, -1.2f), new Vector3(0.6f, 3.5f, 0.6f), stone);
        CreateBox(gate.transform, "PillarR", new Vector3(0f, 1.75f, 1.2f), new Vector3(0.6f, 3.5f, 0.6f), stone);
        CreateBox(gate.transform, "Arch", new Vector3(0f, 3.6f, 0f), new Vector3(3.4f, 0.5f, 0.6f), stone);

        GameObject glowGo = new GameObject("Glow", typeof(Light));
        glowGo.transform.SetParent(gate.transform, false);
        glowGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        Light glow = glowGo.GetComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1f, 0.82f, 0.5f);
        glow.range = 14f;
        glow.intensity = 0f;
    }

    private static void BuildCastle(Transform world)
    {
        // Castillo ORIGINAL del menú de Godot (main_menu.gd::_build_world), replicado
        // con los mismos assets medievales, rotaciones Y=180°, escalas y tintes planos.
        GameObject castle = new GameObject("Castle");
        castle.transform.SetParent(world, false);
        castle.transform.position = new Vector3(0f, 0f, -22f);

        Material wallTint = EnsureMaterial("IntroCastleWall", HexColor("b9c2ce"), null, Vector2.one);
        Material doorTint = EnsureMaterial("IntroCastleDoor", HexColor("7d5635"), null, Vector2.one);
        Material bridgeTint = EnsureMaterial("IntroCastleBridge", HexColor("9a7a52"), null, Vector2.one);
        Material wellTint = EnsureMaterial("IntroCastleWell", HexColor("6e7e8f"), null, Vector2.one);
        Material largeTowerTint = EnsureMaterial("IntroCastleLargeTower", HexColor("c6ccd6"), null, Vector2.one);
        Material pointyTint = EnsureMaterial("IntroCastlePointy", HexColor("b7bec9"), null, Vector2.one);
        Material watchTint = EnsureMaterial("IntroCastleWatch", HexColor("d7c7ae"), null, Vector2.one);
        Material bannerTint = EnsureMaterial("IntroCastleBanner", HexColor("caa24a"), null, Vector2.one);

        // Alturas objetivo (metros) según la referencia visual del menú de Godot.
        SpawnNormalized(castle.transform, "medieval_buildings/WallEntrance.fbx", "Entrance",
            new Vector3(0f, 0f, -9f), 180f, 4.5f, new[] { wallTint });
        SpawnNormalized(castle.transform, "medieval_buildings/Door.fbx", "Door",
            new Vector3(0f, 0.1f, -8.75f), 180f, 2.6f, new[] { doorTint });
        SpawnNormalized(castle.transform, "medieval_buildings/Bridge.fbx", "Bridge",
            new Vector3(0f, 0f, -2.6f), 0f, 4f, new[] { bridgeTint }, useMaxDimension: true);
        SpawnNormalized(castle.transform, "medieval_buildings/Well.fbx", "Well",
            new Vector3(-6f, 0f, -2f), 0f, 1.6f, new[] { wellTint });

        foreach (float side in new[] { -1f, 1f })
        {
            string suffix = side < 0 ? "L" : "R";
            SpawnNormalized(castle.transform, "medieval_buildings/LargeSquareTower.fbx", $"LargeTower{suffix}",
                new Vector3(side * 7f, 0f, -10.5f), 180f, 8f, new[] { largeTowerTint });
            SpawnNormalized(castle.transform, "medieval_buildings/PointyTower.fbx", $"PointyTower{suffix}",
                new Vector3(side * 12f, 0f, -13f), 180f, 11f, new[] { pointyTint });
            SpawnNormalized(castle.transform, "medieval_buildings/WatchTowerWRoof.fbx", $"WatchTower{suffix}",
                new Vector3(side * 13.5f, 0f, -5f), 180f, 6.5f, new[] { watchTint });
            SpawnNormalized(castle.transform, "medieval_buildings/Banner.fbx", $"Banner{suffix}",
                new Vector3(side * 3.8f, 2.8f, -8.55f), 180f, 2.2f, new[] { bannerTint });
        }

        foreach (float x in new[] { -10.5f, -8.8f, -5f, -3.2f, 3.2f, 5f, 8.8f, 10.5f })
        {
            SpawnNormalized(castle.transform, "medieval_buildings/WallBricks.fbx", $"WallBrick{x:0.#}",
                new Vector3(x, 0f, -10f), 180f, 3.6f, new[] { wallTint });
        }

        GameObject lightGo = new GameObject("CastleLight", typeof(Light));
        lightGo.transform.SetParent(castle.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 6f, 4f);
        Light light = lightGo.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.75f, 0.85f, 1f);
        light.range = 45f;
        light.intensity = 0f;
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    /// <summary>Instancia un modelo y lo escala a targetSize (metros) midiendo sus bounds reales.</summary>
    private static void SpawnNormalized(Transform parent, string relativePath, string name, Vector3 localPos,
        float yaw, float targetSize, Material[] materials, bool useMaxDimension = false)
    {
        string path = $"{ModelsRoot}/{relativePath}";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning($"[IntroAuthoring] Modelo no encontrado: {path}");
            return;
        }

        GameObject piece = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (piece == null)
        {
            piece = Object.Instantiate(asset);
        }

        foreach (Collider col in piece.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(col);
        }

        Bounds bounds = ComputeWorldBounds(piece);
        float current = useMaxDimension
            ? Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z))
            : bounds.size.y;
        float factor = current > 1e-5f ? targetSize / current : 1f;

        // El prefab conserva sus escalas internas (FBX en cm traen ×100 en la raíz):
        // se envuelve en un contenedor y se escala el contenedor, nunca el prefab.
        piece.name = "Model";
        GameObject wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent, false);
        wrapper.transform.localPosition = localPos;
        wrapper.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        wrapper.transform.localScale = Vector3.one * factor;
        piece.transform.SetParent(wrapper.transform, false);

        foreach (MeshRenderer renderer in piece.GetComponentsInChildren<MeshRenderer>(true))
        {
            int count = Mathf.Max(1, renderer.sharedMaterials.Length);
            Material[] slots = new Material[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = materials[i % materials.Length];
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

    private static void BuildGraveyard(Transform world, Material material)
    {
        Transform graveyard = new GameObject("Graveyard").transform;
        graveyard.SetParent(world, false);
        for (int i = 0; i < GravePositions.Length; i++)
        {
            int variant = (i % 9) + 1;
            string path = $"{ModelsRoot}/graveyard/GraveStone_{variant}.fbx";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject grave = new GameObject($"Grave{i + 1}");
            grave.transform.SetParent(graveyard, false);
            grave.transform.position = GravePositions[i];

            
            if (asset == null)
            {
                Debug.LogWarning($"[IntroAuthoring] Tumba no encontrada: {path}");
                continue;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) instance = Object.Instantiate(asset);
            instance.name = "Model";
            foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(col);
            }

            Bounds graveBounds = ComputeWorldBounds(instance);
            float graveFactor = graveBounds.size.y > 1e-5f ? 1.3f / graveBounds.size.y : 1f;
            instance.transform.SetParent(grave.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            grave.transform.localScale = Vector3.one * graveFactor;

            foreach (MeshRenderer renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                int count = Mathf.Max(1, renderer.sharedMaterials.Length);
                Material[] slots = new Material[count];
                for (int j = 0; j < count; j++) slots[j] = material;
                renderer.sharedMaterials = slots;
            }
        }
    }

    private static void BuildForest(Transform world, Material trunk, Material leaves)
    {
        Transform forest = new GameObject("Forest").transform;
        forest.SetParent(world, false);
        for (int i = 0; i < TreePlacements.Length; i++)
        {
            IntroTree tree = TreePlacements[i];
            int variant = (i % 4) + 1;
            Mesh mesh = GetModelMesh($"{ModelsRoot}/nature_pack/Tree{variant}.obj");
            GameObject treeGo = new GameObject($"Tree{i + 1}");
            treeGo.transform.SetParent(forest, false);
            treeGo.transform.position = tree.position;
            if (mesh != null)
            {
                // Los OBJ de Godot traen MTL multi-material: submesh 0 = hojas, 1 = tronco.
                AddMeshRenderer(treeGo.transform, mesh, new[] { leaves, trunk });
                Bounds treeBounds = ComputeWorldBounds(treeGo);
                float treeFactor = treeBounds.size.y > 1e-5f ? tree.scale * 3f / treeBounds.size.y : 1f;
                treeGo.transform.localScale = Vector3.one * treeFactor;
            }
        }
    }

    private static void BuildCastleWall(Transform world, Material wallStone)
    {
        Transform wallRoot = new GameObject("CastleWall").transform;
        wallRoot.SetParent(world, false);
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsRoot}/medieval_buildings/WallBricks.fbx");
        for (int i = 0; i < WallPlacements.Length; i++)
        {
            IntroWall wall = WallPlacements[i];
            GameObject module = new GameObject($"WallModule{i + 1}");
            module.transform.SetParent(wallRoot, false);
            module.transform.position = wall.position;
            module.transform.rotation = Quaternion.Euler(0f, wall.yaw, 0f);
            if (asset != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
                if (instance == null) instance = Object.Instantiate(asset);
                instance.name = "Model";
                foreach (Collider col in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(col);
                Bounds wallBounds = ComputeWorldBounds(instance);
                float wallFactor = wallBounds.size.y > 1e-5f ? 3.6f / wallBounds.size.y : 1f;
                instance.transform.SetParent(module.transform, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                module.transform.localScale = Vector3.one * wallFactor;
                foreach (MeshRenderer renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                {
                    int count = Mathf.Max(1, renderer.sharedMaterials.Length);
                    Material[] slots = new Material[count];
                    for (int j = 0; j < count; j++) slots[j] = wallStone;
                    renderer.sharedMaterials = slots;
                }
            }
            else
            {
                CreateBox(module.transform, "Block", new Vector3(0f, 1.5f, 0f), new Vector3(3.6f, 3f, 0.8f), wallStone);
            }
        }
    }

    private static IntroCutsceneDirector.VoiceLine[] BuildLines()
    {
        (string text, float duration, string clip)[] definitions =
        {
            ("Hubo un tiempo en que la luz protegia estas tierras...", 3.0f, "s1_line1"),
            ("Hasta que una sombra la devoro por completo.", 3.0f, "s1_line2"),
            ("Los reinos cayeron uno tras otro.", 1.8f, "s2_line1"),
            ("Sus guardianes lucharon... y desaparecieron.", 1.8f, "s2_line2"),
            ("Solo quedaron ruinas... y silencio.", 2.2f, "s2_line3"),
            ("No recuerdas quien eres...", 2.2f, "s3_line1"),
            ("Ni por que despertaste aqui.", 2.4f, "s3_line2"),
            ("Muchos caminaron este sendero...", 1.8f, "s4_line1"),
            ("Ninguno consiguio regresar.", 1.8f, "s4_line2"),
            ("Pero mientras una llama siga encendida...", 2.2f, "s5_line1"),
            ("...la oscuridad nunca habra vencido.", 2.6f, "s5_line2"),
            ("Si aun respiras...", 2.2f, "s6_line1"),
            ("...es porque el destino aun no ha terminado contigo.", 2.8f, "s6_line2"),
            ("Levantate.", 1.8f, "s7_line1"),
            ("La ultima esperanza... camina contigo.", 3.0f, "s7_line2")
        };

        IntroCutsceneDirector.VoiceLine[] result = new IntroCutsceneDirector.VoiceLine[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            result[i] = new IntroCutsceneDirector.VoiceLine
            {
                text = definitions[i].text,
                duration = definitions[i].duration,
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    $"Assets/WhatTheHell3D/Audio/Source/dialogue/{definitions[i].clip}.mp3")
            };
        }

        return result;
    }

    // ----------------------------------------------------------- utilidades

    private static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        Object.DestroyImmediate(box.GetComponent<Collider>());
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localScale = scale;
        ApplyMaterial(box, material);
        return box;
    }

    private static GameObject AddMeshRenderer(Transform parent, Mesh mesh, Material material)
    {
        return AddMeshRenderer(parent, "Mesh", mesh, new[] { material });
    }

    private static GameObject AddMeshRenderer(Transform parent, Mesh mesh, Material[] materials)
    {
        return AddMeshRenderer(parent, "Mesh", mesh, materials);
    }

    private static GameObject AddMeshRenderer(Transform parent, string name, Mesh mesh, Material material)
    {
        return AddMeshRenderer(parent, name, mesh, new[] { material });
    }

    /// <summary>Asigna materiales cíclicamente a todos los submeshes (respetando MTL multi-material de los OBJ).</summary>
    private static GameObject AddMeshRenderer(Transform parent, string name, Mesh mesh, Material[] materials)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        int count = mesh != null ? Mathf.Max(1, mesh.subMeshCount) : 1;
        Material[] slots = new Material[count];
        for (int i = 0; i < count; i++)
        {
            slots[i] = materials[i % materials.Length];
        }

        renderer.sharedMaterials = slots;
        return go;
    }

    private static void ApplyMaterial(GameObject target, Material material)
    {
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
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
        if (asset == null)
        {
            Debug.LogWarning($"[IntroAuthoring] Modelo no encontrado: {modelPath}");
            return null;
        }

        MeshFilter filter = asset.GetComponentInChildren<MeshFilter>();
        return filter != null ? filter.sharedMesh : null;
    }

    private static Texture2D LoadTexture(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
        if (material == null)
        {
            Shader shader = Shader.Find(unlit
                ? "Universal Render Pipeline/Unlit"
                : "Universal Render Pipeline/Lit");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (unlit)
        {
            if (texture != null)
            {
                material.mainTexture = texture;
            }
        }
        else
        {
            material.color = color;
            if (texture != null)
            {
                material.mainTexture = texture;
                material.mainTextureScale = tiling;
            }
        }

        EditorUtility.SetDirty(material);
        return material;
    }
}
