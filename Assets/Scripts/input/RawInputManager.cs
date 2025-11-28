using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject-based input manager that interfaces with Unity's Input System.
/// Provides centralized access to player input actions.
/// </summary>
[CreateAssetMenu(fileName = "RawInputManager", menuName = "Input/Raw Input Manager")]
public class RawInputManager : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions input;
    private InputValueStorage moveStorage;

    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputSystem_Actions();
            input.Player.SetCallbacks(this);
        }
        input.Player.Enable();
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Disable();
            input = null;
        }
        
        moveStorage = null;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (moveStorage == null)
        {
            moveStorage = new InputValueStorage();
        }
        moveStorage.UpdateValue(context.ReadValue<Vector2>());
    }

    public Vector2 Move() => moveStorage?.Value ?? Vector2.zero;
}
