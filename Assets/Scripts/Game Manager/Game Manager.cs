using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerMovementhandler PlayerMovement;
    [SerializeField] PulpitManager pulpitManager;
    [SerializeField] RawInputManager inputManager;
    [SerializeField] public ConfigLoader configLoader;
    
    private int currentScore = 0;
    private bool isGameActive = false;
    
    public static event Action OnGameOver;
    public static event Action OnGameStart;
    
    void Start()
    {
        configLoader.LoadConfig();
        PlayerMovement.Initialize(configLoader.GetPlayerData(), inputManager);
        
        SubscribeToEvents();
    }

    void Update()
    {
        //Debug.Log(inputManager.Move());
    }
    
    private void SubscribeToEvents()
    {
        PulpitManager.OnPulpitVisited += HandlePulpitVisited;
        PlayerMovementhandler.OnPlayerDeath += HandlePlayerDeath;
        GameUIManager.OnStartGame += StartGame;
        GameUIManager.OnRestartGame += RestartGame;
    }
    
    private void UnsubscribeFromEvents()
    {
        PulpitManager.OnPulpitVisited -= HandlePulpitVisited;
        PlayerMovementhandler.OnPlayerDeath -= HandlePlayerDeath;
        GameUIManager.OnStartGame -= StartGame;
        GameUIManager.OnRestartGame -= RestartGame;
    }
    
    private void HandlePulpitVisited(int points)
    {
        currentScore += points;
    }
    
    private void HandlePlayerDeath()
    {
        EndGame();
    }
    
    public void StartGame()
    {
        isGameActive = true;
        currentScore = 0;
        
        pulpitManager.Initialize(configLoader.GetPulpitData());
        PlayerMovement.ResetPlayer();
        
        OnGameStart?.Invoke();
    }
    
    public void EndGame()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        OnGameOver?.Invoke();
    }
    
    public void RestartGame()
    {
        pulpitManager.CleanupPulpits();
        StartGame();
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    public bool IsGameActive()
    {
        return isGameActive;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}
