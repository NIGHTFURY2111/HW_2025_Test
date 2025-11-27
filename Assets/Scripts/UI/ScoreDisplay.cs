using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private float animationDuration = 0.3f;
    
    private int displayedScore = 0;
    
    private void OnEnable()
    {
        PulpitManager.OnPulpitVisited += UpdateScore;
    }
    
    private void OnDisable()
    {
        PulpitManager.OnPulpitVisited -= UpdateScore;
    }
    
    private void Start()
    {
        UpdateScoreText(0);
    }
    
    private void UpdateScore(int points)
    {
        int newScore = displayedScore + points;
        AnimateScoreChange(newScore);
    }
    
    private void AnimateScoreChange(int targetScore)
    {
        DOTween.To(() => displayedScore, x => {
            displayedScore = x;
            UpdateScoreText(displayedScore);
        }, targetScore, animationDuration)
        .SetEase(Ease.OutQuad);
        
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
    }
    
    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
