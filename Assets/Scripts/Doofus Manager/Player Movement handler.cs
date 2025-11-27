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
    
    private float speed;
    private RawInputManager inputManager;
    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isDead;
    private bool isGameActive;
    
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        TryGetComponent(out rb);
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
        if (!isGameActive || isDead)
        {
            StopMovement();
            return;
        }
        
        ApplyMovement();
        CheckDeath();
    }
    
    public void Initialize(PlayerData data, RawInputManager managerRef)
    {
        speed = data.speed;
        inputManager = managerRef;
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
        Vector3 horizontalVelocity = InputToGlobalPlane(inputManager.Move()) * speed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    private Vector3 InputToGlobalPlane(Vector2 inputVector)
    {
        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
    }
    
    private void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
    
    private void EnablePlayer()
    {
        isGameActive = true;
    }
}
