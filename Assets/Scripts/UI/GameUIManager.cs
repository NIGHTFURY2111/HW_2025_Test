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
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    
    public static event Action OnStartGame;
    public static event Action OnRestartGame;
    
    private void Awake()
    {
        startButton?.onClick.AddListener(HandleStartGame);
        restartButton?.onClick.AddListener(HandleRestartGame);
    }
    
    private void Start()
    {
        ShowPanel(startPanel);
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
        
        CleanupAnimations();
    }

    private void OnDestroy()
    {
        startButton?.onClick.RemoveListener(HandleStartGame);
        restartButton?.onClick.RemoveListener(HandleRestartGame);
        
        CleanupAnimations();
    }
    
    private void CleanupAnimations()
    {
        if (startPanel != null) startPanel.transform.DOKill();
        if (gameOverPanel != null) gameOverPanel.transform.DOKill();
        if (hudPanel != null) hudPanel.transform.DOKill();
        if (startButton != null) startButton.transform.DOKill();
        if (restartButton != null) restartButton.transform.DOKill();
    }
    
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
    
    private void ShowHUD()
    {
        ShowPanel(hudPanel);
    }
    
    private void ShowGameOver()
    {
        if (finalScoreText != null && gameManager != null)
        {
            finalScoreText.text = $"Final Score: {gameManager.GetCurrentScore()}";
        }
        ShowPanel(gameOverPanel);
    }
    
    private void HandleStartGame()
    {
        if (startButton != null)
        {
            AnimateButtonPress(startButton);
        }
        OnStartGame?.Invoke();
    }
    
    private void HandleRestartGame()
    {
        if (restartButton != null)
        {
            AnimateButtonPress(restartButton);
        }
        OnRestartGame?.Invoke();
    }
    
    private void SetPanelState(GameObject panel, bool active)
    {
        panel?.SetActive(active);
    }
    
    private void AnimatePanelEntry(GameObject panel)
    {
        if (panel == null) return;
        
        panel.transform.DOKill();
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    
    private void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        
        button.transform.DOKill();
        button.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
    }
}
