using System;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameOverProduction : MonoBehaviour
{
    public Animator MyAnimator;
    private Camera _camera;
    public bool Test;
    [Header("위치 액션")] 
    public RectTransform GameOverProductionPanel;
    public RectTransform ProfileSlot;
    public RectTransform TopPivot;
    [Header("카메라 액션")]
    public RectTransform CameraObject;
    public Image BlackOut;

    [Header("ForDotween")] 
    [Space]
    [Header("Gameset글자 관련")]
    [Tooltip("GameSet 글자가 멈추는 곳")] public Vector2 GameSetPosition;
    [Tooltip("GameSet이 떨어지는 Ease : 초기값 OutBounce")] public Ease GameSetEase = Ease.OutBounce;
    [Tooltip("GameSet이 떨어지는 속도 (시간)")] public float GameSetTime = 0.5f;
    [Tooltip("GameSet이 떨어지고 잠깐 기다리는 시간 1초 이상 주어야함")] public float GameSetWaitTime = 1f;
    [Space]
    [Header("카메라 관련")]
    [Tooltip("카메라가 줌 아웃이 되는 시간")] public float CameraZoomOutTime = 1f;
    [Tooltip("카메라가 줌 아웃이 되는 양")] public float CameraZoomOutAmount = 11f;
    [Tooltip("카메라가 줌 아웃되는 Ease")] public Ease CameraZoomOutEase = Ease.InCirc;
    [Space]
    [Header("CameraObject관련")]
    [Tooltip("카메라 이미지의 스케일 : 튀어나오는 느낌 주기 최소2.1이어야 처음에 안보임")] public float CameraScale = 2.1f;
    [Tooltip("카메라 이미지가 나오는 속도")] public float CameraScaleTime = 0.5f; 
    [Tooltip("카메라 이미지가 나오는 Ease")] public Ease CameraScaleEase = Ease.InCirc;
    [Tooltip("카메라 이미지 버튼이 눌리는 모션이 나오는 타이밍 : 이미지가 등장하고 얼마 뒤에 눌릴 것인가?")] public float CameraButtonTime = 0.5f;
    [Header("화면 꺼지기 BlackOut")]
    [Tooltip("화면이 꺼지는 시간 : 카메라 버튼이 눌리고 나서 얼마 이후에 꺼질 것인가?")] public float BlackOutTime = 1f;
    [Tooltip("다음씬으로 넘어가는 시간 : 화면이 꺼지고 나서 얼마 이후에 넘어갈 것인가?")] public float NextSceneTime = 0.5f;
    [Tooltip("BlackOut이 Fade되는 시간")] public float BlackOutFadeTime = 0.5f;
    [Tooltip("BlackOut이 Fade되는 Ease")] public Ease BlackOutFadeEase = Ease.InCirc;
    [Header("ProfileSlot 관련")]
    [Tooltip("프로필 슬롯이 사라지는 시간")] public float ProfileSlotTime = 0.5f;
    [Tooltip("프로필 슬롯이 갈 위치")] public Vector2 ProfileSlotEndPosition;
    [Tooltip("프로필 슬롯 Ease")] public Ease ProfileSlotEase = Ease.InCirc;
    [Tooltip("프로필 슬롯 스케일")] public float ProfileSlotScale = 0.5f;
    [Header("타이머 관련")]
    [Tooltip("타이머가 사라지는 시간")] public float TimerTime = 0.5f;
    [Tooltip("타이머가 사라지는 Ease")] public Ease TimerEase = Ease.InCirc;
    [Tooltip("타이머가 움직일 위치 : 사라질 때의 위치")] public Vector2 TimerEndPosition;
    [Header("모든 오브젝트 초기 설정")]
    [Tooltip("GameSet 글자가 떨어지기 시작할 위치")] public Vector2 GameSetStartPosition;
    [Tooltip("Profile슬롯들의 처음 위치 : 초기 53.19")] public Vector2 ProfileSlotStartPosition = new Vector2(0, 53.19f);
    [Tooltip("타이머의 원래 위치")] public Vector2 TimerOriginPosition;
    
    [Header("사운드")]
    public AudioClip GameEndBell_1;
    public AudioClip GameEndCameraOff_2;
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        EventManager.Instance.OnGameOver += Play;
    }
    // private void OnEnable()
    // {
    //     _camera = Camera.main;
    //     
    //     if (!Test)
    //     {
    //         return;
    //     }
    //     
    //     Play();
    // }

    public void Play()
    {
        SoundManager.Instance.PlayLocalSound(nameof(GameEndBell_1), transform, 0f, false, SoundType.SFX, true, 0.5f, 0.5f);
        
        GameOverProductionPanel.gameObject.SetActive(true);
        
        EventManager.Instance.OnGameOver -= Play;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(GameOverProductionPanel.DOAnchorPos(GameSetPosition, GameSetTime).SetEase(GameSetEase));
        sequence.JoinCallback(TimerOff);
        sequence.AppendInterval(GameSetWaitTime);
        sequence.Append(_camera.DOOrthoSize(CameraZoomOutAmount, CameraZoomOutTime).SetEase(CameraZoomOutEase));
        sequence.JoinCallback(CameraOn);
        sequence.AppendInterval(CameraButtonTime);
        sequence.AppendCallback(CameraButtonDown);
        sequence.AppendInterval(BlackOutTime);
        sequence.AppendCallback(CameraOff);
        sequence.AppendCallback(GameManager.Instance.GameResultCheck);
        sequence.AppendInterval(NextSceneTime);
        sequence.OnComplete(() =>
        {
            GameManager.Instance.GameStateChange(EGameState.GameOver);

            if (PhotonNetwork.IsMasterClient == false)
            {
                return;
            }
            
            PhotonNetwork.DestroyAll(); // 전부 다 지우기
            
            if (!PhotonNetwork.IsMessageQueueRunning)
                return; // 또는 로딩 상태 체크

            PhotonNetwork.LoadLevel(ESceneList.ResultScene.ToString());
        });

    }

    private void TimerOff()
    {
        DOTween.To(() => TopPivot.offsetMax, x => TopPivot.offsetMax = x, TimerEndPosition,
            TimerTime).SetEase(TimerEase);
        ProfileSlot.DOAnchorPos(ProfileSlotEndPosition,ProfileSlotTime).SetEase(ProfileSlotEase);
    }
    private void CameraOn()
    {
        CameraObject.gameObject.SetActive(true);
        CameraObject.DOScale(new Vector3(1f,1f,1f), CameraScaleTime).SetEase(CameraScaleEase);
        
    }
    
    private void CameraOff()
    {
        BlackOut.gameObject.SetActive(true);
        BlackOut.DOFade(1, BlackOutFadeTime).SetEase(BlackOutFadeEase);
    }
    
    private void CameraButtonDown()
    {
        MyAnimator.SetTrigger("Down");
        SoundManager.Instance.PlayLocalSound(nameof(GameEndCameraOff_2), transform, 0f, false, SoundType.SFX, true, 0.5f, 0.5f);

    }
    private void OnDisable()
    {
        GameOverProductionPanel.gameObject.SetActive(false);
        CameraObject.gameObject.SetActive(false);
        BlackOut.gameObject.SetActive(false);
        GameOverProductionPanel.anchoredPosition = GameSetStartPosition;
        CameraObject.localScale = new Vector3(CameraScale,CameraScale,CameraScale);
        BlackOut.color = new Color(0, 0, 0, 0);
        ProfileSlot.anchoredPosition = ProfileSlotStartPosition;
        ProfileSlot.localScale = new Vector3(1,1,1);
        TopPivot.offsetMax = TimerOriginPosition;
        DOTween.Kill(this);
    }
}