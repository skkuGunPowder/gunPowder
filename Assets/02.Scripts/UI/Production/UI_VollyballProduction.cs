using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_VollyballProduction : MonoBehaviour
{
    // 2팀 : 2개 슬롯 필요
    // 점수 득점 시 켜질 백그라운드 필요
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private RectTransform _textPivot;
    [SerializeField] private List<UI_TextSlot> _textSlotList;
    
    [Header("연출에 필요한 정보")]
    [SerializeField] private float _scaleTime = 0.5f;
    [SerializeField] private float _intervalTime = 0.5f;
    [SerializeField] private float _fadeTime = 1f;
    [SerializeField] private float _shakeDuration = 0.1f;
    [SerializeField] private float _shakeStrength = 0.1f;
    [SerializeField] private int _vibrato = 110;
    [SerializeField] private Ease _ease = Ease.OutSine;
    
    private Dictionary<EInGameTeam, UI_TextSlot> _teamScoreDict = new Dictionary<EInGameTeam, UI_TextSlot>();
    
    private void Awake()
    {
        EventManager.Instance.OnScoreUpdate += Play;
    }

    private void Start()
    {
        Init();
    }
    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < _textSlotList.Count; i++)
        {
            if (players.Length > i)
            {
                EInGameTeam team = (EInGameTeam)players[i].CustomProperties[EProperties.Team.ToString()];   
                _teamScoreDict.Add(team, _textSlotList[i]);
                _teamScoreDict[team].BackgroundRefresh(ColorSet(team));
                _teamScoreDict[team].TextRefresh(0);
            }
            else
            {
                _textSlotList[i].gameObject.SetActive(false);
            }
        }
        
        
        _backgroundImage.gameObject.SetActive(false);
    }
    
    private Color32 ColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red : return ColorPalette.ColorDictionary[EColorType.Red];
            case EInGameTeam.Blue : return ColorPalette.ColorDictionary[EColorType.Blue];
            case EInGameTeam.Green : return ColorPalette.ColorDictionary[EColorType.Green];
            case EInGameTeam.Yellow : return ColorPalette.ColorDictionary[EColorType.Yellow];
       
            default : return Color.white;
        }
    }
    private void Play(EInGameTeam team, int score)
    {
        RectTransform rectTransform = _teamScoreDict[team].GetComponent<RectTransform>();
        
        _backgroundImage.gameObject.SetActive(true);
        
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(_intervalTime);
        sequence.Append(rectTransform.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _vibrato,90f, false,true,ShakeRandomnessMode.Harmonic));
        sequence.AppendCallback(()=>
        {
            _teamScoreDict[team].TextRefresh(score);
        });
        sequence.AppendInterval(_fadeTime);
        sequence.OnComplete(()=>
        {
            _backgroundImage.gameObject.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnScoreUpdate -= Play;
    }
    
}
