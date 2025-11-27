using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    
    [Header("Animation Settings")]
    [SerializeField] private float basePunchScale = 0.3f;
    [SerializeField] private float basePunchRotation = 10f;
    [SerializeField] private float maxPunchScale = 0.8f;
    [SerializeField] private float maxPunchRotation = 25f;
    
    [Header("Scaling")]
    [SerializeField] private int scoreForMaxEffect = 30;
    
    private int currentScore = 0;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;
    
    private void Awake()
    {
        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
            originalRotation = scoreText.transform.localRotation;
            originalColor = Color.white;
        }
    }
    
    private void OnEnable()
    {
        PulpitManager.OnPulpitVisited += OnScoreChanged;
    }
    
    private void OnDisable()
    {
        PulpitManager.OnPulpitVisited -= OnScoreChanged;
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
        
        // Calculate effect intensity based on current score
        float intensity = Mathf.Clamp01((float)currentScore / scoreForMaxEffect);
        
        // Lerp between base and max values
        float punchScale = Mathf.Lerp(basePunchScale, maxPunchScale, intensity);
        float punchRotation = Mathf.Lerp(basePunchRotation, maxPunchRotation, intensity);
        
        // Kill any existing animations
        DOTween.Kill(scoreText.transform);
        DOTween.Kill(scoreText);
        
        // Reset to original state before animating
        scoreText.transform.localScale = originalScale;
        scoreText.transform.localRotation = originalRotation;
        scoreText.color = originalColor;
        
        // Punch scale - grows with score
        scoreText.transform.DOPunchScale(Vector3.one * punchScale, 0.5f, 10, 1f)
            .OnComplete(() => scoreText.transform.localScale = originalScale);
        
        // Punch rotation - spins more at higher scores
        scoreText.transform.DOPunchRotation(new Vector3(0, 0, punchRotation), 0.5f, 10, 1f)
            .OnComplete(() => scoreText.transform.localRotation = originalRotation);
        
        // Color flash - more intense at higher scores
        Color flashColor = Color.Lerp(new Color(1f, 1f, 0.5f), Color.yellow, intensity);
        scoreText.DOColor(flashColor, 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => scoreText.color = originalColor);
    }
}
