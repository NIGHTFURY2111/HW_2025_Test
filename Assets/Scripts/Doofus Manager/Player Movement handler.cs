using UnityEngine;
using System;

/// <summary>
/// Handles player movement, death detection, and player state management.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementhandler : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float fallDeathThreshold = -10f;
    
    private RawInputManager inputManager;
    private Rigidbody rb;
    private Vector3 startPosition;
    private float speed;
    private bool isDead;
    private bool isGameActive;
    private bool isInitialized;
    
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        if (!TryGetComponent(out rb))
        {
            Debug.LogError("PlayerMovementhandler: Rigidbody component not found!");
        }
        startPosition = transform.position;
    }
    
    private void OnEnable()
    {
        GameManager.OnGameStart += EnablePlayer;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameStart -= EnablePlayer;
    }
    
    private void FixedUpdate()
    {
        if (!CanMove())
        {
            StopMovement();
            return;
        }
        
        ApplyMovement();
        CheckDeath();
    }
    
    public void Initialize(PlayerData data, RawInputManager managerRef)
    {
        if (data == null ||managerRef == null)
        {
            Debug.LogError("refernce is null!");
            return;
        }
        
        speed = data.speed;
        inputManager = managerRef;
        isInitialized = true;
    }
    
    public void ResetPlayer()
    {
        isDead = false;
        isGameActive = false;
        transform.position = startPosition;
        StopMovement();
    }

    private void ApplyMovement()
    {
        if (rb == null || inputManager == null) return;
        
        Vector3 horizontalVelocity = new Vector3(inputManager.Move().x, 0, inputManager.Move().y).normalized * speed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }
    
    private void CheckDeath()
    {
        if (transform.position.y < fallDeathThreshold)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isGameActive = false;
        StopMovement();
        OnPlayerDeath?.Invoke();
    }

    private void StopMovement()
    {
        if (rb == null) return;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    private bool CanMove() => isGameActive && !isDead && isInitialized && rb != null;
    
    private void EnablePlayer()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("PlayerMovementhandler: Cannot enable player - not initialized!");
            return;
        }
        isGameActive = true;
    }
}
