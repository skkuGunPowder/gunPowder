using UnityEngine;

public interface IKnockable
{
    void Knockback(Vector2 explosionOrigin, float power, float duration, DG.Tweening.Ease easeType = DG.Tweening.Ease.OutBack);
}
