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
        input ??= new InputSystem_Actions();
        input.Player.SetCallbacks(this);
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input?.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveStorage ??= new InputValueStorage();
        moveStorage.UpdateValue(context.ReadValue<Vector2>());
    }

    public Vector2 Move() => moveStorage?.GetValue ?? Vector2.zero;
}
