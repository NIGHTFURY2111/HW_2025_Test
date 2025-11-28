using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject-based input manager that interfaces with Unity's Input System.
/// Provides centralized access to player input actions.
/// </summary>
[CreateAssetMenu(fileName = "RawInputManager", menuName = "Input/Raw Input Manager")]
public class RawInputManager : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    #region Private Fields
    
    private InputSystem_Actions input;
    private InputValueStorage moveStorage;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Initializes and enables the input system.
    /// </summary>
    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputSystem_Actions();
            input.Player.SetCallbacks(this);
        }
        input.Player.Enable();
    }

    /// <summary>
    /// Disables and cleans up the input system.
    /// </summary>
    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Disable();
            input = null;
        }
        
        moveStorage = null;
    }
    
    #endregion
    
    #region Input Callbacks
    
    /// <summary>
    /// Handles move input events from the Input System.
    /// </summary>
    /// <param name="context">Input action callback context.</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        if (moveStorage == null)
        {
            moveStorage = new InputValueStorage();
        }
        moveStorage.UpdateValue(context.ReadValue<Vector2>());
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Gets the current movement input value.
    /// </summary>
    /// <returns>Movement input as Vector2.</returns>
    public Vector2 Move() => moveStorage?.Value ?? Vector2.zero;
    
    #endregion
}
