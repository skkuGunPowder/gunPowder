using System;
using DG.Tweening;
using UnityEngine;

public class GameStartProduction : MonoBehaviour
{
    public RectTransform Timer;
    public RectTransform Profile;
    [Header("게임 시작 텍스트")]
    public GameObject GameStartCountText1;
    public GameObject GameStartText;
    public GameObject GameStartCountText2;
    public GameObject GameStartCountText3;
    
    [Header("게임 시작 Dotween")]
    public float DotweenDuration;
    public Vector2 TimerEndPosition;
    public Ease TimerEase;
    public Vector2 ProfileEndPosition;
    public Ease ProfileEase;
    
    public float GameStartTextInterval;
    public float GameStartTextSpeed;
    public Vector3 GameStartTextScale;
    public Ease GameStartTextEase;
    
    [Header("원래 위치 조정")] 
    public Vector2 TimerOriginPosition;
    public Vector2 ProfileOriginPosition;
    public Vector3 GameStartTextScaleOrigin;
    
    private void Awake()
    {
        EventManager.Instance.OnLoadFinished += Play;
    }

    public void Play()
    {
        Timer.DOAnchorPos(TimerEndPosition, DotweenDuration).SetEase(TimerEase).SetUpdate(true);
        Profile.DOAnchorPos(ProfileEndPosition, DotweenDuration).SetEase(ProfileEase).SetUpdate(true);
    }

    public void GameStart()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(GameStartCountText3.transform.DOScale(GameStartTextScale, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        sequence.AppendInterval(GameStartTextInterval);
        sequence.Append(GameStartCountText2.transform.DOScale(GameStartTextScale, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        sequence.Join(GameStartCountText3.transform.DOScale(GameStartTextScaleOrigin,GameStartTextSpeed / 2)
            .SetEase(GameStartTextEase));
        sequence.AppendInterval(GameStartTextInterval);
        sequence.Append(GameStartCountText1.transform.DOScale(GameStartTextScale, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        sequence.Join(GameStartCountText2.transform.DOScale(GameStartTextScaleOrigin,GameStartTextSpeed / 2)
            .SetEase(GameStartTextEase));
        sequence.AppendInterval(GameStartTextInterval);
        sequence.Append(GameStartText.transform.DOScale(GameStartTextScale, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        sequence.Join(GameStartCountText1.transform.DOScale(GameStartTextScaleOrigin,GameStartTextSpeed / 2)
            .SetEase(GameStartTextEase));
        sequence.AppendInterval(GameStartTextInterval);
        sequence.Append(GameStartText.transform.DOScale(GameStartTextScaleOrigin, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        sequence.OnComplete(() =>
        {
            GameManager.Instance.GameStateChange(EGameState.Playing);
        });
    }
    private void OnDisable()
    {
        Timer.anchoredPosition = TimerOriginPosition;
        Profile.anchoredPosition = ProfileOriginPosition;
        EventManager.Instance.OnLoadFinished -= Play;
    }
}
