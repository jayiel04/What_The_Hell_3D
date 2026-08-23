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
        Material statueMat = EnsureMaterial("IntroStatue", new Color(0.72f, 0.68f, 0.58f),
            LoadTexture($"{ModelsRoot}/models/statue/statue_color.jpg"), Vector2.one);
        Material houseMat = EnsureMaterial("IntroHouse", new Color(0.65f, 0.58f, 0.48f),
            LoadTexture($"{ModelsRoot}/models/small_building/small_building_color.jpg"), Vector2.one);
        Material cityMat = EnsureMaterial("IntroCityHouse", new Color(0.60f, 0.55f, 0.50f),
            LoadTexture($"{ModelsRoot}/models/city_house/city_house_color.jpg"), Vector2.one);
        Material trunk = EnsureMaterial("IntroTrunk", new Color(0.34f, 0.22f, 0.10f), null, Vector2.one);
        Material foliageA = EnsureMaterial("IntroFoliageA", new Color(0.10f, 0.37f, 0.16f), null, Vector2.one);
        Material foliageB = EnsureMaterial("IntroFoliageB", new Color(0.08f, 0.32f, 0.14f), null, Vector2.one);
        Material graveMat = EnsureMaterial("IntroGrave", new Color(0.62f, 0.63f, 0.66f), null, Vector2.one);

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
        BuildCastle(world, cityMat, houseMat, stone);
        BuildGraveyard(world, graveMat);
        BuildForest(world, trunk, foliageA, foliageB);
        BuildCastleWall(world, stone);

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
        moonGo.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

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
        GameObject statue = new GameObject("Statue");
        statue.transform.SetParent(world, false);
        statue.transform.position = new Vector3(3.2f, 0f, -7f);
        Mesh mesh = GetModelMesh($"{ModelsRoot}/models/statue/statue.obj");
        if (mesh != null)
        {
            AddMeshRenderer(statue.transform, mesh, material);
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

    private static void BuildCastle(Transform world, Material cityMat, Material towerMat, Material stone)
    {
        GameObject castle = new GameObject("Castle");
        castle.transform.SetParent(world, false);
        castle.transform.position = new Vector3(0f, 0f, -28f);
        castle.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        Mesh keepMesh = GetModelMesh($"{ModelsRoot}/models/city_house/city_house.obj");
        Mesh towerMesh = GetModelMesh($"{ModelsRoot}/models/small_building/small_building.obj");

        if (keepMesh != null)
        {
            GameObject keep = AddMeshRenderer(castle.transform, "Keep", keepMesh, cityMat);
            keep.transform.localPosition = new Vector3(2f, 0f, 0f);
            keep.transform.localRotation = Quaternion.identity;
            keep.transform.localScale = Vector3.one * 2f;
        }

        if (towerMesh != null)
        {
            foreach ((string name, Vector3 localPos, float scale) in new[]
            {
                ("TowerL", new Vector3(-2f, 0f, -5.5f), 1.3f),
                ("TowerR", new Vector3(-2f, 0f, 5.5f), 1.3f),
                ("RearL", new Vector3(3.5f, 0f, -4f), 1.1f),
                ("RearR", new Vector3(3.5f, 0f, 4f), 1.1f)
            })
            {
                GameObject tower = AddMeshRenderer(castle.transform, name, towerMesh, towerMat);
                tower.transform.localPosition = localPos;
                tower.transform.localRotation = Quaternion.identity;
                tower.transform.localScale = Vector3.one * scale;
            }
        }

        CreateBox(castle.transform, "BaseWall", new Vector3(0f, 0.5f, 0f), new Vector3(14f, 1f, 12f), stone);

        GameObject lightGo = new GameObject("CastleLight", typeof(Light));
        lightGo.transform.SetParent(castle.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 6f, 3f);
        Light light = lightGo.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.75f, 0.85f, 1f);
        light.range = 40f;
        light.intensity = 0f;
    }

    private static void BuildGraveyard(Transform world, Material material)
    {
        Transform graveyard = new GameObject("Graveyard").transform;
        graveyard.SetParent(world, false);
        for (int i = 0; i < GravePositions.Length; i++)
        {
            int variant = (i % 9) + 1;
            Mesh mesh = GetModelMesh($"{ModelsRoot}/graveyard/GraveStone_{variant}.fbx");
            GameObject grave = new GameObject($"Grave{i + 1}");
            grave.transform.SetParent(graveyard, false);
            grave.transform.position = GravePositions[i];
            if (mesh != null)
            {
                AddMeshRenderer(grave.transform, mesh, material);
            }
        }
    }

    private static void BuildForest(Transform world, Material trunk, Material foliageA, Material foliageB)
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
            treeGo.transform.localScale = Vector3.one * tree.scale;
            if (mesh != null)
            {
                AddMeshRenderer(treeGo.transform, mesh, i % 2 == 0 ? foliageA : foliageB);
            }
        }
    }

    private static void BuildCastleWall(Transform world, Material stone)
    {
        Transform wallRoot = new GameObject("CastleWall").transform;
        wallRoot.SetParent(world, false);
        Mesh moduleMesh = GetModelMesh($"{ModelsRoot}/medieval_buildings/WallBricks.fbx");
        for (int i = 0; i < WallPlacements.Length; i++)
        {
            IntroWall wall = WallPlacements[i];
            GameObject module = new GameObject($"WallModule{i + 1}");
            module.transform.SetParent(wallRoot, false);
            module.transform.position = wall.position;
            module.transform.rotation = Quaternion.Euler(0f, wall.yaw, 0f);
            module.transform.localScale = Vector3.one * 1.2f;
            if (moduleMesh != null)
            {
                AddMeshRenderer(module.transform, moduleMesh, stone);
            }
            else
            {
                CreateBox(module.transform, "Block", new Vector3(0f, 1.5f, 0f), new Vector3(3.6f, 3f, 0.8f), stone);
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
        GameObject go = new GameObject("Mesh");
        go.transform.SetParent(parent, false);
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return go;
    }

    private static GameObject AddMeshRenderer(Transform parent, string name, Mesh mesh, Material material)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
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
