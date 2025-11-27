using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerMovementhandler PlayerMovement;
    [SerializeField] RawInputManager inputManager;
    [SerializeField] public ConfigLoader configLoader;
    void Start()
    {
        configLoader.LoadConfig();
        PlayerMovement.Initialize(configLoader.GetPlayerData(), inputManager);
    }

    void Update()
    {
        //Debug.Log(inputManager.Move());
    }
}
