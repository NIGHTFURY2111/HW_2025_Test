using System;
using UnityEngine;
using UnityEngine.Networking;

#region Data Classes

/// <summary>
/// Configuration data for player movement settings.
/// </summary>
[Serializable]
public class PlayerData
{
    public float speed;
}

/// <summary>
/// Configuration data for pulpit spawning and destruction.
/// </summary>
[Serializable]
public class PulpitData
{
    public float min_pulpit_destroy_time;
    public float max_pulpit_destroy_time;
    public float pulpit_spawn_time;

    /// <summary>
    /// Gets a random destroy time within the configured range.
    /// </summary>
    /// <returns>Random time value between min and max destroy time.</returns>
    public float GetRandomDestroyTime() => 
        UnityEngine.Random.Range(min_pulpit_destroy_time, max_pulpit_destroy_time);
}

/// <summary>
/// Root configuration data containing all game settings.
/// </summary>
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
    #region Serialized Fields
    
    [SerializeField] private string configURL = "https://s3.ap-south-1.amazonaws.com/superstars.assetbundles.testbuild/doofus_game/doofus_diary.json";
    [SerializeField] private TextAsset fallbackConfigFile;
    
    #endregion
    
    #region Private Fields
    
    private GameConfigData configData;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Loads configuration from remote URL or fallback file.
    /// </summary>
    /// <returns>True if loading succeeded, false otherwise.</returns>
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

    /// <summary>
    /// Gets the player configuration data.
    /// </summary>
    /// <returns>Player data or null if not loaded.</returns>
    public PlayerData GetPlayerData() => configData?.player_data;
    
    /// <summary>
    /// Gets the pulpit configuration data.
    /// </summary>
    /// <returns>Pulpit data or null if not loaded.</returns>
    public PulpitData GetPulpitData() => configData?.pulpit_data;
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Loads configuration from the local fallback file.
    /// </summary>
    /// <returns>True if loading succeeded, false otherwise.</returns>
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

    /// <summary>
    /// Parses JSON configuration text into data objects.
    /// </summary>
    /// <param name="jsonText">JSON string to parse.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
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
    
    #endregion
}
