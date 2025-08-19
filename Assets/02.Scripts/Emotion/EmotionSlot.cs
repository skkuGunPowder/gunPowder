using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmotionSlot : MonoBehaviour, IDropHandler
{
    public EEmotion Emotion;
    public Animator Animator;
    
    public UI_EmotionPopup Popup;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void Refresh()
    {
        Animator.SetTrigger(Emotion.ToString());
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Emotion = Popup.CurrentEmotion;
        Refresh();
    }
}
