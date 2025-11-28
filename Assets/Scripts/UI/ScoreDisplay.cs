using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Manages score display with dynamic animations that scale with score value.
/// </summary>
public class ScoreDisplay : MonoBehaviour
{
    #region Serialized Fields
    
    [SerializeField] private TMP_Text scoreText;
    
    [Header("Animation Settings")]
    [SerializeField] private float basePunchScale = 0.3f;
    [SerializeField] private float basePunchRotation = 10f;
    [SerializeField] private float maxPunchScale = 0.8f;
    [SerializeField] private float maxPunchRotation = 25f;
    [SerializeField] private int scoreForMaxEffect = 30;
    
    #endregion
    
    #region Private Fields
    
    private int currentScore;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;
    
    #endregion
    
    #region Unity Lifecycle
    
    /// <summary>
    /// Initializes original transform values.
    /// </summary>
    private void Awake()
    {
        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
            originalRotation = scoreText.transform.localRotation;
            originalColor = scoreText.color;
        }
    }
    
    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void OnEnable()
    {
        PulpitManager.OnPulpitVisited += OnScoreChanged;
        GameManager.OnGameStart += ResetScore;
    }
    
    /// <summary>
    /// Unsubscribes from game events and cleans up animations.
    /// </summary>
    private void OnDisable()
    {
        PulpitManager.OnPulpitVisited -= OnScoreChanged;
        GameManager.OnGameStart -= ResetScore;
        
        CleanupAnimations();
    }

    /// <summary>
    /// Cleans up animations when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        CleanupAnimations();
    }
    
    /// <summary>
    /// Initializes score display.
    /// </summary>
    private void Start()
    {
        UpdateScoreText();
    }
    
    #endregion
    
    #region Event Handlers
    
    /// <summary>
    /// Handles score change events.
    /// </summary>
    /// <param name="points">Points to add to current score.</param>
    private void OnScoreChanged(int points)
    {
        currentScore += points;
        UpdateScoreText();
        PlayScoreAnimation();
    }
    
    /// <summary>
    /// Resets the score to zero.
    /// </summary>
    private void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
        ResetTransform();
    }
    
    #endregion
    
    #region Display Updates
    
    /// <summary>
    /// Updates the score text display.
    /// </summary>
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
    
    #endregion
    
    #region Animation Methods
    
    /// <summary>
    /// Plays score increment animation with intensity based on current score.
    /// </summary>
    private void PlayScoreAnimation()
    {
        if (scoreText == null) return;
        
        // Calculate animation intensity based on score
        float intensity = Mathf.Clamp01((float)currentScore / scoreForMaxEffect);
        
        DOTween.Kill(scoreText.transform);
        DOTween.Kill(scoreText);
        ResetTransform();
        
        // Scale punch animation
        scoreText.transform.DOPunchScale(Vector3.one * Mathf.Lerp(basePunchScale, maxPunchScale, intensity), 0.5f, 10, 1f)
            .SetAutoKill(true)
            .OnComplete(() => 
            {
                if (scoreText != null)
                {
                    scoreText.transform.localScale = originalScale;
                }
            });
        
        // Rotation punch animation
        scoreText.transform.DOPunchRotation(new Vector3(0, 0, Mathf.Lerp(basePunchRotation, maxPunchRotation, intensity)), 0.5f, 10, 1f)
            .SetAutoKill(true)
            .OnComplete(() => 
            {
                if (scoreText != null)
                {
                    scoreText.transform.localRotation = originalRotation;
                }
            });
        
        // Color flash animation
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
    
    /// <summary>
    /// Resets score text transform to original values.
    /// </summary>
    private void ResetTransform()
    {
        if (scoreText == null) return;
        
        scoreText.transform.localScale = originalScale;
        scoreText.transform.localRotation = originalRotation;
        scoreText.color = originalColor;
    }
    
    /// <summary>
    /// Cleans up all active animations.
    /// </summary>
    private void CleanupAnimations()
    {
        if (scoreText != null)
        {
            DOTween.Kill(scoreText.transform);
            DOTween.Kill(scoreText);
        }
    }
    
    #endregion
}
