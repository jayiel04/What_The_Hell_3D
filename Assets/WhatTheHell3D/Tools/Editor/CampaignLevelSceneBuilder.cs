using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Reconstruye las escenas de campaña como escenas authoradas: crea todos los objetos
/// persistentes (geometría, jugador, enemigos, pickups, hazards, checkpoints, meta,
/// cámaras, luces y UI) desde los CampaignLevelConfig y guarda la escena serializada.
/// </summary>
public static class CampaignLevelSceneBuilder
{
    private const string ScenesRoot = "Assets/WhatTheHell3D/Scenes";
    private const string MaterialsRoot = "Assets/WhatTheHell3D/Materials";
    private const string DataRoot = "Assets/WhatTheHell3D/Data";

    private static readonly string[] LevelScenes =
    {
        "CampaignLevel01",
        "CampaignLevel02",
        "CampaignLevel03"
    };

    [MenuItem("WhatTheHell3D/Autoría/Reconstruir niveles de campaña desde configs")]
    public static void BuildAllFromMenu()
    {
        BuildAll();
    }

    public static void BuildAll()
    {
        foreach (string sceneName in LevelScenes)
        {
            BuildLevel(sceneName);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Builder] Tres escenas de campaña reconstruidas y guardadas.");
    }

    private static void BuildLevel(string sceneName)
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cargar los assets DESPUÉS de NewScene: el cambio de escena descarga
        // assets sin referencias en memoria.
        CampaignLevelConfig config = LoadConfigFor(sceneName);
        if (config == null)
        {
            Debug.LogError($"[Builder] No se encontró config para {sceneName}.");
            return;
        }

        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/WhatTheHell3D/Prefabs/WitchProjectile.prefab");
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/WhatTheHell3D/Audio/WhatTheHellMixer.mixer");
        Transform levelRoot = new GameObject("LevelContent").transform;

        // ------------------------------------------------------------- raíz
        GameObject root = new GameObject("CampaignScene");
        SceneBootstrap bootstrap = root.AddComponent<SceneBootstrap>();
        bootstrap.role = RuntimeSceneRole.CampaignLevel;
        bootstrap.levelConfig = config;
        bootstrap.sceneCatalog = AssetDatabase.LoadAssetAtPath<CampaignSceneCatalog>($"{DataRoot}/CampaignSceneCatalog.asset");
        bootstrap.inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");

        CampaignLevelRuntime runtime = root.AddComponent<CampaignLevelRuntime>();
        CampaignHudController hud = root.AddComponent<CampaignHudController>();
        PauseController pause = root.AddComponent<PauseController>();
        CampaignAudioDirector audioDirector = root.AddComponent<CampaignAudioDirector>();

        // ------------------------------------------------------------ jugador
        PlayerController player = CreatePlayer(config);
        player.transform.SetParent(levelRoot, true);

        // ------------------------------------------------------------- cámara
        CameraController cameraController = CreateCamera(player.transform);

        // -------------------------------------------------------------- luces
        Light keyLight = CreateLight("KeyLight", config.ambientLightColor, config.keyLightEnergy, new Vector3(48f, -32f, 0f));
        keyLight.transform.SetParent(levelRoot, true);
        Light fillLight = CreateLight("FillLight", config.fillLightColor, config.fillLightEnergy, new Vector3(25f, 145f, 0f));
        fillLight.transform.SetParent(levelRoot, true);

        // ---------------------------------------------------------- geometría
        BuildGeometry(config, levelRoot);
        BuildHazards(config, levelRoot);
        BuildPickups(config, levelRoot);
        BuildCheckpoints(config, levelRoot);
        BuildEnemies(config, levelRoot);
        BuildGoal(config, levelRoot);

        // ------------------------------------------------ referencias runtime
        runtime.player = player;
        runtime.cameraController = cameraController;
        runtime.keyLight = keyLight;
        runtime.fillLight = fillLight;
        runtime.hud = hud;
        runtime.pause = pause;
        runtime.audioDirector = audioDirector;
        EditorUtility.SetDirty(runtime);

