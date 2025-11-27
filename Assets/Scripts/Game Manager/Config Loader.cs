using System;
using UnityEngine;

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
/// Loads game configuration from JSON file and provides access to game data.
/// </summary>
[CreateAssetMenu(fileName = "Config Loader", menuName = "Scriptable Objects/Config Loader")]
public class ConfigLoader : ScriptableObject
{
    [SerializeField] private TextAsset configFile;
    
    private GameConfigData configData;

    public void LoadConfig()
    {
        if (configFile == null)
        {
            Debug.LogError("Config file not assigned!");
            return;
        }

        try
        {
            configData = JsonUtility.FromJson<GameConfigData>(configFile.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load config: {e.Message}");
        }
    }

    public PlayerData GetPlayerData()
    {
        if (configData == null)
            LoadConfig();
        return configData?.player_data;
    }

    public PulpitData GetPulpitData()
    {
        if (configData == null)
            LoadConfig();
        return configData?.pulpit_data;
    }
}
