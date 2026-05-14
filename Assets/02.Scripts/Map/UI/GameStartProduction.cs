using System;
using DG.Tweening;
using UnityEngine;

public class GameStartProduction : MonoBehaviour
{
    public RectTransform TopPivot;
    public RectTransform Profile;
    public GameObject Timer;
    [Header("게임 시작 텍스트")]
    public GameObject GameStartCountText1;
    public GameObject GameStartText;
    public GameObject GameStartCountText2;
    public GameObject GameStartCountText3;
    
    [Header("게임 시작 Dotween")]
    public float DotweenDuration;
    public int TopPivotY;
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
    
    [Header("사운드")]
    public AudioClip GameStartBell_1;
    private void Awake()
    {
        // EventManager.Instance.OnLoadFinished += Play;
    }

    private void Start()
    {
        // 중복 구독 방어
        EventManager.Instance.OnGameStart -= GameStart;
        EventManager.Instance.OnGameStart += GameStart;
    }
    public void Play()
    {
        Timer.gameObject.SetActive(true);
        DOTween.To(() => TopPivot.offsetMax, x => TopPivot.offsetMax = x, new Vector2(TopPivot.offsetMax.x, TopPivotY),
            DotweenDuration).SetEase(TimerEase).SetUpdate(true);
        Profile.DOAnchorPos(ProfileEndPosition, DotweenDuration).SetEase(ProfileEase).SetUpdate(true);
    }

    public void GameStart()
    {
        InputHandler.BlockInput = true;
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.AppendCallback(Play);
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
        sequence.JoinCallback(SoundStart);
        sequence.AppendInterval(GameStartTextInterval);
        sequence.Append(GameStartText.transform.DOScale(GameStartTextScaleOrigin, GameStartTextSpeed)
            .SetEase(GameStartTextEase));
        // sequence.OnComplete(() =>
        // {
        //     EventManager.Instance.OnGameStart -= GameStart;
        // });
    }
    
    private void SoundStart()
    {
        GameManager.Instance.GameStartSetting();
        SoundManager.Instance.PlayLocalSound(nameof(GameStartBell_1), transform, 0f, false, SoundType.SFX, true, 0.5f, 0.5f);
        PopupCloseCheck();
        // InputHandler.BlockInput = false;
        // BombSelect 상태일 때만 폭탄 선택 단계 시작 (Playing 상태에서는 미발동)
    }
    
    // 팝업창이 여전히 열려있는지 체크
    private void PopupCloseCheck()
    {
        UI_TempStorage popup = (UI_TempStorage)PopupManager.Instance.GetPopup(EPopupType.UI_TempStorage);

        if (popup.gameObject.activeSelf)
        {
            popup.SetCloseCallback(() => InputHandler.BlockInput = false);
            InputHandler.BlockInput = true;
        }
        else
        {
            InputHandler.BlockInput = false;
        }
    }
    private void OnDisable()
    {
        TopPivot.offsetMax = TimerOriginPosition;
        Profile.anchoredPosition = ProfileOriginPosition;
        
        EventManager.Instance.OnGameStart -= GameStart;
        // EventManager.Instance.OnLoadFinished -= Play;
    }
}
