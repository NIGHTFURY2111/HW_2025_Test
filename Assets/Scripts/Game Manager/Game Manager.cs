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
    private bool isInitialized;
    
    public static event Action OnGameOver;
    public static event Action OnGameStart;
    
    private void Start()
    {        
        if (configLoader.LoadConfig())
        {
            PlayerData playerData = configLoader.GetPlayerData();
            PulpitData pulpitData = configLoader.GetPulpitData();
            
            if (playerData == null || pulpitData == null)
            {
                Debug.LogError("GameManager: Config data is null. Cannot initialize.");
                return;
            }
            
            playerMovement.Initialize(playerData, inputManager);
            isInitialized = true;
            SubscribeToEvents();
        }
        else
        {
            Debug.LogError("Failed to load config. Game cannot start.");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
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
        if (!isInitialized)
        {
            Debug.LogError("GameManager: Cannot start game - not initialized!");
            return;
        }
        
        isGameActive = true;
        currentScore = 0;
        
        PulpitData pulpitData = configLoader.GetPulpitData();
        if (pulpitData != null)
        {
            pulpitManager.Initialize(pulpitData);
        }
        
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
        if (!isInitialized)
        {
            Debug.LogError("GameManager: Cannot restart game - not initialized!");
            return;
        }
        
        pulpitManager.CleanupPulpits();
        StartGame();
    }
    
    public int GetCurrentScore() => currentScore;
    
    public bool IsGameActive() => isGameActive;
}
