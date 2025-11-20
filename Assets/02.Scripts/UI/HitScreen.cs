using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HitScreen : MonoBehaviour
{
    public Image HitScreenImage;
    public float Value = 0.3f;
    public int MaxValue = 1;
    public float FadeSpeed = 0.5f;
    public float FadeInSpeed = 0.3f;
    private Sequence currentSequence;

    private void Awake()
    {
        EventManager.Instance.OnHitScreen += PlayHitScreen;
    }

    private void PlayHitScreen()
    {
        // 이전 시퀀스가 있다면 완료 콜백을 제거하고 Kill
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill(false); // false 파라미터로 현재 값을 유지
        }

        float currentValue = HitScreenImage.color.a;
        currentValue = Mathf.Min(currentValue + Value, MaxValue);
        Play(currentValue);
    }

    private void Play(float fade)
    {
        currentSequence = DOTween.Sequence();
        currentSequence.Append(HitScreenImage.DOFade(fade, FadeInSpeed));
        currentSequence.Append(HitScreenImage.DOFade(0, FadeSpeed).SetEase(Ease.InCirc));
    }
    private void OnDestroy()
    {
        EventManager.Instance.OnHitScreen -= PlayHitScreen;
    }

}
