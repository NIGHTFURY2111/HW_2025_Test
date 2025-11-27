using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerMovementhandler PlayerMovement;
    [SerializeField] PulpitManager pulpitManager;
    [SerializeField] RawInputManager inputManager;
    [SerializeField] public ConfigLoader configLoader;
    
    private int currentScore = 0;
    
    void Start()
    {
        configLoader.LoadConfig();
        PlayerMovement.Initialize(configLoader.GetPlayerData(), inputManager);
        pulpitManager.Initialize(configLoader.GetPulpitData());
        
        SubscribeToEvents();
    }

    void Update()
    {
        //Debug.Log(inputManager.Move());
    }
    
    private void SubscribeToEvents()
    {
        PulpitManager.OnPulpitVisited += HandlePulpitVisited;
    }
    
    private void UnsubscribeFromEvents()
    {
        PulpitManager.OnPulpitVisited -= HandlePulpitVisited;
    }
    
    private void HandlePulpitVisited(int points)
    {
        currentScore += points;
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}
