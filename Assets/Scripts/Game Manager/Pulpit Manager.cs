using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pulpit spawning, lifecycle, and player interaction tracking.
/// </summary>
public class PulpitManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("References")]
    [SerializeField] private GameObject pulpitPrefab;
    [SerializeField] private Transform pulpitParent;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private float pulpitSize = 9f;
    
    #endregion
    
    #region Constants
    
    private const int MAX_PULPITS = 2;
    
    #endregion
    
    #region Private Fields
    
    private PulpitData pulpitData;
    private List<Pulpit> activePulpits = new List<Pulpit>();
    private Vector3 lastSpawnPosition;
    private bool isGameActive;
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Event triggered when a player visits a pulpit.
    /// </summary>
    public static event Action<int> OnPulpitVisited;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnGameStart += EnableSpawning;
        GameManager.OnGameOver += DisableSpawning;
    }
    
    /// <summary>
    /// Unsubscribes from game events.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnGameStart -= EnableSpawning;
        GameManager.OnGameOver -= DisableSpawning;
    }
    
    /// <summary>
    /// Updates active pulpit timers and checks for new spawns.
    /// </summary>
    private void Update()
    {
        if (!isGameActive || pulpitData == null) return;
        
        UpdatePulpitTimers();
        CheckSpawnNewPulpit();
    }

    /// <summary>
    /// Cleans up all pulpits when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        CleanupPulpits();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initializes the pulpit manager with configuration data.
    /// </summary>
    /// <param name="data">Pulpit configuration data.</param>
    public void Initialize(PulpitData data)
    {
        if (data == null || pulpitPrefab == null)
        {
            Debug.LogError("PulpitManager: Reference is null!");
            return;
        }
        
        pulpitData = data;
        lastSpawnPosition = startPosition;
        SpawnPulpitAt(startPosition);
    }
    
    /// <summary>
    /// Removes and destroys all active pulpits.
    /// </summary>
    public void CleanupPulpits()
    {
        for (int i = activePulpits.Count - 1; i >= 0; i--)
        {
            Pulpit pulpit = activePulpits[i];
            if (pulpit != null)
            {
                UnsubscribeFromPulpit(pulpit);
                Destroy(pulpit.gameObject);
            }
        }
        activePulpits.Clear();
    }
    
    #endregion
    
    #region Pulpit Management
    
    /// <summary>
    /// Updates timers for all active pulpits.
    /// </summary>
    private void UpdatePulpitTimers()
    {
        for (int i = activePulpits.Count - 1; i >= 0; i--)
        {
            if (activePulpits[i] != null)
            {
                activePulpits[i].UpdateTimer(Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Checks if a new pulpit should be spawned based on timing.
    /// </summary>
    private void CheckSpawnNewPulpit()
    {
        if (activePulpits.Count == 0 || activePulpits.Count >= MAX_PULPITS) 
            return;
        
        Pulpit oldestPulpit = activePulpits[0];
        if (oldestPulpit != null && oldestPulpit.ShouldSpawnNext(pulpitData.pulpit_spawn_time))
        {
            SpawnPulpitAt(GetNextPulpitPosition());
        }
    }
    
    /// <summary>
    /// Spawns a pulpit at the specified position.
    /// </summary>
    /// <param name="position">World position to spawn the pulpit.</param>
    private void SpawnPulpitAt(Vector3 position)
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("PulpitManager: Cannot spawn pulpit - prefab is null!");
            return;
        }
        
        GameObject pulpitObj = Instantiate(pulpitPrefab, position, Quaternion.identity, pulpitParent);
        
        if (pulpitObj == null)
        {
            Debug.LogError("PulpitManager: Failed to instantiate pulpit!");
            return;
        }
        
        Pulpit pulpit = pulpitObj.GetComponent<Pulpit>();
        
        if (pulpit == null)
        {
            Debug.LogError("PulpitManager: Instantiated prefab does not have Pulpit component!");
            Destroy(pulpitObj);
            return;
        }
        
        pulpit.Initialize(pulpitData.GetRandomDestroyTime());
        pulpit.OnPulpitDestroyed += HandlePulpitDestroyed;
        pulpit.OnPlayerEntered += HandlePlayerEntered;
        
        activePulpits.Add(pulpit);
        lastSpawnPosition = position;
    }
    
    #endregion
    
    #region Position Calculation
    
    /// <summary>
    /// Calculates a valid position for the next pulpit spawn.
    /// </summary>
    /// <returns>World position for the next pulpit.</returns>
    private Vector3 GetNextPulpitPosition()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        List<Vector3> validPositions = new List<Vector3>();
        
        foreach (Vector3 direction in directions)
        {
            Vector3 newPos = lastSpawnPosition + direction * pulpitSize;
            if (!IsPositionOccupied(newPos))
            {
                validPositions.Add(newPos);
            }
        }
        
        return validPositions.Count > 0 
            ? validPositions[UnityEngine.Random.Range(0, validPositions.Count)]
            : lastSpawnPosition + directions[UnityEngine.Random.Range(0, directions.Length)] * pulpitSize;
    }
    
    /// <summary>
    /// Checks if a position is already occupied by an active pulpit.
    /// </summary>
    /// <param name="position">Position to check.</param>
    /// <returns>True if occupied, false otherwise.</returns>
    private bool IsPositionOccupied(Vector3 position)
    {
        foreach (Pulpit pulpit in activePulpits)
        {
            if (pulpit != null && Vector3.Distance(pulpit.transform.position, position) < 1f)
            {
                return true;
            }
        }
        return false;
    }
    
    #endregion
    
    #region Event Handlers
    
    /// <summary>
    /// Handles pulpit destruction events.
    /// </summary>
    /// <param name="pulpit">The pulpit being destroyed.</param>
    private void HandlePulpitDestroyed(Pulpit pulpit)
    {
        if (pulpit != null)
        {
            UnsubscribeFromPulpit(pulpit);
        }
        activePulpits.Remove(pulpit);
    }
    
    /// <summary>
    /// Handles player entering a pulpit.
    /// </summary>
    /// <param name="pulpit">The pulpit the player entered.</param>
    private void HandlePlayerEntered(Pulpit pulpit)
    {
        OnPulpitVisited?.Invoke(1);
    }
    
    /// <summary>
    /// Unsubscribes from a pulpit's events.
    /// </summary>
    /// <param name="pulpit">Pulpit to unsubscribe from.</param>
    private void UnsubscribeFromPulpit(Pulpit pulpit)
    {
        if (pulpit == null) return;
        
        pulpit.OnPulpitDestroyed -= HandlePulpitDestroyed;
        pulpit.OnPlayerEntered -= HandlePlayerEntered;
    }
    
    #endregion
    
    #region State Management
    
    private void EnableSpawning() => isGameActive = true;
    
    private void DisableSpawning() => isGameActive = false;
    
    #endregion
}
