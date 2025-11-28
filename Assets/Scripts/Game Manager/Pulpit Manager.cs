using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages pulpit spawning, lifecycle, and player interaction tracking.
/// </summary>
public class PulpitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pulpitPrefab;
    [SerializeField] private Transform pulpitParent;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private float pulpitSize = 9f;
    
    private const int MAX_PULPITS = 2;
    
    private PulpitData pulpitData;
    private List<Pulpit> activePulpits = new List<Pulpit>();
    private Vector3 lastSpawnPosition;
    private bool isGameActive;
    
    public static event Action<int> OnPulpitVisited;
    
    private void OnEnable()
    {
        GameManager.OnGameStart += EnableSpawning;
        GameManager.OnGameOver += DisableSpawning;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameStart -= EnableSpawning;
        GameManager.OnGameOver -= DisableSpawning;
    }
    
    private void Update()
    {
        if (!isGameActive || pulpitData == null) return;
        
        UpdatePulpitTimers();
        CheckSpawnNewPulpit();
    }

    private void OnDestroy()
    {
        CleanupPulpits();
    }
    
    public void Initialize(PulpitData data)
    {
        if (data == null || pulpitPrefab == null)
        {
            Debug.LogError("reference is null!");
            return;
        }
        
        pulpitData = data;
        lastSpawnPosition = startPosition;
        SpawnPulpitAt(startPosition);
    }
    
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
    
    private void HandlePulpitDestroyed(Pulpit pulpit)
    {
        if (pulpit != null)
        {
            UnsubscribeFromPulpit(pulpit);
        }
        activePulpits.Remove(pulpit);
    }
    
    private void HandlePlayerEntered(Pulpit pulpit)
    {
        OnPulpitVisited?.Invoke(1);
    }
    
    private void UnsubscribeFromPulpit(Pulpit pulpit)
    {
        if (pulpit == null) return;
        
        pulpit.OnPulpitDestroyed -= HandlePulpitDestroyed;
        pulpit.OnPlayerEntered -= HandlePlayerEntered;
    }
    
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
    
    private void EnableSpawning() => isGameActive = true;
    
    private void DisableSpawning() => isGameActive = false;
}
