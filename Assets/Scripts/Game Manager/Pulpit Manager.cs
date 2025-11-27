using System.Collections.Generic;
using UnityEngine;
using System;

public class PulpitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pulpitPrefab;
    [SerializeField] private Transform pulpitParent;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private float pulpitSize = 9f;
    
    private PulpitData pulpitData;
    private List<Pulpit> activePulpits = new List<Pulpit>();
    private Vector3 lastSpawnPosition;
    private const int MAX_PULPITS = 2;
    
    public static event Action<int> OnPulpitVisited;
    
    public void Initialize(PulpitData data)
    {
        pulpitData = data;
        lastSpawnPosition = startPosition;
        Debug.Log($"[PulpitManager] Initialize - min: {data.min_pulpit_destroy_time}, max: {data.max_pulpit_destroy_time}, spawn: {data.pulpit_spawn_time}");
        SpawnInitialPulpit();
    }
    
    void Update()
    {
        if (pulpitData == null)
        {
            Debug.LogWarning("[PulpitManager] Update - pulpitData is NULL!");
            return;
        }
        
        UpdatePulpitTimers();
        CheckSpawnNewPulpit();
    }
    
    private void SpawnInitialPulpit()
    {
        Debug.Log($"[PulpitManager] SpawnInitialPulpit at position: {startPosition}");
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
        if (activePulpits.Count == 0) return;
        if (activePulpits.Count >= MAX_PULPITS) return;
        
        Pulpit oldestPulpit = activePulpits[0];
        if (oldestPulpit != null && oldestPulpit.ShouldSpawnNext(pulpitData.pulpit_spawn_time))
        {
            Vector3 newPosition = GetNextPulpitPosition();
            Debug.Log($"[PulpitManager] Spawning next pulpit at: {newPosition}");
            SpawnPulpitAt(newPosition);
        }
    }
    
    private void SpawnPulpitAt(Vector3 position)
    {
        if (pulpitPrefab == null)
        {
            Debug.LogError("[PulpitManager] SpawnPulpitAt - pulpitPrefab is NULL!");
            return;
        }
        
        GameObject pulpitObj = Instantiate(pulpitPrefab, position, Quaternion.identity, pulpitParent);
        Pulpit pulpit = pulpitObj.GetComponent<Pulpit>();
        
        if (pulpit != null)
        {
            float lifetime = pulpitData.GetRandomDestroyTime();
            Debug.Log($"[PulpitManager] SpawnPulpitAt - Pulpit created with lifetime: {lifetime}s");
            pulpit.Initialize(lifetime);
            pulpit.OnPulpitDestroyed += HandlePulpitDestroyed;
            pulpit.OnPlayerEntered += HandlePlayerEntered;
            
            activePulpits.Add(pulpit);
            lastSpawnPosition = position;
            Debug.Log($"[PulpitManager] Active pulpits count: {activePulpits.Count}");
        }
        else
        {
            Debug.LogError("[PulpitManager] SpawnPulpitAt - Pulpit component not found on prefab!");
        }
    }
    
    private Vector3 GetNextPulpitPosition()
    {
        Vector3[] possibleDirections = new Vector3[]
        {
            Vector3.forward * pulpitSize,
            Vector3.back * pulpitSize,
            Vector3.right * pulpitSize,
            Vector3.left * pulpitSize
        };
        
        List<Vector3> validPositions = new List<Vector3>();
        
        foreach (Vector3 direction in possibleDirections)
        {
            Vector3 newPos = lastSpawnPosition + direction;
            if (!IsPositionOccupied(newPos))
            {
                validPositions.Add(newPos);
            }
        }
        
        if (validPositions.Count == 0)
        {
            return lastSpawnPosition + possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Length)];
        }
        
        return validPositions[UnityEngine.Random.Range(0, validPositions.Count)];
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
        Debug.Log($"[PulpitManager] HandlePulpitDestroyed");
        pulpit.OnPulpitDestroyed -= HandlePulpitDestroyed;
        pulpit.OnPlayerEntered -= HandlePlayerEntered;
        
        if (activePulpits.Contains(pulpit))
        {
            activePulpits.Remove(pulpit);
        }
    }
    
    private void HandlePlayerEntered(Pulpit pulpit)
    {
        Debug.Log($"[PulpitManager] HandlePlayerEntered - Invoking score event");
        OnPulpitVisited?.Invoke(1);
    }
    
    private void OnDestroy()
    {
        foreach (Pulpit pulpit in activePulpits)
        {
            if (pulpit != null)
            {
                pulpit.OnPulpitDestroyed -= HandlePulpitDestroyed;
                pulpit.OnPlayerEntered -= HandlePlayerEntered;
            }
        }
    }
}
