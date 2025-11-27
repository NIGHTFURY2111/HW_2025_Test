using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    
    [Header("Game Over Text")]
    [SerializeField] private TMP_Text finalScoreText;
    
    public static event Action OnStartGame;
    public static event Action OnRestartGame;
    
    private void Awake()
    {
        SubscribeToButtons();
    }
    
    private void Start()
    {
        ShowStartScreen();
    }
    
    private void OnEnable()
    {
        GameManager.OnGameStart += ShowHUD;
        GameManager.OnGameOver += ShowGameOver;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameStart -= ShowHUD;
        GameManager.OnGameOver -= ShowGameOver;
    }

    private void OnDestroy()
    {
        UnsubscribeFromButtons();
    }
    
    private void SubscribeToButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(HandleStartGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(HandleRestartGame);
    }
    
    private void UnsubscribeFromButtons()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartGame);
        
        if (restartButton != null)
            restartButton.onClick.RemoveListener(HandleRestartGame);
    }
    
    private void ShowStartScreen()
    {
        SetPanelActive(startPanel, true);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(hudPanel, false);
        
        AnimatePanelEntry(startPanel);
    }
    
    private void ShowHUD()
    {
        SetPanelActive(startPanel, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(hudPanel, true);
    }
    
    private void ShowGameOver()
    {
        SetPanelActive(hudPanel, false);
        SetPanelActive(gameOverPanel, true);
        
        UpdateFinalScore();
        AnimatePanelEntry(gameOverPanel);
    }
    
    private void HandleStartGame()
    {
        OnStartGame?.Invoke();
        AnimateButtonPress(startButton);
    }
    
    private void HandleRestartGame()
    {
        OnRestartGame?.Invoke();
        AnimateButtonPress(restartButton);
    }
    
    private void UpdateFinalScore()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {gameManager.GetCurrentScore()}";
        }
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
    
    private void AnimatePanelEntry(GameObject panel)
    {
        if (panel == null) return;
        
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    
    private void AnimateButtonPress(Button button)
    {
        if (button != null)
        {
            button.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }
    }
}
