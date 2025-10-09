using System;
using UnityEngine;
using DG.Tweening;

public abstract class UltimateEffectBase : MonoBehaviour,IUltimateEffect
{
    public string BombName;
    
    [Header("Base:이펙트 오브젝트들")]
    public GameObject LineEffectUp;
    public GameObject LineEffectDown;
    
    [Header("Base:Dotween 관련")]
    [Header("Base:LineEffectUp")]
    public Vector3 StartPositionUp;
    public Vector3 EndPositionUp;
    public float DurationUp;
    public Ease EaseUp;
    
    [Header("Base:LineEffectDown")]
    public Vector3 StartPositionDown;
    public Vector3 EndPositionDown;
    public float DurationDown;
    public Ease EaseDown;
    
    [Header("Base:전체 Delay")]
    public float Delay = 0.4f;

    protected virtual void OnEnable()
    {
    }

    public virtual void Play()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendInterval(Delay);
        sequence.AppendCallback(EffectOn);
        sequence.Join(LineEffectUp.transform.DOLocalMove(EndPositionUp, DurationUp).SetEase(EaseUp));
        sequence.Join(LineEffectDown.transform.DOLocalMove(EndPositionDown, DurationDown).SetEase(EaseDown));
        
    }

    public virtual void Stop()
    {
        LineEffectUp.transform.localPosition = StartPositionUp;
        LineEffectDown.transform.localPosition = StartPositionDown;
        
        LineEffectUp.gameObject.SetActive(false);
        LineEffectDown.gameObject.SetActive(false);
    }
    protected virtual void EffectOn()
    {
        LineEffectUp.gameObject.SetActive(true);
        LineEffectDown.gameObject.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        // LineEffectUp.transform.localPosition = StartPositionUp;
        // LineEffectDown.transform.localPosition = StartPositionDown;
        //
        // LineEffectUp.SetActive(false);
        // LineEffectDown.SetActive(false);
    }
}
