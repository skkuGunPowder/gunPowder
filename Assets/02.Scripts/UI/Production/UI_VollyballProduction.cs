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
    [SerializeField] private UI_TextSlot _textSlot;
    [SerializeField] private UI_TextSlot _textSlot2;
    
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
        _backgroundImage.gameObject.SetActive(false);
    }
    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        int index = 0;
        
        //현재 존재하는 팀만 설정하도록
        foreach (Photon.Realtime.Player player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];

            if (_teamScoreDict.ContainsKey(team))
            {
                return;
            }

            if (index == 0)
            {
                _teamScoreDict.Add(team,_textSlot);
                _teamScoreDict[team].TextRefresh(0);
                index++; 
            }
            else
            { 
                _teamScoreDict.Add(team,_textSlot2);
                _teamScoreDict[team].TextRefresh(0);
            }
            
        }
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
        sequence.Append(_textPivot.DOScale(1, _scaleTime).SetEase(_ease));
        sequence.AppendInterval(_intervalTime);
        sequence.Append(rectTransform.DOShakeAnchorPos(_shakeDuration, _shakeStrength, _vibrato,90f, false,true,ShakeRandomnessMode.Harmonic));
        sequence.AppendCallback(()=>
        {
            _teamScoreDict[team].TextRefresh(score);
        });
        sequence.AppendInterval(_fadeTime);
        sequence.Append(_textPivot.DOScale(0, _scaleTime).SetEase(_ease));
        sequence.OnComplete(()=>
        {
            _backgroundImage.gameObject.SetActive(false);
            EventManager.Instance.GameRespawn();
        });
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnScoreUpdate -= Play;
    }
    
}
