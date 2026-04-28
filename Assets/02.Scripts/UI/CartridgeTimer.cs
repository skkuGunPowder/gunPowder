using UnityEngine;
using UnityEngine.UI;

public class CartridgeTimer : MonoBehaviour
{
    [SerializeField] private Slider  _slider;
    
    private void OnEnable()
    {
        EventManager.Instance.OnTimerUpdate += Refresh;
        EventManager.Instance.OnTimeSet += TimeSet;
    }

    private void TimeSet(int max)
    {
        _slider.maxValue = max;
        _slider.value = max;
    }

    private void Refresh(int time)
    {
        _slider.value = time;
    }
    private void OnDisable()
    {
        EventManager.Instance.OnTimerUpdate -= Refresh;          
        EventManager.Instance.OnTimeSet -= TimeSet;
    }
}
