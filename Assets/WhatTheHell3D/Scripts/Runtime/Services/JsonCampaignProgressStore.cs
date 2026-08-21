using System.IO;
using UnityEngine;

public interface ICampaignProgressStore
{
    CampaignProgressData Load();
    void Save(CampaignProgressData progress);
    void Delete();
}

public sealed class JsonCampaignProgressStore : ICampaignProgressStore
{
    private readonly string filePath;

    public JsonCampaignProgressStore(string fileName = "save_game.json")
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public CampaignProgressData Load()
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            CampaignProgressData progress = JsonUtility.FromJson<CampaignProgressData>(File.ReadAllText(filePath));
            return progress != null && progress.version <= CampaignProgressData.CurrentVersion ? progress : null;
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"No se pudo leer el guardado de campaña: {exception.Message}");
            return null;
        }
    }

    public void Save(CampaignProgressData progress)
    {
        if (progress == null)
        {
            return;
        }

        progress.version = CampaignProgressData.CurrentVersion;
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(filePath, json);
    }

    public void Delete()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
