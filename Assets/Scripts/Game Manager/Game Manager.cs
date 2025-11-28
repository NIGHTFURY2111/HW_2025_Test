using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("References")]
    [SerializeField] private PlayerMovementhandler playerMovement;
    [SerializeField] private PulpitManager pulpitManager;
    [SerializeField] private RawInputManager inputManager;
    [SerializeField] private ConfigLoader configLoader;
    
    #endregion
    
    #region Private Fields
    
    private int currentScore;
    private bool isGameActive;
    private bool isInitialized;
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Event triggered when the game ends.
    /// </summary>
    public static event Action OnGameOver;
    
    /// <summary>
    /// Event triggered when the game starts.
    /// </summary>
    public static event Action OnGameStart;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Initializes the game by loading configuration and setting up all systems.
    /// </summary>
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

    /// <summary>
    /// Cleans up event subscriptions when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    #endregion
    
    #region Event Management
    
    /// <summary>
    /// Subscribes to all relevant game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        PulpitManager.OnPulpitVisited += HandlePulpitVisited;
        PlayerMovementhandler.OnPlayerDeath += HandlePlayerDeath;
        GameUIManager.OnStartGame += StartGame;
        GameUIManager.OnRestartGame += RestartGame;
    }
    
    /// <summary>
    /// Unsubscribes from all game events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        PulpitManager.OnPulpitVisited -= HandlePulpitVisited;
        PlayerMovementhandler.OnPlayerDeath -= HandlePlayerDeath;
        GameUIManager.OnStartGame -= StartGame;
        GameUIManager.OnRestartGame -= RestartGame;
    }
    
    #endregion
    
    #region Event Handlers
    
    /// <summary>
    /// Handles pulpit visit events and updates score.
    /// </summary>
    /// <param name="points">Points to add to the current score.</param>
    private void HandlePulpitVisited(int points)
    {
        if (!isGameActive) return;
        currentScore += points;
    }
    
    /// <summary>
    /// Handles player death event.
    /// </summary>
    private void HandlePlayerDeath()
    {
        EndGame();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Starts a new game session.
    /// </summary>
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
    
    /// <summary>
    /// Ends the current game session.
    /// </summary>
    public void EndGame()
    {
        if (!isGameActive) return;
        
        isGameActive = false;
        OnGameOver?.Invoke();
    }
    
    /// <summary>
    /// Restarts the game by cleaning up and starting fresh.
    /// </summary>
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
    
    /// <summary>
    /// Gets the current game score.
    /// </summary>
    /// <returns>The current score value.</returns>
    public int GetCurrentScore() => currentScore;
    
    /// <summary>
    /// Checks if the game is currently active.
    /// </summary>
    /// <returns>True if game is active, false otherwise.</returns>
    public bool IsGameActive() => isGameActive;
    
    #endregion
}
