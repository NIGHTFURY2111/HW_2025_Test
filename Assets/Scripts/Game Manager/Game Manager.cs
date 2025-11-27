using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementhandler playerMovement;
    [SerializeField] private PulpitManager pulpitManager;
    [SerializeField] private RawInputManager inputManager;
    [SerializeField] private ConfigLoader configLoader;
    
    private int currentScore;
    private bool isGameActive;
    
    public static event Action OnGameOver;
    public static event Action OnGameStart;
    
    private void Start()
    {
        InitializeGame();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeGame()
    {
        configLoader.LoadConfig();
        playerMovement.Initialize(configLoader.GetPlayerData(), inputManager);
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
        if (!isGameActive) return;
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
        playerMovement.ResetPlayer();
        
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
    
    public int GetCurrentScore() => currentScore;
    
    public bool IsGameActive() => isGameActive;
}
