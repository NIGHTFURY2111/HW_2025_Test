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
    private Tween colorTween;
    private Tween scaleTween;
    private bool isInWarningState;
    private bool isInDangerState;
    
    public event Action<Pulpit> OnPulpitDestroyed;
    public event Action<Pulpit> OnPlayerEntered;
    
    private void Awake()
    {
        CacheOriginalTransforms();
    }
    
    private void OnEnable()
    {
        InitializeMaterial();
        PlaySpawnAnimation();
    }
    
    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
        DestroyMaterial();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayerVisited)
        {
            hasPlayerVisited = true;
            OnPlayerEntered?.Invoke(this);
            PlayLandAnimation();
        }
    }
    
    public void Initialize(float lifetimeValue)
    {
        lifetime = lifetimeValue;
        elapsedTime = 0f;
        hasPlayerVisited = false;
        isInWarningState = false;
        isInDangerState = false;
        
        if (pulpitMaterial != null)
        {
            pulpitMaterial.color = normalColor;
        }
        
        UpdateTimerDisplay();
    }
    
    public void UpdateTimer(float deltaTime)
    {
        elapsedTime += deltaTime;
        
        float normalizedTime = (lifetime - elapsedTime) / lifetime;
        
        UpdateTimerDisplay();
        UpdateVisualFeedback(normalizedTime);
        
        if (elapsedTime >= lifetime)
        {
            DestroyPulpit();
        }
    }
    
    public bool ShouldSpawnNext(float spawnTime) => elapsedTime >= spawnTime;
    
    private void CacheOriginalTransforms()
    {
        originalScale = transform.localScale;
        
        if (timerText != null)
        {
            timerOriginalScale = timerText.transform.localScale;
            timerOriginalRotation = timerText.transform.localRotation;
        }
    }
    
    private void InitializeMaterial()
    {
        if (TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            pulpitMaterial = meshRenderer.material;
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        float timeRemaining = Mathf.Max(0, lifetime - elapsedTime);
        timerText.text = timeRemaining.ToString("F1") + "s";
        
        float normalizedTime = timeRemaining / lifetime;
        
        timerText.color = normalizedTime <= dangerThreshold ? dangerColor :
                         normalizedTime <= warningThreshold ? warningColor : 
                         Color.white;
    }
    
    private void UpdateVisualFeedback(float normalizedTime)
    {
        if (pulpitMaterial == null) return;
        
        if (normalizedTime <= dangerThreshold && !isInDangerState)
        {
            isInDangerState = true;
            isInWarningState = false;
            AnimateToDangerState();
        }
        else if (normalizedTime <= warningThreshold && normalizedTime > dangerThreshold && !isInWarningState)
        {
            isInWarningState = true;
            isInDangerState = false;
            AnimateToWarningState();
        }
    }
    
    private void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        
        scaleTween = transform.DOScale(originalScale, spawnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => transform.localScale = originalScale);
        
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
                .OnComplete(ResetTimerTransform);
        }
    }
    
    private void AnimateToWarningState()
    {
        KillColorTween();
        KillTimerTweens();
        
        if (pulpitMaterial != null)
        {
            colorTween = DOTween.To(() => pulpitMaterial.color, 
                x => pulpitMaterial.color = x, 
                warningColor, 
                0.5f)
                .SetEase(Ease.InOutSine)
                .SetTarget(pulpitMaterial);
        }
        
        if (timerText != null)
        {
            timerText.DOFade(0.5f, 0.3f).SetLoops(-1, LoopType.Yoyo).SetTarget(timerText);
        }
    }
    
    private void AnimateToDangerState()
    {
        KillColorTween();
        KillTimerTweens();
        
        if (pulpitMaterial != null)
        {
            colorTween = DOTween.To(() => pulpitMaterial.color, 
                x => pulpitMaterial.color = x, 
                dangerColor, 
                0.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(pulpitMaterial);
        }
        
        if (timerText != null)
        {
            timerText.DOFade(0.2f, 0.2f).SetLoops(-1, LoopType.Yoyo).SetTarget(timerText);
            timerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 5, 0.5f)
                .SetLoops(-1)
                .SetTarget(timerText.transform)
                .OnStepComplete(() => timerText.transform.localScale = timerOriginalScale);
        }
    }
    
    private void DestroyPulpit()
    {
        KillTweens();
        OnPulpitDestroyed?.Invoke(this);
        
        scaleTween = transform.DOScale(Vector3.zero, destroyDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
            
        if (timerText != null)
        {
            timerText.transform.DOScale(Vector3.zero, destroyDuration).SetEase(Ease.InBack);
        }
    }
    
    private void PlayLandAnimation()
    {
        Vector3 currentScale = transform.localScale;
        
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f)
            .OnComplete(() => transform.localScale = currentScale);
        
        if (timerText != null)
        {
            Vector3 currentTimerScale = timerText.transform.localScale;
            
            timerText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f)
                .OnComplete(ResetTimerTransform);
        }
    }
    
    private void ResetTimerTransform()
    {
        if (timerText == null) return;
        
        timerText.transform.localScale = timerOriginalScale;
        timerText.transform.localRotation = timerOriginalRotation;
    }
    
    private void KillTweens()
    {
        KillColorTween();
        KillScaleTween();
        KillTimerTweens();
        
        if (!gameObject.activeSelf) return;
        
        transform.localScale = originalScale;
        ResetTimerTransform();
    }
    
    private void KillTimerTweens()
    {
        if (timerText != null)
        {
            DOTween.Kill(timerText);
            DOTween.Kill(timerText.transform);
        }
    }
    
    private void KillColorTween()
    {
        if (colorTween != null && colorTween.IsActive())
        {
            colorTween.Kill();
        }
    }
    
    private void KillScaleTween()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }
    }
    
    private void DestroyMaterial()
    {
        if (pulpitMaterial != null)
        {
            Destroy(pulpitMaterial);
        }
    }
}
