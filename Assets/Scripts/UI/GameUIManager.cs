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
        if (startButton != null)
            startButton.onClick.AddListener(HandleStartGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(HandleRestartGame);
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
    
    private void Start()
    {
        ShowStartScreen();
    }
    
    private void ShowStartScreen()
    {
        SetPanelActive(startPanel, true);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(hudPanel, false);
        
        if (startPanel != null)
        {
            startPanel.transform.localScale = Vector3.zero;
            startPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
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
        
        // Get final score
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && finalScoreText != null)
        {
            int finalScore = gameManager.GetCurrentScore();
            finalScoreText.text = $"Final Score: {finalScore}";
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.transform.localScale = Vector3.zero;
            gameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }
    
    private void HandleStartGame()
    {
        OnStartGame?.Invoke();
        
        if (startButton != null)
        {
            startButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }
    }
    
    private void HandleRestartGame()
    {
        OnRestartGame?.Invoke();
        
        if (restartButton != null)
        {
            restartButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        }
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
    
    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartGame);
        
        if (restartButton != null)
            restartButton.onClick.RemoveListener(HandleRestartGame);
    }
}
