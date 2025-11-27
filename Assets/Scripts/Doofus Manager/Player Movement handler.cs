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
        Vector3 horizontalVelocity = InputToGlobalPlane(inputManager.Move()) * speed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        Debug.Log(rb.linearVelocity.magnitude);
    }

    private Vector3 InputToGlobalPlane(Vector2 inputVector)
    {
        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
    }
}
