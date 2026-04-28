using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CartridgeTimer : MonoBehaviour
{
    [SerializeField] private Slider  _slider;
    [SerializeField] private float _playDuration = 1f;
    private Tween _valueTween;
    
    private void OnEnable()
    {
        EventManager.Instance.OnTimerUpdate += Refresh;
        EventManager.Instance.OnTimeSet += TimeSet;
    }

    private void TimeSet(int max)
    {
        // 기존 트윈 있으면 종료
        _valueTween?.Kill();
        
        _slider.maxValue = max;
        _slider.value = max;
        
        _valueTween = _slider.DOValue(0, max)
            .SetEase(Ease.Linear);
    }

    private void Refresh(int time)
    {
        // _valueTween?.Kill();
        // float fTime = time;
        //
        // // 현재 값 → 목표 값으로 부드럽게 이동
        // _valueTween = DOTween.To(
        //     () => _slider.value,
        //     x => _slider.value = x,
        //     fTime,
        //     _playDuration // 지속 시간 (조절 가능)
        // );
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnTimerUpdate -= Refresh;          
        EventManager.Instance.OnTimeSet -= TimeSet;
    }
}
