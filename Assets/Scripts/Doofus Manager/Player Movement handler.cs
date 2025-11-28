using UnityEngine;
using System;

/// <summary>
/// Handles player movement with force-based physics, gravity, levitation, and rotation stabilization.
/// Manages player state, death detection, and provides smooth physics-based movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementhandler : MonoBehaviour
{
    #region Serialized Fields
    
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
    
    #endregion
    
    #region Private Fields
    
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
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Event triggered when the player dies.
    /// </summary>
    public static event Action OnPlayerDeath;
    
    #endregion
    
    #region Unity Lifecycle

    /// <summary>
    /// Initializes the rigidbody reference and stores the starting position.
    /// </summary>
    private void Awake()
    {
        if (!TryGetComponent(out rb))
        {
            Debug.LogError("PlayerMovementhandler: Rigidbody component not found!");
        }
        startPosition = transform.position;
    }
    
    /// <summary>
    /// Subscribes to game start event.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnGameStart += EnablePlayer;
    }
    
    /// <summary>
    /// Unsubscribes from game start event.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnGameStart -= EnablePlayer;
    }
    
    /// <summary>
    /// Handles all physics-based updates for movement, levitation, rotation, and death checking.
    /// </summary>
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
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initializes the player with data and input manager reference.
    /// </summary>
    /// <param name="data">Player configuration data containing speed settings.</param>
    /// <param name="managerRef">Reference to the input manager for reading player input.</param>
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
    
    /// <summary>
    /// Resets the player to starting position and clears all states.
    /// </summary>
    public void ResetPlayer()
    {
        isDead = false;
        isGameActive = false;
        transform.position = startPosition;
        StopMovement();
    }
    
    #endregion
    
    #region Ground Detection

    /// <summary>
    /// Performs a sphere cast downward to detect ground beneath the player.
    /// </summary>
    private void PerformGroundCheck()
    {
        var ray = new Ray(transform.position, -transform.up);
        isGrounded = Physics.SphereCast(ray, sphereCastRadius, out groundHit, groundCheckRange, groundLayerMask, QueryTriggerInteraction.Ignore);
    }
    
    #endregion
    
    #region Physics Forces

    /// <summary>
    /// Applies custom gravity force to the rigidbody based on gravity multiplier.
    /// </summary>
    private void ApplyGravity()
    {
        var gravityForceMagnitude = gravityMultiplier * rb.mass * Physics.gravity.magnitude;
        gravityForce = gravityForceMagnitude * Physics.gravity.normalized;
        rb.AddForce(gravityForce, ForceMode.Force);
    }

    /// <summary>
    /// Applies levitation forces to keep the player hovering at a target ride height.
    /// Uses spring-damper physics to maintain smooth hovering above ground.
    /// </summary>
    private void ApplyLevitation()
    {
        if (!isGrounded) return;

        // Calculate vertical velocity relative to ground
        var relativeVerticalVelocity = -rb.linearVelocity.y;
        
        // Get actual distance from center to ground surface
        var radiusIndependentDistance = groundHit.distance - sphereCastRadius;
        
        // Calculate how far off we are from target ride height
        var rideHeightError = radiusIndependentDistance - rideHeight;

        // Spring-damper force calculation
        var restoringForceMagnitude = (rideHeightError * levitatorStiffness - relativeVerticalVelocity * levitatorDamper);
        var restoringForce = restoringForceMagnitude * Vector3.down;
        
        // Compensate for gravity to prevent double-application
        var gravityCorrectedRestoringForce = restoringForce - gravityForce;

        rb.AddForce(gravityCorrectedRestoringForce, ForceMode.Force);
    }

    /// <summary>
    /// Applies horizontal movement forces based on player input.
    /// Uses force-based physics with acceleration clamping and applies force at an offset to create lean effect.
    /// </summary>
    private void ApplyMovement()
    {
        if (rb == null || inputManager == null) return;
        
        var moveInput = inputManager.Move();
        var worldInput = new Vector3(moveInput.x, 0, moveInput.y);
        
        // Track last movement direction for rotation
        if (worldInput.sqrMagnitude > 0f)
        {
            lastNonZeroInputDirection = worldInput.normalized;
        }

        // Calculate current horizontal velocity
        var horizontalRigidbodyVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        
        // Calculate target velocity
        var goalVelocity = worldInput * speed * speedMultiplier;
        var horizontalGoalVelocity = Vector3.ProjectOnPlane(goalVelocity, Vector3.up);

        // Calculate required acceleration and clamp it
        var horizontalAcceleration = (horizontalGoalVelocity - horizontalRigidbodyVelocity) / Time.fixedDeltaTime;
        horizontalAcceleration = Vector3.ClampMagnitude(horizontalAcceleration, maximumAcceleration);

        // Apply force at an offset to create lean effect
        var force = rb.mass * horizontalAcceleration;
        var position = rb.position + Vector3.up * lean;
        rb.AddForceAtPosition(force, position, ForceMode.Force);
    }

    /// <summary>
    /// Applies torque to keep the player upright and facing the movement direction.
    /// Uses spring-damper physics for smooth rotation stabilization.
    /// </summary>
    private void ApplyErector()
    {
        var rotation = rb.rotation;
        var goalRotation = Quaternion.LookRotation(lastNonZeroInputDirection, Vector3.up);
        var rotationError = ShortestRotation(goalRotation, rotation);

        // Convert rotation error to torque using spring-damper
        rotationError.ToAngleAxis(out var degreesOfError, out var axisOfError);
        var radiansOfError = degreesOfError * Mathf.Deg2Rad;
        var torque = axisOfError * radiansOfError * erectorStiffness - rb.angularVelocity * erectorDamper;

        rb.AddTorque(torque, ForceMode.Force);
    }
    
    #endregion
    
    #region Rotation Utilities

    /// <summary>
    /// Calculates the shortest rotation between two quaternions, handling the double-cover property.
    /// </summary>
    /// <param name="a">Target rotation.</param>
    /// <param name="b">Current rotation.</param>
    /// <returns>The shortest rotation from b to a.</returns>
    private Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0)
        {
            return a * Quaternion.Inverse(Multiply(b, -1));
        }
        return a * Quaternion.Inverse(b);
    }

    /// <summary>
    /// Multiplies a quaternion by a scalar value.
    /// </summary>
    /// <param name="input">The quaternion to multiply.</param>
    /// <param name="scalar">The scalar multiplier.</param>
    /// <returns>The scaled quaternion.</returns>
    private Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }
    
    #endregion
    
    #region Death Management
    
    /// <summary>
    /// Checks if the player has fallen below the death threshold.
    /// </summary>
    private void CheckDeath()
    {
        if (transform.position.y < fallDeathThreshold)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Handles player death, triggering the death event and stopping all movement.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isGameActive = false;
        StopMovement();
        OnPlayerDeath?.Invoke();
    }
    
    #endregion
    
    #region Movement Control

    /// <summary>
    /// Stops all linear and angular velocity of the rigidbody.
    /// </summary>
    private void StopMovement()
    {
        if (rb == null) return;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    /// <summary>
    /// Checks if the player is allowed to move based on current state.
    /// </summary>
    /// <returns>True if the player can move, false otherwise.</returns>
    private bool CanMove() => isGameActive && !isDead && isInitialized && rb != null;
    
    /// <summary>
    /// Enables player movement when the game starts.
    /// </summary>
    private void EnablePlayer()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("PlayerMovementhandler: Cannot enable player - not initialized!");
            return;
        }
        isGameActive = true;
    }
    
    #endregion
}
