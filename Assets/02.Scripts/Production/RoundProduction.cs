using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class RoundProduction : MonoBehaviour
{
    // 점수 득점 시 켜질 백그라운드 필요
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private RectTransform _textPivot;
    [SerializeField] private List<UI_RoundSlot> _textSlotList;
    [SerializeField] private List<GameObject> _playerSlotList;
    [SerializeField] private SkinSettingForUlti _skinPlayer;
    
    [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroup;
    [SerializeField] private HorizontalLayoutGroup _playerHorizontalLayoutGroup;
    private Dictionary<EInGameTeam, UI_RoundSlot> _teamScoreDict = new Dictionary<EInGameTeam, UI_RoundSlot>();

    [Header("등장 애니메이션 조절")]
    [SerializeField] private float _duration;
    [SerializeField] private float _interval;
    [SerializeField] private Ease _ease;
    
    [Header("테스트용")]
    [SerializeField] private bool _isTest = false;
    [SerializeField] private int _people = 0;
    [SerializeField] private GameObject _playerPrefab;
    
    
    private bool _initialized = false;

    private void Awake()
    {
        EventManager.Instance.OnScoreUpdate += Play;
        EventManager.Instance.OnLoadEnd += Init;
        if (_skinPlayer == null)
        {
           _skinPlayer = FindAnyObjectByType<SkinSettingForUlti>();
        }
    }
    
    private void Start()
    {
        // if (_isTest)
        // {
        //     ColorPalette.Init();
        //     TestInit();
        // }
        // else
        // {
        //     Init();
        // }
    }

    private void TestInit()
    {
        for (int i = 0; i < _textSlotList.Count; i++)
        {
            bool neg = i % 2 == 0;
            
            if (_people > i)
            {
                EInGameTeam team = (EInGameTeam)i;
                if (!_teamScoreDict.ContainsKey(team))
                {
                    _teamScoreDict.Add(team, _textSlotList[i]);
                    _teamScoreDict[team].Init(ColorSet(team), i, neg);
                }
                else
                {
                    _teamScoreDict[team].SetTeamCount(i);
                    _textSlotList[i].gameObject.SetActive(false);
                    _playerSlotList[i].SetActive(false);
                }
            }
            else
            {
                _textSlotList[i].gameObject.SetActive(false);
                _playerSlotList[i].SetActive(false);
            }
        }
    }
    // 팀 개수, 
    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        for (int i = 0; i < _textSlotList.Count; i++)
        {
            bool neg = i % 2 == 0;
            
            if (players.Length > i)
            {
                EInGameTeam team = (EInGameTeam)players[i].CustomProperties[EProperties.Team.ToString()];   
                if (!_teamScoreDict.ContainsKey(team))
                {
                    _teamScoreDict.Add(team, _textSlotList[i]);
                    _teamScoreDict[team].Init(ColorSet(team), i, neg);
                }
                else
                {
                    _teamScoreDict[team].SetTeamCount(i);
                    _textSlotList[i].gameObject.SetActive(false);
                    _playerSlotList[i].SetActive(false);
                }
                
            }
            else
            {
                _textSlotList[i].gameObject.SetActive(false);
                _playerSlotList[i].SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!_isTest) { return; }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _playerPrefab.SetActive(true);
            List<EInGameTeam> teams = new List<EInGameTeam>(_teamScoreDict.Keys);
            EInGameTeam randomTeam = teams[UnityEngine.Random.Range(0, teams.Count)];
            Play(randomTeam, 1);
        }
    }

    // 팀에 맞는 배경 색 제공
    private Color32 ColorSet(EInGameTeam team)
    {
        return ColorPalette.GetTeamColor(team);
    }

    public void Play(EInGameTeam team, int score)
    {
        
        Debug.Log($"round play");
        _backgroundImage.gameObject.SetActive(true);
        _skinPlayer.AllActive(true);
        _initialized = false;
        
        _teamScoreDict[team].ScoreChange(score);
        TaskPlay();
    }
    
    private async UniTaskVoid TaskPlay()
    {
        int index = 0;
        int count = _teamScoreDict.Count;
        foreach (var pair in _teamScoreDict)
        {
            bool isLast = (index == count - 1);

            if (isLast)
            {
                pair.Value.PlayStart(_duration, _ease, ScoreChange);
            }
            else
            {
                pair.Value.PlayStart(_duration, _ease);
            }

            index++;
            await UniTask.WaitForSeconds(_interval);
        }
    }

    private void ScoreChange()
    {
        foreach (var pair in _teamScoreDict)
        {
            pair.Value.ScorePlay(Stop);
        }
    }


    private void Stop()
    {
        // foreach (var skin in _skinPlayer.StartSkinList)
        // {
        //     skin.gameObject.SetActive(false);
        // }

        foreach (var pair in _teamScoreDict)
        {
            pair.Value.ScoreStop(RoundEnd);
        }
    }

    private void RoundEnd()
    {
        _skinPlayer.AllActive(false);
        _backgroundImage.gameObject.SetActive(false);
        
        if (_initialized)
        {
            return;
        }
        
        _initialized = true;
        
        if (PhotonNetwork.IsMasterClient)
        {
            EventManager.Instance.RoundEnd();
        }
    } 
    private void OnDestroy()
    {
        EventManager.Instance.OnScoreUpdate -= Play;
        EventManager.Instance.OnLoadEnd -= Init;
    }
}
