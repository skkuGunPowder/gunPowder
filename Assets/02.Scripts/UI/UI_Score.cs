using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_Score : MonoBehaviour
{
    /// <summary>
    /// 점수 변경 시 팀에 맞는 스코어 변경
    /// 시작 시 팀 체크, 팀에 따라 뒷 배경 색상 변경, Dictionary 추가
    /// </summary>
    [SerializeField] private List<UI_TextSlot> _scoreTextList;
    private Dictionary<EInGameTeam, UI_TextSlot> _teamScoreDict = new Dictionary<EInGameTeam, UI_TextSlot>();
    
    private void Awake()
    {
        EventManager.Instance.OnScoreUpdate += RefreshScore;
    }

    private void Start()
    {
        Init();
    }

    /// <summary>
    /// 현재 존재하는 팀 찾아서 그 팀만 슬롯 On
    /// </summary>
    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;

        int index = 0;
        
        //현재 존재하는 팀만 설정하도록
        foreach (Photon.Realtime.Player player in players)
        {
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];

            if (_teamScoreDict.ContainsKey(team) == false)
            {
                _teamScoreDict.Add(team, _scoreTextList[index]);
                _teamScoreDict[team].TextRefresh(0);
                _teamScoreDict[team].BackgroundRefresh(ColorSet(team));
                Debug.LogWarning($"팀 딕셔너리 : {team}");
                index++;
            }
        }
        
        // 나머지 지우기
        for(int i = index; i < _scoreTextList.Count; i++)
        {
            _scoreTextList[i].gameObject.SetActive(false);
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
    private void RefreshScore(EInGameTeam team, int score)
    {
        _teamScoreDict[team].TextRefresh(score);
    }
    
    private void OnDestroy()
    {
        EventManager.Instance.OnScoreUpdate -= RefreshScore;
    }
    
}
