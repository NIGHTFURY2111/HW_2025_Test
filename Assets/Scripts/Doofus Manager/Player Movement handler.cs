using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementhandler : MonoBehaviour
{
    private float speed;
    private RawInputManager inputManager;
    private Rigidbody rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }
    public void Initialize(PlayerData data, RawInputManager managerRef)
    {
        speed = data.speed;
        inputManager = managerRef;
    }

    private void FixedUpdate()
    {
        Debug.Log(InputToGlobalPlane(inputManager.Move()) * speed);
        rb.linearVelocity = InputToGlobalPlane(inputManager.Move()) * speed;
    }

    private Vector3 InputToGlobalPlane(Vector2 inputVector)
    {
        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
    }
}
