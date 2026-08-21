using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CampaignRuntimeState : MonoBehaviour
{
    public static CampaignRuntimeState Instance { get; private set; }

    public CampaignSceneCatalog Catalog { get; private set; }
    public CampaignProgressData Progress { get; private set; }

    private ICampaignProgressStore progressStore;

    public static CampaignRuntimeState Ensure(CampaignSceneCatalog catalog)
    {
        if (Instance == null)
        {
            GameObject root = new GameObject("CampaignRuntimeState");
            Instance = root.AddComponent<CampaignRuntimeState>();
            DontDestroyOnLoad(root);
        }

        if (catalog != null)
        {
            Instance.Configure(catalog);
        }

        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        progressStore = new JsonCampaignProgressStore();
    }

    public void Configure(CampaignSceneCatalog catalog)
    {
        Catalog = catalog;
        progressStore ??= new JsonCampaignProgressStore();
        Progress ??= CampaignProgressData.CreateNew(Catalog.GetCampaignLevelScene(1));
    }

    public void StartNewGame()
    {
        EnsureCatalog();
        Progress = CampaignProgressData.CreateNew(Catalog.GetCampaignLevelScene(1));
        progressStore.Delete();
        progressStore.Save(Progress);
        LoadScene(Catalog.introScene);
    }

    public void ContinueGame()
    {
        EnsureCatalog();
        CampaignProgressData loaded = progressStore.Load();
        if (loaded == null)
        {
            StartNewGame();
            return;
        }

        Progress = loaded;
        LoadScene(string.IsNullOrEmpty(Progress.checkpointScene)
            ? Catalog.GetCampaignLevelScene(Progress.currentLevelId)
            : Progress.checkpointScene);
    }

    public void SelectLevel(int levelId)
    {
        EnsureCatalog();
        string scene = Catalog.GetCampaignLevelScene(levelId);
        if (string.IsNullOrEmpty(scene))
        {
            return;
        }

        Progress = CampaignProgressData.CreateNew(scene);
        Progress.currentLevelId = levelId;
        Progress.currentLevelScene = scene;
        Progress.checkpointScene = scene;
        progressStore.Save(Progress);
        LoadScene(scene);
    }

    public void BeginLevel(CampaignLevelConfig config)
    {
        EnsureCatalog();
        string scene = Catalog.GetCampaignLevelScene(config.levelId);
        bool differentLevel = Progress == null || Progress.currentLevelScene != scene;
        if (differentLevel || Progress.levelFinished)
        {
            Progress = CampaignProgressData.CreateNew(scene);
            Progress.currentLevelId = config.levelId;
            Progress.currentLevelScene = scene;
            Progress.nextLevelScene = Catalog.GetNextScene(config.levelId);
        }
        else if (Progress.checkpointPosition == Vector3.zero)
        {
            Progress.checkpointPosition = config.playerStart;
        }

        Progress.totalCollectibles = CountConfiguredCollectibles(config);
        progressStore.Save(Progress);
    }

    public bool Collect(CampaignPickupKind kind)
    {
        if (Progress == null)
        {
            return false;
        }

        if (kind == CampaignPickupKind.Key)
        {
            Progress.keyCollected = true;
        }
        else if (kind == CampaignPickupKind.Coin)
        {
            Progress.collected++;
        }

        progressStore.Save(Progress);
        return true;
    }

    public bool SetCheckpoint(int index, Vector3 position)
    {
        if (Progress == null || index <= Progress.checkpointIndex)
        {
            return false;
        }

        Progress.checkpointIndex = index;
        Progress.checkpointPosition = position;
        Progress.checkpointScene = SceneManager.GetActiveScene().path;
        progressStore.Save(Progress);
        return true;
    }

    public bool CanFinishLevel()
    {
        return Progress != null && Progress.keyCollected;
    }

    public void FinishLevel(int levelId)
    {
        if (!CanFinishLevel())
        {
            return;
        }

        Progress.levelFinished = true;
        progressStore.Save(Progress);
        LoadScene(Catalog.GetNextScene(levelId));
    }

    public void RestartLevel()
    {
        if (Progress != null)
        {
            Vector3 savedCheckpoint = Progress.checkpointPosition;
            int savedCheckpointIndex = Progress.checkpointIndex;
            Progress.collected = 0;
            Progress.keyCollected = false;
            Progress.checkpointIndex = savedCheckpointIndex;
            Progress.checkpointPosition = savedCheckpoint;
            Progress.levelFinished = false;
            progressStore.Save(Progress);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }

    public Vector3 GetRespawnPosition(Vector3 fallback)
    {
        return Progress != null && Progress.checkpointPosition != Vector3.zero
            ? Progress.checkpointPosition
            : fallback;
    }

    private void EnsureCatalog()
    {
        if (Catalog == null)
        {
            Catalog = Resources.Load<CampaignSceneCatalog>("CampaignSceneCatalog");
        }

        progressStore ??= new JsonCampaignProgressStore();
        Progress ??= CampaignProgressData.CreateNew(Catalog != null
            ? Catalog.GetCampaignLevelScene(1)
            : string.Empty);
    }

    private static int CountConfiguredCollectibles(CampaignLevelConfig config)
    {
        int total = 0;
        foreach (CampaignPickupLinePlacement line in config.pickupLines)
        {
            total += line.count;
        }

        foreach (CampaignPickupArcPlacement arc in config.pickupArcs)
        {
            total += arc.count;
        }

        return total;
    }

    private static void LoadScene(string scenePath)
    {
        if (!string.IsNullOrEmpty(scenePath))
        {
            SceneManager.LoadScene(scenePath);
        }
    }
}
