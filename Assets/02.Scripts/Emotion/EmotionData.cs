using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmotionData : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UI_EmotionPopup Popup;
    
    public EEmotion Emotion;
    public Image DraggedObject;
    private Animator _myAnimator;

    private void Awake()
    {
        _myAnimator = GetComponent<Animator>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        DraggedObject.gameObject.SetActive(true);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DraggedObject.rectTransform.parent as RectTransform, // 기준이 되는 부모 RectTransform
            Input.mousePosition,                                  // 마우스 스크린 좌표
            Camera.main,                                            // UI 카메라 (Canvas에 할당된 카메라)
            out localPos
        );

        DraggedObject.rectTransform.anchoredPosition = localPos;
        
        Popup.CurrentEmotion = Emotion;
    }
    public void OnDrop(PointerEventData eventData)
    {
        DraggedObject.gameObject.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _myAnimator.SetTrigger(Emotion.ToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _myAnimator.SetTrigger("Exit");
    }

}