        // ------------------------------------------- HUD, pausa, audio, navmesh
        CampaignAuthoringTools.AuthorHud(hud);
        CampaignAuthoringTools.AuthorPausePanel(pause);
        CampaignAuthoringTools.AuthorAudio(audioDirector, player, mixer, sceneName);
        CampaignAuthoringTools.AssignProjectilePrefabToWitches(projectilePrefab);
        CampaignAuthoringTools.AuthorNavMesh(sceneName);

        EditorSceneManager.SaveScene(scene, $"{ScenesRoot}/{sceneName}.unity");
        Debug.Log($"[Builder] {sceneName} construida desde {config.name}: {levelRoot.childCount} hijos en LevelContent.");
    }

    private static CampaignLevelConfig LoadConfigFor(string sceneName)
    {
        switch (sceneName)
        {
            case "CampaignLevel01": return AssetDatabase.LoadAssetAtPath<CampaignLevelConfig>($"{DataRoot}/CampaignLevel01_Forest.asset");
            case "CampaignLevel02": return AssetDatabase.LoadAssetAtPath<CampaignLevelConfig>($"{DataRoot}/CampaignLevel02_Mines.asset");
            default: return AssetDatabase.LoadAssetAtPath<CampaignLevelConfig>($"{DataRoot}/CampaignLevel03_Castle.asset");
        }
    }

    // ---------------------------------------------------------------- actores

    private static PlayerController CreatePlayer(CampaignLevelConfig config)
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.tag = "Player";
        SetLayer(playerObject, "Player");
        playerObject.transform.position = config.playerStart;

        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.42f;
        controller.center = Vector3.up * 0.9f;
        controller.stepOffset = 0.45f;
        controller.slopeLimit = 50f;

        HealthComponent health = playerObject.AddComponent<HealthComponent>();
        health.maxHealth = 100;
        return playerObject.AddComponent<PlayerController>();
    }

    private static CameraController CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 300f;
        cameraObject.AddComponent<AudioListener>();
        CameraController controller = cameraObject.AddComponent<CameraController>();
        return controller;
    }

    private static Light CreateLight(string name, Color color, float intensity, Vector3 eulerRotation)
    {
        GameObject lightObject = new GameObject(name);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        lightObject.transform.rotation = Quaternion.Euler(eulerRotation);
        return light;
    }

    // ------------------------------------------------------------- contenido

    private static void BuildGeometry(CampaignLevelConfig config, Transform parent)
    {
        foreach (CampaignPlatformPlacement placement in config.platforms)
        {
            CreateCube("Platform", placement.position, placement.size, MaterialRole.Terrain, parent, "Ground");
        }

        foreach (CampaignPlatformPlacement placement in config.bridges)
        {
            CreateCube("Bridge", placement.position, placement.size, MaterialRole.Terrain, parent, "Ground");
        }

        foreach (CampaignForestIslandPlacement placement in config.forestIslandBases)
        {
            CreateCube("ForestIsland_" + placement.variant, placement.position, placement.size, MaterialRole.Terrain, parent, "Ground");
        }

        foreach (CampaignMovingPlatformPlacement placement in config.movingPlatforms)
        {
            GameObject platform = CreateCube("MovingPlatform", placement.position, placement.size, MaterialRole.Pickup, parent, "Ground");
            MovingPlatformRuntime mover = platform.AddComponent<MovingPlatformRuntime>();
            mover.travel = placement.travel;
            mover.duration = placement.duration;
        }

        foreach (CampaignPlatformPlacement placement in config.fallingPlatforms)
        {
            GameObject platform = CreateCube("FallingPlatform", placement.position, placement.size, MaterialRole.Hazard, parent, "Ground");
            FallingPlatformRuntime faller = platform.AddComponent<FallingPlatformRuntime>();
            faller.platform = platform.transform;
            GameObject trigger = CreateTriggerCube("FallingPlatformTrigger", placement.position + Vector3.up * 0.6f,
                new Vector3(placement.size.x * 0.9f, 1.4f, placement.size.z * 0.9f), MaterialRole.Terrain, parent, "Ground");
            trigger.transform.SetParent(platform.transform, true);
        }

        foreach (CampaignStairPlacement stair in config.stairs)
        {
            for (int i = 0; i < stair.count; i++)
            {
                Vector3 position = stair.start + new Vector3(i * 1.5f, i * stair.step, 0f);
                CreateCube("Stair", position, new Vector3(1.6f, 0.6f + i * 0.05f, 3.4f), MaterialRole.Terrain, parent, "Ground");
            }
        }

        foreach (CampaignVolumePlacement lava in config.lava)
        {
            GameObject lavaObject = CreateTriggerCube("LavaVolume", lava.position, lava.size, MaterialRole.Hazard, parent, "Hazard");
            HazardRuntime hazard = lavaObject.AddComponent<HazardRuntime>();
            hazard.kind = CampaignHazardKind.Lava;
            hazard.damage = 40;
        }

        float minX = Mathf.Min(config.playerStart.x, config.goalPosition.x) - 30f;
        float maxX = Mathf.Max(config.playerStart.x, config.goalPosition.x) + 30f;
        GameObject voidVolume = CreateTriggerCube("OutOfBounds",
            new Vector3((minX + maxX) * 0.5f, config.playerStart.y - 12f, 0f),
            new Vector3(maxX - minX, 1f, 80f), MaterialRole.Hazard, parent, "Hazard");
        HazardRuntime voidHazard = voidVolume.AddComponent<HazardRuntime>();
        voidHazard.kind = CampaignHazardKind.Lava;
        voidHazard.damage = 999;
        voidHazard.repeatDelay = 0.1f;
    }

    private static void BuildHazards(CampaignLevelConfig config, Transform parent)
    {
        foreach (CampaignHazardPlacement placement in config.hazards)
        {
            GameObject hazard = CreateTriggerCube(placement.kind.ToString(), placement.position, placement.size,
                placement.kind == CampaignHazardKind.Lava ? MaterialRole.Hazard : MaterialRole.Enemy, parent, "Hazard");
            HazardRuntime runtime = hazard.AddComponent<HazardRuntime>();
            runtime.kind = placement.kind;
            runtime.damage = placement.kind == CampaignHazardKind.Lava ? 40 : 30;
        }
    }

    private static void BuildPickups(CampaignLevelConfig config, Transform parent)
    {
        foreach (CampaignPickupLinePlacement line in config.pickupLines)
        {
            for (int i = 0; i < line.count; i++)
            {
                CreatePickup(line.start + line.offset * i, line.kind, parent);
            }
        }

        foreach (CampaignPickupArcPlacement arc in config.pickupArcs)
        {
            for (int i = 0; i < arc.count; i++)
            {
                float t = arc.count <= 1 ? 0.5f : i / (float)(arc.count - 1);
                Vector3 position = arc.center + new Vector3((t - 0.5f) * arc.width, Mathf.Sin(t * Mathf.PI) * 1.15f, 0f);
                CreatePickup(position, arc.kind, parent);
            }
        }

        foreach (CampaignPickupPlacement pickup in config.pickups)
        {
            CreatePickup(pickup.position, pickup.kind, parent);
        }

        foreach (CampaignBonusCachePlacement cache in config.bonusCaches)
        {
            GameObject cacheObject = CreateCube("BonusCache_" + cache.theme, cache.position, new Vector3(1.3f, 1.1f, 1.1f),
                MaterialRole.Goal, parent, "Interactable");
            BonusCacheRuntime runtime = cacheObject.AddComponent<BonusCacheRuntime>();
        }
    }

    private static void BuildCheckpoints(CampaignLevelConfig config, Transform parent)
    {
        foreach (CampaignCheckpointPlacement checkpoint in config.checkpoints)
        {
            GameObject checkpointObject = CreateTriggerCube("Checkpoint_" + checkpoint.index, checkpoint.position,
                new Vector3(1.5f, 2f, 3.5f), MaterialRole.Player, parent, "Checkpoint");
            CheckpointRuntime runtime = checkpointObject.AddComponent<CheckpointRuntime>();
            runtime.index = checkpoint.index;
            runtime.respawnPosition = checkpoint.position;
        }
    }

    private static void BuildEnemies(CampaignLevelConfig config, Transform parent)
    {
        foreach (CampaignEnemyPlacement placement in config.enemies)
        {
            GameObject enemyObject = new GameObject(placement.kind + "Enemy");
            enemyObject.transform.SetParent(parent, true);
            enemyObject.transform.position = placement.position;
            enemyObject.tag = "Enemy";
            SetLayer(enemyObject, "Enemy");

            CharacterController controller = enemyObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.45f;
            controller.center = Vector3.up * 0.9f;
            HealthComponent health = enemyObject.AddComponent<HealthComponent>();
            EnemyController enemy = enemyObject.AddComponent<EnemyController>();
            enemy.kind = placement.kind;
            enemy.patrolDistance = Mathf.Max(0.5f, placement.patrolDistance);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "EnemyVisual";
            visual.transform.SetParent(enemyObject.transform, false);
            visual.transform.localPosition = Vector3.up * 0.9f;
            visual.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            ApplyMaterial(visual, MaterialRole.Enemy);
        }
    }

    private static void BuildGoal(CampaignLevelConfig config, Transform parent)
    {
        GameObject goal = CreateTriggerCube("Goal", config.goalPosition, new Vector3(2.5f, 3f, 5f), MaterialRole.Goal, parent, "Goal");
        GoalRuntime runtime = goal.AddComponent<GoalRuntime>();
        runtime.levelId = config.levelId;
    }

    private static void CreatePickup(Vector3 position, CampaignPickupKind kind, Transform parent)
    {
        PrimitiveType primitive = kind == CampaignPickupKind.Key ? PrimitiveType.Cube : PrimitiveType.Sphere;
        GameObject pickup = GameObject.CreatePrimitive(primitive);
        pickup.name = kind + "Pickup";
        pickup.transform.SetParent(parent, true);
        pickup.transform.position = position;
        pickup.transform.localScale = kind == CampaignPickupKind.Key ? Vector3.one * 0.55f : Vector3.one * 0.42f;
        SetLayer(pickup, "Pickup");
        pickup.GetComponent<Collider>().isTrigger = true;
        ApplyMaterial(pickup, MaterialRole.Pickup);
        PickupRuntime runtime = pickup.AddComponent<PickupRuntime>();
        runtime.kind = kind;
    }

    // ------------------------------------------------------------ utilidades

    private enum MaterialRole
    {
        Terrain,
        Player,
        Enemy,
        Pickup,
        Hazard,
        Goal
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 size, MaterialRole role,
        Transform parent, string layerName)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = size;
        SetLayer(cube, layerName);
        ApplyMaterial(cube, role);
        return cube;
    }

    private static GameObject CreateTriggerCube(string name, Vector3 position, Vector3 size, MaterialRole role,
        Transform parent, string layerName)
    {
        GameObject cube = CreateCube(name, position, size, role, parent, layerName);
        cube.GetComponent<Collider>().isTrigger = true;
        return cube;
    }

    private static void ApplyMaterial(GameObject target, MaterialRole role)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        string materialName = role switch
        {
            MaterialRole.Player => "WTH_Player",
            MaterialRole.Enemy => "WTH_Enemy",
            MaterialRole.Pickup => "WTH_Pickup",
            MaterialRole.Hazard => "WTH_Hazard",
            MaterialRole.Goal => "WTH_Goal",
            _ => "WTH_Default"
        };

        Material material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsRoot}/{materialName}.mat");
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void SetLayer(GameObject target, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
        {
            target.layer = layer;
        }
    }
}
