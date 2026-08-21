using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CampaignLevelRuntime : MonoBehaviour
{
    private CampaignLevelConfig config;
    private InputActionAsset inputActions;
    private Transform levelRoot;
    private PlayerController player;
    private CameraController cameraController;

    public Transform PlayerTransform => player == null ? null : player.transform;

    public void Configure(CampaignLevelConfig level, InputActionAsset actions)
    {
        config = level;
        inputActions = actions;
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("CampaignLevelRuntime necesita una CampaignLevelConfig.");
            return;
        }

        CampaignRuntimeState state = CampaignRuntimeState.Ensure(null);
        state.BeginLevel(config);
        levelRoot = new GameObject("CampaignLevel_" + config.levelId).transform;

        ConfigureEnvironment();
        BuildPlayer();
        BuildCamera();
        BuildGeometry();
        BuildHazards();
        BuildPickups();
        BuildCheckpoints();
        BuildEnemies();
        BuildGoal();
        BuildPresentation();
    }

    private void ConfigureEnvironment()
    {
        Camera.main?.GetComponent<Camera>();
        RenderSettings.fog = config.fogDensity > 0f;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = config.fogDensity;
        RenderSettings.fogColor = config.backgroundColor;
        RenderSettings.ambientLight = config.ambientLightColor;
        RenderSettings.skybox = null;

        GameObject keyObject = new GameObject("KeyLight");
        Light keyLight = keyObject.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = config.ambientLightColor;
        keyLight.intensity = config.keyLightEnergy;
        keyObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

        GameObject fillObject = new GameObject("FillLight");
        Light fillLight = fillObject.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = config.fillLightColor;
        fillLight.intensity = config.fillLightEnergy;
        fillObject.transform.rotation = Quaternion.Euler(25f, 145f, 0f);
    }

    private void BuildPlayer()
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
        player = playerObject.AddComponent<PlayerController>();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "PlayerVisual";
        visual.transform.SetParent(playerObject.transform, false);
        visual.transform.localPosition = Vector3.up * 0.9f;
        visual.transform.localScale = new Vector3(0.84f, 0.9f, 0.84f);
        Destroy(visual.GetComponent<Collider>());
        SetMaterial(visual, new Color(0.12f, 0.62f, 0.95f));
    }

    private void BuildCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 300f;
        cameraObject.AddComponent<AudioListener>();
        cameraController = cameraObject.AddComponent<CameraController>();
        cameraController.Configure(player.transform, inputActions, config);
        player.Configure(inputActions, camera.transform, config.playerStart);
    }

    private void BuildGeometry()
    {
        foreach (CampaignPlatformPlacement placement in config.platforms)
        {
            CreateCube("Platform", placement.position, placement.size, ColorForTheme(0.36f), "Ground");
        }

        foreach (CampaignPlatformPlacement placement in config.bridges)
        {
            CreateCube("Bridge", placement.position, placement.size, ColorForTheme(0.48f), "Ground");
        }

        foreach (CampaignForestIslandPlacement placement in config.forestIslandBases)
        {
            Color color = placement.variant % 2 == 0 ? new Color(0.18f, 0.32f, 0.17f) : new Color(0.25f, 0.4f, 0.2f);
            CreateCube("ForestIsland", placement.position, placement.size, color, "Ground");
        }

        foreach (CampaignMovingPlatformPlacement placement in config.movingPlatforms)
        {
            GameObject platform = CreateCube("MovingPlatform", placement.position, placement.size, ColorForTheme(0.58f), "Ground");
            platform.AddComponent<MovingPlatformRuntime>().Configure(placement.travel, placement.duration);
        }

        foreach (CampaignPlatformPlacement placement in config.fallingPlatforms)
        {
            GameObject platform = CreateCube("FallingPlatform", placement.position, placement.size, ColorForTheme(0.62f), "Ground");
            platform.AddComponent<FallingPlatformRuntime>().Configure();
            GameObject trigger = CreateTriggerCube("FallingPlatformTrigger", placement.position + Vector3.up * 0.6f, new Vector3(placement.size.x * 0.9f, 1.4f, placement.size.z * 0.9f), new Color(1f, 1f, 1f, 0.01f), "Ground");
            trigger.transform.SetParent(platform.transform, true);
        }

        foreach (CampaignStairPlacement stair in config.stairs)
        {
            for (int i = 0; i < stair.count; i++)
            {
                Vector3 position = stair.start + new Vector3(i * 1.5f, i * stair.step, 0f);
                CreateCube("Stair", position, new Vector3(1.6f, 0.6f + i * 0.05f, 3.4f), ColorForTheme(0.42f), "Ground");
            }
        }

        foreach (CampaignVolumePlacement lava in config.lava)
        {
            GameObject lavaObject = CreateTriggerCube("LavaVolume", lava.position, lava.size, new Color(0.9f, 0.16f, 0.03f), "Hazard");
            lavaObject.AddComponent<HazardRuntime>().Configure(CampaignHazardKind.Lava);
        }

        float minX = Mathf.Min(config.playerStart.x, config.goalPosition.x) - 30f;
        float maxX = Mathf.Max(config.playerStart.x, config.goalPosition.x) + 30f;
        GameObject voidVolume = CreateTriggerCube("OutOfBounds", new Vector3((minX + maxX) * 0.5f, config.playerStart.y - 12f, 0f), new Vector3(maxX - minX, 1f, 80f), new Color(0.8f, 0.05f, 0.02f), "Hazard");
        HazardRuntime voidHazard = voidVolume.AddComponent<HazardRuntime>();
        voidHazard.damage = 999;
        voidHazard.repeatDelay = 0.1f;
    }

    private void BuildHazards()
    {
        foreach (CampaignHazardPlacement placement in config.hazards)
        {
            Color color = placement.kind == CampaignHazardKind.Lava ? new Color(0.9f, 0.12f, 0.02f) : new Color(0.72f, 0.72f, 0.76f);
            GameObject hazard = CreateTriggerCube(placement.kind.ToString(), placement.position, placement.size, color, "Hazard");
            hazard.AddComponent<HazardRuntime>().Configure(placement.kind);
        }
    }

    private void BuildPickups()
    {
        foreach (CampaignPickupLinePlacement line in config.pickupLines)
        {
            for (int i = 0; i < line.count; i++)
            {
                CreatePickup(line.start + line.offset * i, line.kind);
            }
        }

        foreach (CampaignPickupArcPlacement arc in config.pickupArcs)
        {
            for (int i = 0; i < arc.count; i++)
            {
                float t = arc.count <= 1 ? 0.5f : i / (float)(arc.count - 1);
                Vector3 position = arc.center + new Vector3((t - 0.5f) * arc.width, Mathf.Sin(t * Mathf.PI) * 1.15f, 0f);
                CreatePickup(position, arc.kind);
            }
        }

        foreach (CampaignPickupPlacement pickup in config.pickups)
        {
            CreatePickup(pickup.position, pickup.kind);
        }

        foreach (CampaignBonusCachePlacement cache in config.bonusCaches)
        {
            GameObject cacheObject = CreateCube("BonusCache_" + cache.theme, cache.position, new Vector3(1.3f, 1.1f, 1.1f), new Color(0.55f, 0.3f, 0.1f), "Interactable");
            cacheObject.AddComponent<BonusCacheRuntime>().Configure();
        }
    }

    private void BuildCheckpoints()
    {
        foreach (CampaignCheckpointPlacement checkpoint in config.checkpoints)
        {
            GameObject checkpointObject = CreateTriggerCube("Checkpoint_" + checkpoint.index, checkpoint.position, new Vector3(1.5f, 2f, 3.5f), new Color(0.2f, 0.55f, 1f), "Checkpoint");
            checkpointObject.AddComponent<CheckpointRuntime>().Configure(checkpoint.index, checkpoint.position);
        }
    }

    private void BuildEnemies()
    {
        foreach (CampaignEnemyPlacement placement in config.enemies)
        {
            GameObject enemyObject = new GameObject(placement.kind + "Enemy");
            enemyObject.transform.SetParent(levelRoot, true);
            enemyObject.transform.position = placement.position;
            enemyObject.tag = "Enemy";
            SetLayer(enemyObject, "Enemy");

            CharacterController controller = enemyObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.45f;
            controller.center = Vector3.up * 0.9f;
            HealthComponent health = enemyObject.AddComponent<HealthComponent>();
            EnemyController enemy = enemyObject.AddComponent<EnemyController>();
            enemy.Configure(placement.kind, placement.patrolDistance, player.transform);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "EnemyVisual";
            visual.transform.SetParent(enemyObject.transform, false);
            visual.transform.localPosition = Vector3.up * 0.9f;
            visual.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            Destroy(visual.GetComponent<Collider>());
            SetMaterial(visual, EnemyColor(placement.kind));
        }
    }

    private void BuildGoal()
    {
        GameObject goal = CreateTriggerCube("Goal", config.goalPosition, new Vector3(2.5f, 3f, 5f), new Color(0.95f, 0.8f, 0.2f), "Goal");
        goal.AddComponent<GoalRuntime>().Configure(config.levelId);
    }

    private void BuildPresentation()
    {
        CampaignHudController hud = gameObject.AddComponent<CampaignHudController>();
        hud.Configure(config, player);
        PauseController pause = gameObject.AddComponent<PauseController>();
        pause.Configure(config, player);
        gameObject.AddComponent<CampaignAudioDirector>().Configure(config);
    }

    private GameObject CreatePickup(Vector3 position, CampaignPickupKind kind)
    {
        PrimitiveType primitive = kind == CampaignPickupKind.Key ? PrimitiveType.Cube : PrimitiveType.Sphere;
        GameObject pickup = GameObject.CreatePrimitive(primitive);
        pickup.name = kind + "Pickup";
        pickup.transform.SetParent(levelRoot, true);
        pickup.transform.position = position;
        pickup.transform.localScale = kind == CampaignPickupKind.Key ? Vector3.one * 0.55f : Vector3.one * 0.42f;
        SetLayer(pickup, "Pickup");
        Collider collider = pickup.GetComponent<Collider>();
        collider.isTrigger = true;
        SetMaterial(pickup, kind == CampaignPickupKind.Key ? new Color(1f, 0.78f, 0.1f) : kind == CampaignPickupKind.Heart ? new Color(0.95f, 0.15f, 0.25f) : new Color(1f, 0.85f, 0.15f));
        pickup.AddComponent<PickupRuntime>().Configure(kind);
        return pickup;
    }

    private GameObject CreateCube(string name, Vector3 position, Vector3 size, Color color, string layerName)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(levelRoot, true);
        cube.transform.position = position;
        cube.transform.localScale = size;
        SetLayer(cube, layerName);
        SetMaterial(cube, color);
        return cube;
    }

    private GameObject CreateTriggerCube(string name, Vector3 position, Vector3 size, Color color, string layerName)
    {
        GameObject cube = CreateCube(name, position, size, color, layerName);
        cube.GetComponent<Collider>().isTrigger = true;
        return cube;
    }

    private Color ColorForTheme(float brightness)
    {
        if (config.artTheme == "mines")
        {
            return new Color(brightness * 0.8f, brightness * 0.65f, brightness * 0.48f);
        }

        if (config.artTheme == "castle")
        {
            return new Color(brightness * 0.56f, brightness * 0.6f, brightness * 0.7f);
        }

        return new Color(brightness * 0.48f, brightness * 0.78f, brightness * 0.4f);
    }

    private static Color EnemyColor(CampaignEnemyKind kind)
    {
        switch (kind)
        {
            case CampaignEnemyKind.Zombie:
                return new Color(0.5f, 0.65f, 0.36f);
            case CampaignEnemyKind.Witch:
                return new Color(0.52f, 0.2f, 0.72f);
            default:
                return new Color(0.85f, 0.25f, 0.12f);
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

    private static void SetMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader != null)
        {
            Material material = new Material(shader)
            {
                color = color
            };
            renderer.sharedMaterial = material;
        }
    }
}
