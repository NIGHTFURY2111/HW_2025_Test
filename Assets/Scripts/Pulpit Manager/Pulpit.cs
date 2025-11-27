using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(Collider))]
public class Pulpit : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    
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
    private Tween colorTween;
    private Tween scaleTween;
    private bool isInWarningState;
    private bool isInDangerState;
    
    public event Action<Pulpit> OnPulpitDestroyed;
    public event Action<Pulpit> OnPlayerEntered;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        Debug.Log($"[Pulpit] Awake - originalScale: {originalScale}");
    }
    
    private void OnEnable()
    {
        if (meshRenderer != null)
        {
            pulpitMaterial = meshRenderer.material;
            Debug.Log($"[Pulpit] OnEnable - Material assigned: {pulpitMaterial != null}");
        }
        else
        {
            Debug.LogError($"[Pulpit] OnEnable - MeshRenderer is NULL!");
        }
        PlaySpawnAnimation();
    }
    
    private void OnDisable()
    {
        KillTweens();
    }
    
    public void Initialize(float lifetimeValue)
    {
        lifetime = lifetimeValue;
        elapsedTime = 0f;
        hasPlayerVisited = false;
        isInWarningState = false;
        isInDangerState = false;
        
        Debug.Log($"[Pulpit] Initialize - Lifetime: {lifetime}s, Warning at: {lifetime * warningThreshold}s, Danger at: {lifetime * dangerThreshold}s");
        
        if (pulpitMaterial != null)
        {
            pulpitMaterial.color = normalColor;
            Debug.Log($"[Pulpit] Initialize - Set color to: {normalColor}");
        }
    }
    
    public void UpdateTimer(float deltaTime)
    {
        elapsedTime += deltaTime;
        
        float timeRemaining = lifetime - elapsedTime;
        float normalizedTime = timeRemaining / lifetime;
        
        Debug.Log($"[Pulpit] UpdateTimer - Elapsed: {elapsedTime:F2}s / {lifetime:F2}s, Normalized: {normalizedTime:F2}, Material: {pulpitMaterial != null}");
        
        UpdateVisualFeedback(normalizedTime);
        
        if (elapsedTime >= lifetime)
        {
            Debug.Log($"[Pulpit] UpdateTimer - Lifetime reached! Destroying pulpit.");
            DestroyPulpit();
        }
    }
    
    public bool ShouldSpawnNext(float spawnTime)
    {
        return elapsedTime >= spawnTime;
    }
    
    private void UpdateVisualFeedback(float normalizedTime)
    {
        if (pulpitMaterial == null)
        {
            Debug.LogError($"[Pulpit] UpdateVisualFeedback - Material is NULL!");
            return;
        }
        
        Debug.Log($"[Pulpit] UpdateVisualFeedback - NormTime: {normalizedTime:F2}, Warning: {!isInWarningState}, Danger: {!isInDangerState}");
        
        if (normalizedTime <= dangerThreshold && !isInDangerState)
        {
            Debug.Log($"[Pulpit] ENTERING DANGER STATE (normTime: {normalizedTime:F2} <= {dangerThreshold})");
            isInDangerState = true;
            isInWarningState = false;
            AnimateToDangerState();
        }
        else if (normalizedTime <= warningThreshold && normalizedTime > dangerThreshold && !isInWarningState)
        {
            Debug.Log($"[Pulpit] ENTERING WARNING STATE (normTime: {normalizedTime:F2} <= {warningThreshold})");
            isInWarningState = true;
            isInDangerState = false;
            AnimateToWarningState();
        }
    }
    
    private void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        
        scaleTween = transform.DOScale(originalScale, spawnDuration)
            .SetEase(Ease.OutBack);
        
        if (pulpitMaterial != null)
        {
            pulpitMaterial.color = normalColor;
            Debug.Log($"[Pulpit] PlaySpawnAnimation - Color set to: {pulpitMaterial.color}");
        }
    }
    
    private void AnimateToWarningState()
    {
        KillColorTween();
        
        if (pulpitMaterial != null)
        {
            Debug.Log($"[Pulpit] AnimateToWarningState - Starting tween from {pulpitMaterial.color} to {warningColor}");
            
            colorTween = DOTween.To(() => pulpitMaterial.color, 
                x => pulpitMaterial.color = x, 
                warningColor, 
                0.5f)
                .SetEase(Ease.InOutSine)
                .SetTarget(pulpitMaterial)
                .OnUpdate(() => Debug.Log($"[Pulpit] WARNING Tween Update - Color: {pulpitMaterial.color}"))
                .OnComplete(() => Debug.Log($"[Pulpit] WARNING Tween Complete"));
        }
    }
    
    private void AnimateToDangerState()
    {
        KillColorTween();
        
        if (pulpitMaterial != null)
        {
            Debug.Log($"[Pulpit] AnimateToDangerState - Starting loop tween from {pulpitMaterial.color} to {dangerColor}");
            
            colorTween = DOTween.To(() => pulpitMaterial.color, 
                x => pulpitMaterial.color = x, 
                dangerColor, 
                0.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(pulpitMaterial)
                .OnUpdate(() => Debug.Log($"[Pulpit] DANGER Tween Update - Color: {pulpitMaterial.color}"));
        }
    }
    
    private void DestroyPulpit()
    {
        KillTweens();
        
        OnPulpitDestroyed?.Invoke(this);
        
        scaleTween = transform.DOScale(Vector3.zero, destroyDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
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
    
    private void PlayLandAnimation()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
    }
    
    private void KillTweens()
    {
        KillColorTween();
        KillScaleTween();
    }
    
    private void KillColorTween()
    {
        if (colorTween != null && colorTween.IsActive())
        {
            colorTween.Kill();
            Debug.Log($"[Pulpit] Color tween killed");
        }
    }
    
    private void KillScaleTween()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }
    }
    
    private void OnDestroy()
    {
        KillTweens();
        
        if (pulpitMaterial != null)
        {
            Destroy(pulpitMaterial);
        }
    }
}
