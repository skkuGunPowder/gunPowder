using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(DOTweenAnimation))]
public class Pointer_DownUp : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{
    // Enter시 나올 사운드
    public AudioClip Sound;
    
    // Dotween을 적용시킬 것이면
    public DOTweenAnimation DotAnimation;
    
    //적용시킬 것들
    public bool IsSoundOn;
    public bool IsTweeningOn;

    private void Awake()
    {
        DotAnimation = gameObject.GetComponent<DOTweenAnimation>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsTweeningOn == false)
        {
            return;
        }
        DotAnimation.DOPlayForward();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsTweeningOn == false)
        {
            return;
        }

        DotAnimation.DOPlayBackwards();
    }

}
