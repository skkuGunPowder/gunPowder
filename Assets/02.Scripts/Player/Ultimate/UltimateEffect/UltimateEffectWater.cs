using DG.Tweening;
using UnityEngine;

public class UltimateEffectWater : UltimateEffectBase
{
    [Header("오버라이드")]
    public GameObject LineEffect;
    
    public override void Play()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendInterval(Delay);
        sequence.AppendCallback(EffectOn);
    }

    public override void Stop()
    {
        InnerEffect.SetActive(false);
        LineEffect.SetActive(false);
    }

    protected override void EffectOn()
    {
        InnerEffect.SetActive(true);
        LineEffect.SetActive(true);
    }

    protected override void OnDisable()
    {
        // // InnerEffect.SetActive(false);
        // LineEffect.SetActive(false);
    }
}
