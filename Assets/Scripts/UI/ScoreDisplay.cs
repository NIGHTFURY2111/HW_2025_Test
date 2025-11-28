using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Manages score display with dynamic animations that scale with score value.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    
    [Header("Animation Settings")]
    [SerializeField] private float basePunchScale = 0.3f;
    [SerializeField] private float basePunchRotation = 10f;
    [SerializeField] private float maxPunchScale = 0.8f;
    [SerializeField] private float maxPunchRotation = 25f;
    [SerializeField] private int scoreForMaxEffect = 30;
    
    private int currentScore;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;
    
    private void Awake()
    {
        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
            originalRotation = scoreText.transform.localRotation;
            originalColor = scoreText.color;
        }
    }
    
    private void OnEnable()
    {
        PulpitManager.OnPulpitVisited += OnScoreChanged;
        GameManager.OnGameStart += ResetScore;
    }
    
    private void OnDisable()
    {
        PulpitManager.OnPulpitVisited -= OnScoreChanged;
        GameManager.OnGameStart -= ResetScore;
        
        CleanupAnimations();
    }

    private void OnDestroy()
    {
        CleanupAnimations();
    }
    
    private void Start()
    {
        UpdateScoreText();
    }
    
    private void OnScoreChanged(int points)
    {
        currentScore += points;
        UpdateScoreText();
        PlayScoreAnimation();
    }
    
    private void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
        ResetTransform();
    }
    
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
    
    private void PlayScoreAnimation()
    {
        if (scoreText == null) return;
        
        float intensity = Mathf.Clamp01((float)currentScore / scoreForMaxEffect);
        
        DOTween.Kill(scoreText.transform);
        DOTween.Kill(scoreText);
        ResetTransform();
        
        scoreText.transform.DOPunchScale(Vector3.one * Mathf.Lerp(basePunchScale, maxPunchScale, intensity), 0.5f, 10, 1f)
            .SetAutoKill(true)
            .OnComplete(() => 
            {
                if (scoreText != null)
                {
                    scoreText.transform.localScale = originalScale;
                }
            });
        
        scoreText.transform.DOPunchRotation(new Vector3(0, 0, Mathf.Lerp(basePunchRotation, maxPunchRotation, intensity)), 0.5f, 10, 1f)
            .SetAutoKill(true)
            .OnComplete(() => 
            {
                if (scoreText != null)
                {
                    scoreText.transform.localRotation = originalRotation;
                }
            });
        
        scoreText.DOColor(Color.Lerp(new Color(1f, 1f, 0.5f), Color.yellow, intensity), 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetAutoKill(true)
            .OnComplete(() => 
            {
                if (scoreText != null)
                {
                    scoreText.color = originalColor;
                }
            });
    }
    
    private void ResetTransform()
    {
        if (scoreText == null) return;
        
        scoreText.transform.localScale = originalScale;
        scoreText.transform.localRotation = originalRotation;
        scoreText.color = originalColor;
    }
    
    private void CleanupAnimations()
    {
        if (scoreText != null)
        {
            DOTween.Kill(scoreText.transform);
            DOTween.Kill(scoreText);
        }
    }
}
