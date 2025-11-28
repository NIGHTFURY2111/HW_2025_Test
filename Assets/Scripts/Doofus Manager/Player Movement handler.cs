using UnityEngine;
using System;

/// <summary>
/// Handles player movement with force-based physics, gravity, levitation, and rotation stabilization.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementhandler : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float fallDeathThreshold = -10f;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField, Min(0f)] private float maximumAcceleration = 50f;
    [SerializeField, Min(0f)] private float lean = 0.2f;

    [Header("Gravity Settings")]
    [SerializeField, Min(0.01f)] private float gravityMultiplier = 1f;

    [Header("Levitator Settings")]
    [SerializeField, Min(0f)] private float rideHeight = 0.8f;
    [SerializeField, Min(0f)] private float levitatorStiffness = 50f;
    [SerializeField, Min(0f)] private float levitatorDamper = 10f;

    [Header("Erector Settings")]
    [SerializeField, Min(0f)] private float erectorStiffness = 50f;
    [SerializeField, Min(0f)] private float erectorDamper = 5f;

    [Header("Ground Check Settings")]
    [SerializeField, Min(0f)] private float groundCheckRange = 1f;
    [SerializeField] private LayerMask groundLayerMask = ~0;
    [SerializeField, Min(0f)] private float sphereCastRadius = 0.5f;
    
    private RawInputManager inputManager;
    private Rigidbody rb;
    private Vector3 startPosition;
    private float speed;
    private bool isDead;
    private bool isGameActive;
    private bool isInitialized;
    private Vector3 lastNonZeroInputDirection = Vector3.forward;
    private bool isGrounded;
    private RaycastHit groundHit;
    private Vector3 gravityForce;
    
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
        
        PerformGroundCheck();
        ApplyGravity();
        ApplyLevitation();
        ApplyMovement();
        ApplyErector();
        CheckDeath();
    }
    
    public void Initialize(PlayerData data, RawInputManager managerRef)
    {
        if (data == null || managerRef == null)
        {
            Debug.LogError("PlayerMovementhandler: Reference is null!");
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

    private void PerformGroundCheck()
    {
        var ray = new Ray(transform.position, -transform.up);
        isGrounded = Physics.SphereCast(ray, sphereCastRadius, out groundHit, groundCheckRange, groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    private void ApplyGravity()
    {
        var gravityForceMagnitude = gravityMultiplier * rb.mass * Physics.gravity.magnitude;
        gravityForce = gravityForceMagnitude * Physics.gravity.normalized;
        rb.AddForce(gravityForce, ForceMode.Force);
    }

    private void ApplyLevitation()
    {
        if (!isGrounded) return;

        var relativeVerticalVelocity = -rb.linearVelocity.y;
        var radiusIndependentDistance = groundHit.distance - sphereCastRadius;
        var rideHeightError = radiusIndependentDistance - rideHeight;

        var restoringForceMagnitude = (rideHeightError * levitatorStiffness - relativeVerticalVelocity * levitatorDamper);
        var restoringForce = restoringForceMagnitude * Vector3.down;
        var gravityCorrectedRestoringForce = restoringForce - gravityForce;

        rb.AddForce(gravityCorrectedRestoringForce, ForceMode.Force);
    }

    private void ApplyMovement()
    {
        if (rb == null || inputManager == null) return;
        
        var moveInput = inputManager.Move();
        var worldInput = new Vector3(moveInput.x, 0, moveInput.y);
        
        if (worldInput.sqrMagnitude > 0f)
        {
            lastNonZeroInputDirection = worldInput.normalized;
        }

        var horizontalRigidbodyVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        var goalVelocity = worldInput * speed * speedMultiplier;
        var horizontalGoalVelocity = Vector3.ProjectOnPlane(goalVelocity, Vector3.up);

        var horizontalAcceleration = (horizontalGoalVelocity - horizontalRigidbodyVelocity) / Time.fixedDeltaTime;
        horizontalAcceleration = Vector3.ClampMagnitude(horizontalAcceleration, maximumAcceleration);

        var force = rb.mass * horizontalAcceleration;
        var position = rb.position + Vector3.up * lean;
        rb.AddForceAtPosition(force, position, ForceMode.Force);
    }

    private void ApplyErector()
    {
        var rotation = rb.rotation;
        var goalRotation = Quaternion.LookRotation(lastNonZeroInputDirection, Vector3.up);
        var rotationError = ShortestRotation(goalRotation, rotation);

        rotationError.ToAngleAxis(out var degreesOfError, out var axisOfError);
        var radiansOfError = degreesOfError * Mathf.Deg2Rad;
        var torque = axisOfError * radiansOfError * erectorStiffness - rb.angularVelocity * erectorDamper;

        rb.AddTorque(torque, ForceMode.Force);
    }

    private Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(Multiply(b, -1));
        }
        return a * Quaternion.Inverse(b);
    }

    private Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
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
