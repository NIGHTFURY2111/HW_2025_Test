using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "RawInputManager", menuName = "Input/Raw Input Manager")]
public class RawInputManager : ScriptableObject,InputSystem_Actions.IPlayerActions
{

    private InputSystem_Actions input;
    private InputValueStorage MoveStorage;

    #region Input System Callback initialization
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
    #endregion

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveStorage ??= new InputValueStorage();
        MoveStorage.UpdateValue(context.ReadValue<Vector2>());
    }

    public Vector2 Move()
    {
        return MoveStorage?.GetValue ?? Vector2.zero;
    }
}
