using System;
using UnityEngine;
using UnityEngine.Networking;

#region Data Classes

[Serializable]
public class PlayerData
{
    public float speed;
}

[Serializable]
public class PulpitData
{
    public float min_pulpit_destroy_time;
    public float max_pulpit_destroy_time;
    public float pulpit_spawn_time;

    public float GetRandomDestroyTime() => 
        UnityEngine.Random.Range(min_pulpit_destroy_time, max_pulpit_destroy_time);
}

[Serializable]
public class GameConfigData
{
    public PlayerData player_data = new PlayerData();
    public PulpitData pulpit_data = new PulpitData();
}

#endregion

/// <summary>
/// Loads game configuration from remote JSON URL with fallback to local file.
/// </summary>
[CreateAssetMenu(fileName = "Config Loader", menuName = "Scriptable Objects/Config Loader")]
public class ConfigLoader : ScriptableObject
{
    [SerializeField] private string configURL = "https://s3.ap-south-1.amazonaws.com/superstars.assetbundles.testbuild/doofus_game/doofus_diary.json";
    [SerializeField] private TextAsset fallbackConfigFile;
    
    private GameConfigData configData;

    public bool LoadConfig()
    {
        if (configData != null) return true;

        using (UnityWebRequest request = UnityWebRequest.Get(configURL))
        {
            request.SendWebRequest();
            
            while (!request.isDone) { }
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Config loaded from remote URL.");
                return ParseConfig(request.downloadHandler.text);
            }
            else
            {
                Debug.LogWarning($"Failed to load config from URL: {request.error}. Using fallback...");
                return LoadFallbackConfig();
            }
        }
    }

    private bool LoadFallbackConfig()
    {
        if (fallbackConfigFile != null)
        {
            Debug.Log("Config loaded from local fallback file.");
            return ParseConfig(fallbackConfigFile.text);
        }
        
        Debug.LogError("No fallback config file assigned! Config loading failed.");
        return false;
    }

    private bool ParseConfig(string jsonText)
    {
        try
        {
            configData = JsonUtility.FromJson<GameConfigData>(jsonText);
            return configData != null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse config JSON: {e.Message}");
            return false;
        }
    }

    public PlayerData GetPlayerData() => configData?.player_data;
    
    public PulpitData GetPulpitData() => configData?.pulpit_data;
}
