using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

/// <summary>
/// Manages individual pulpit behavior including lifecycle, animations, and player detection.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    
    [Header("Timer Display")]
    [SerializeField] private TMP_Text timerText;
    
    [Header("Animation Settings")]
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private float destroyDuration = 0.3f;
    [SerializeField] private float warningThreshold = 0.3f;
    [SerializeField] private float dangerThreshold = 0.15f;
    
    private float lifetime;
    private float elapsedTime;
    private bool hasPlayerVisited;
    private Material pulpitMaterial;
    private Vector3 originalScale;
    private Vector3 timerOriginalScale;
    private Quaternion timerOriginalRotation;
    
    private enum VisualState { Normal, Warning, Danger }
    private VisualState currentState = VisualState.Normal;

    public event Action<Pulpit> OnPulpitDestroyed;
    public event Action<Pulpit> OnPlayerEntered;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        
        if (timerText != null)
        {
            timerOriginalScale = timerText.transform.localScale;
            timerOriginalRotation = timerText.transform.localRotation;
        }
        
        if (TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            pulpitMaterial = meshRenderer.material;
        }
    }
    
    private void OnEnable()
    {
        PlaySpawnAnimation();
    }
    
    private void OnDisable()
    {
        KillAllTweens();
        transform.localScale = originalScale;
        ResetTimerTransform();
    }

    private void OnDestroy()
    {
        KillAllTweens();
        
        if (pulpitMaterial != null)
        {
            Destroy(pulpitMaterial);
            pulpitMaterial = null;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayerVisited)
        {
            hasPlayerVisited = true;
            OnPlayerEntered?.Invoke(this);
            
            if (transform != null)
            {
                transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f).SetAutoKill(true);
            }
            
            if (timerText != null)
            {
                timerText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f).SetAutoKill(true);
            }
        }
    }
    
    public void Initialize(float lifetimeValue)
    {
        lifetime = lifetimeValue;
        elapsedTime = 0f;
        hasPlayerVisited = false;
        currentState = VisualState.Normal;
        
        if (pulpitMaterial != null)
        {
            pulpitMaterial.color = normalColor;
        }
        
        UpdateTimerDisplay();
    }
    
    public void UpdateTimer(float deltaTime)
    {
        elapsedTime += deltaTime;
        
        UpdateTimerDisplay();
        UpdateVisualState((lifetime - elapsedTime) / lifetime);
        
        if (elapsedTime >= lifetime)
        {
            DestroyPulpit();
        }
    }
    
    public bool ShouldSpawnNext(float spawnTime) => elapsedTime >= spawnTime;
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        float timeRemaining = Mathf.Max(0, lifetime - elapsedTime);
        float normalizedTime = timeRemaining / lifetime;
        
        timerText.text = $"{timeRemaining:F1}s";
        timerText.color = normalizedTime <= dangerThreshold ? dangerColor :
                         normalizedTime <= warningThreshold ? warningColor : 
                         Color.white;
    }
    
    private void UpdateVisualState(float normalizedTime)
    {
        if (pulpitMaterial == null) return;
        
        VisualState newState = normalizedTime <= dangerThreshold ? VisualState.Danger :
                              normalizedTime <= warningThreshold ? VisualState.Warning :
                              VisualState.Normal;
        
        if (newState != currentState)
        {
            currentState = newState;
            TransitionToState(newState);
        }
    }
    
    private void TransitionToState(VisualState state)
    {
        KillAllTweens();
        
        if (state == VisualState.Warning)
        {
            AnimateStateTransition(warningColor, 0.5f, false, 0.5f, 0.3f, false);
        }
        else if (state == VisualState.Danger)
        {
            AnimateStateTransition(dangerColor, 0.3f, true, 0.2f, 0.2f, true);
        }
    }
    
    private void AnimateStateTransition(Color color, float colorDuration, bool loopColor, 
        float fadeTarget, float fadeDuration, bool includePulse)
    {
        if (pulpitMaterial != null)
        {
            Color tempColor = pulpitMaterial.color;
            var colorTween = DOTween.To(() => tempColor, x => 
                {
                    tempColor = x;
                    if (pulpitMaterial != null)
                    {
                        pulpitMaterial.color = x;
                    }
                }, color, colorDuration)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(true);
            
            if (loopColor) colorTween.SetLoops(-1, LoopType.Yoyo);
        }
        
        if (timerText != null)
        {
            timerText.DOFade(fadeTarget, fadeDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true);
            
            if (includePulse)
            {
                timerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5, 0.5f)
                    .SetLoops(-1)
                    .SetAutoKill(true);
            }
        }
    }
    
    private void PlaySpawnAnimation()
    {
        if (transform != null)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(originalScale, spawnDuration)
                .SetEase(Ease.OutBack)
                .SetAutoKill(true);
        }
        
        if (pulpitMaterial != null)
        {
            pulpitMaterial.color = normalColor;
        }
        
        if (timerText != null)
        {
            timerText.transform.localScale = Vector3.zero;
            timerText.transform.DOScale(timerOriginalScale, spawnDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(0.2f)
                .SetAutoKill(true);
        }
    }
    
    private void DestroyPulpit()
    {
        KillAllTweens();
        OnPulpitDestroyed?.Invoke(this);
        
        if (transform != null)
        {
            transform.DOScale(Vector3.zero, destroyDuration)
                .SetEase(Ease.InBack)
                .SetAutoKill(true)
                .OnComplete(() => 
                {
                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }
                });
        }
            
        if (timerText != null)
        {
            timerText.transform.DOScale(Vector3.zero, destroyDuration)
                .SetEase(Ease.InBack)
                .SetAutoKill(true);
        }
    }
    
    private void ResetTimerTransform()
    {
        if (timerText == null) return;
        
        timerText.transform.localScale = timerOriginalScale;
        timerText.transform.localRotation = timerOriginalRotation;
    }
    
    private void KillAllTweens()
    {
        if (transform != null) DOTween.Kill(transform);
        if (timerText != null)
        {
            DOTween.Kill(timerText);
            DOTween.Kill(timerText.transform);
        }
    }
}
