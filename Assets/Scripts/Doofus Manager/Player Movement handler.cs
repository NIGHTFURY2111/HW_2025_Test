using UnityEngine;

public class PlayerMovementhandler : MonoBehaviour
{
    private float speed;
    private RawInputManager inputManager;

    public void Initialize(PlayerData data)
    {
        speed = data.speed;
    }
}
