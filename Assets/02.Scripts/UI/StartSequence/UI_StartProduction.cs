using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

using PhotonPlayer = Photon.Realtime.Player;
public class UI_StartProduction : MonoBehaviour
{
    public LoadSceneChecker LoadChecker;
    // 플레이어 리스트 들고 있어야함.
    private List<PhotonPlayer> _playerList = new List<PhotonPlayer>();
    // 플레이어 슬롯을 들고 있어야함.
    public List<UI_ProductionSlot> ProductionSlotList = new List<UI_ProductionSlot>();
    // 연출을 위한 게임오브젝트들 필요
    
    private void OnEnable()
    {
        LoadChecker.OnLoading += LoadCheck;
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        _playerList = new List<PhotonPlayer>(players);
        
        Init();
    }
    
    private void Init()
    {
        for (int i = 0; i < _playerList.Count; i++)
        {
            PhotonPlayer player = _playerList[i];
            
            //정보 불러오기
            EInGameTeam team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];
            ItemDTO item = ItemDatabase.Instance.GetItem(player.CustomProperties[EItemType.Bomb.ToString()].ToString());
            Sprite sprite = item.Image;
            
            ProductionSlotList[i].gameObject.SetActive(true);
            ProductionSlotList[i].Init(player.NickName, team, sprite );
        }
    }

    private void LoadCheck(int playerNumber, bool isLoad)
    {
        for (int i = 0; i < ProductionSlotList.Count; i++)
        {
            if (_playerList[i].ActorNumber == playerNumber)
            {
                ProductionSlotList[i].LoadCheck(isLoad);
            }
        }
    }
    private void OnDisable()
    {
        _playerList.Clear();
        LoadChecker.OnLoading -= LoadCheck;
    }
}
