using System;
using DG.Tweening;
using UnityEngine;

public class GameStartProduction : MonoBehaviour
{
    public RectTransform Timer;
    public RectTransform Profile;
    [Header("게임 시작 Dotween")]
    public float DotweenDuration;
    public Vector2 TimerEndPosition;
    public Ease TimerEase;
    public Vector2 ProfileEndPosition;
    public Ease ProfileEase;
    
    
    [Header("원래 위치 조정")] 
    public Vector2 TimerOriginPosition;
    public Vector2 ProfileOriginPosition;

    private void Awake()
    {
        EventManager.Instance.OnLoadFinished += Play;
        Debug.Log("gamestart");
    }

    public void Play()
    {
        Debug.Log("production");
        Timer.DOAnchorPos(TimerEndPosition, DotweenDuration).SetEase(TimerEase);
        Profile.DOAnchorPos(ProfileEndPosition, DotweenDuration).SetEase(ProfileEase);
        
    }

    private void OnDisable()
    {
        Timer.anchoredPosition = TimerOriginPosition;
        Profile.anchoredPosition = ProfileOriginPosition;
        EventManager.Instance.OnLoadFinished -= Play;
    }
}
