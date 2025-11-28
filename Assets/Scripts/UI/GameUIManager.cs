using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class GameUIManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    
    [Header("Game Over Text")]
    [SerializeField] private TMP_Text finalScoreText;
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Event triggered when the start button is pressed.
    /// </summary>
    public static event Action OnStartGame;
    
    /// <summary>
    /// Event triggered when the restart button is pressed.
    /// </summary>
    public static event Action OnRestartGame;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Sets up button listeners.
    /// </summary>
    private void Awake()
    {
        startButton?.onClick.AddListener(HandleStartGame);
        restartButton?.onClick.AddListener(HandleRestartGame);
    }
    
    /// <summary>
    /// Shows the start panel on game load.
    /// </summary>
    private void Start()
    {
        ShowPanel(startPanel);
    }
    
    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnGameStart += ShowHUD;
        GameManager.OnGameOver += ShowGameOver;
    }
    
    /// <summary>
    /// Unsubscribes from game events and cleans up animations.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnGameStart -= ShowHUD;
        GameManager.OnGameOver -= ShowGameOver;
        
        CleanupAnimations();
    }

    /// <summary>
    /// Removes button listeners and cleans up animations.
    /// </summary>
    private void OnDestroy()
    {
        startButton?.onClick.RemoveListener(HandleStartGame);
        restartButton?.onClick.RemoveListener(HandleRestartGame);
        
        CleanupAnimations();
    }
    
    #endregion
    
    #region Panel Management
    
    /// <summary>
    /// Shows specified panels and hides all others.
    /// </summary>
    /// <param name="panelsToShow">Panels to display.</param>
    private void ShowPanel(params GameObject[] panelsToShow)
    {
        SetPanelState(startPanel, false);
        SetPanelState(gameOverPanel, false);
        SetPanelState(hudPanel, false);
        
        foreach (GameObject panel in panelsToShow)
        {
            SetPanelState(panel, true);
            AnimatePanelEntry(panel);
        }
    }
    
    /// <summary>
    /// Shows the HUD panel.
    /// </summary>
    private void ShowHUD()
    {
        ShowPanel(hudPanel);
    }
    
    /// <summary>
    /// Shows the game over panel with final score.
    /// </summary>
    private void ShowGameOver()
    {
        if (finalScoreText != null && gameManager != null)
        {
            finalScoreText.text = $"Final Score: {gameManager.GetCurrentScore()}";
        }
        ShowPanel(gameOverPanel);
    }
    
    /// <summary>
    /// Sets a panel's active state.
    /// </summary>
    /// <param name="panel">Panel to modify.</param>
    /// <param name="active">Target active state.</param>
    private void SetPanelState(GameObject panel, bool active)
    {
        panel?.SetActive(active);
    }
    
    #endregion
    
    #region Button Handlers
    
    /// <summary>
    /// Handles start button press.
    /// </summary>
    private void HandleStartGame()
    {
        if (startButton != null)
        {
            AnimateButtonPress(startButton);
        }
        OnStartGame?.Invoke();
    }
    
    /// <summary>
    /// Handles restart button press.
    /// </summary>
    private void HandleRestartGame()
    {
        if (restartButton != null)
        {
            AnimateButtonPress(restartButton);
        }
        OnRestartGame?.Invoke();
    }
    
    #endregion
    
    #region Animation Methods
    
    /// <summary>
    /// Animates a panel's entry with scale animation.
    /// </summary>
    /// <param name="panel">Panel to animate.</param>
    private void AnimatePanelEntry(GameObject panel)
    {
        if (panel == null) return;
        
        panel.transform.DOKill();
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    
    /// <summary>
    /// Animates a button press with punch scale.
    /// </summary>
    /// <param name="button">Button to animate.</param>
    private void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        
        button.transform.DOKill();
        button.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
    }
    
    /// <summary>
    /// Cleans up all active UI animations.
    /// </summary>
    private void CleanupAnimations()
    {
        if (startPanel != null) startPanel.transform.DOKill();
        if (gameOverPanel != null) gameOverPanel.transform.DOKill();
        if (hudPanel != null) hudPanel.transform.DOKill();
        if (startButton != null) startButton.transform.DOKill();
        if (restartButton != null) restartButton.transform.DOKill();
    }
    
    #endregion
}
