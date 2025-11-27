using Unity.VisualScripting;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementhandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallDeathThreshold = -10f;
    
    private float speed;
    private RawInputManager inputManager;
    private Rigidbody rb;
    private Vector3 startPosition;
    private bool isDead = false;
    private bool isGameActive = false;
    
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
    
    public void Initialize(PlayerData data, RawInputManager managerRef)
    {
        speed = data.speed;
        inputManager = managerRef;
    }

    private void FixedUpdate()
    {
        if (!isGameActive || isDead)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }
        
        Vector3 horizontalVelocity = InputToGlobalPlane(inputManager.Move()) * speed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        
        CheckDeath();
    }

    private Vector3 InputToGlobalPlane(Vector2 inputVector)
    {
        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        OnPlayerDeath?.Invoke();
    }
    
    private void EnablePlayer()
    {
        isGameActive = true;
    }
    
    public void ResetPlayer()
    {
        isDead = false;
        isGameActive = false;
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
