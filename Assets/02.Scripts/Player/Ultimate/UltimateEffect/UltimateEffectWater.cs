using DG.Tweening;
using UnityEngine;

public class UltimateEffectWater : UltimateEffectBase
{
    public override void Play()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendInterval(Delay);
        sequence.AppendCallback(EffectOn);
    }

    public override void Stop()
    {
        LineEffectUp.SetActive(false);
    }

    protected override void EffectOn()
    {
        LineEffectUp.SetActive(true);
    }

    protected override void OnDisable()
    {
        // // InnerEffect.SetActive(false);
        // LineEffect.SetActive(false);
    }
}
