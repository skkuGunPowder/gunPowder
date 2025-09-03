using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfile : MonoBehaviour
{ 
    [SerializeField]
    private List<UI_InGameProfileSlot> UI_InGameProfileSlotList = new List<UI_InGameProfileSlot>();
    private List<PhotonPlayer> _playerActorNumberList = new List<PhotonPlayer>();
    
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Init()
    {
        _playerActorNumberList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);

        for (int i = 0; i < UI_InGameProfileSlotList.Count; i++)
        {
            if (i < _playerActorNumberList.Count)
            {
                // 후에 수정
                ItemDTO item = ItemDatabase.Instance.GetItem(_playerActorNumberList[i].CustomProperties[EItemType.Bomb.ToString()].ToString());
                Sprite bomb = item.Image;
                string playerName = _playerActorNumberList[i].NickName;
                EInGameTeam team = (EInGameTeam)_playerActorNumberList[i].CustomProperties[EProperties.Team.ToString()];
                UI_InGameProfileSlotList[i].Init(playerName,bomb, team, _playerActorNumberList[i]);
                UI_InGameProfileSlotList[i].Refresh(RoomStatManager.Instance.PlayerGunpowder, RoomStatManager.Instance.PlayerLife , 0);
            }
            else
            {
                UI_InGameProfileSlotList[i].gameObject.SetActive(false);
            }
        }
        
        EventManager.Instance.OnProfileInit -= Init;
    }
    
    private void SetTopPlayer(int playerNumber)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i].ActorNumber == playerNumber)
            {
                UI_InGameProfileSlotList[i].SetTop(true);
            }
            else
            {
                UI_InGameProfileSlotList[i].SetTop(false);
            }
        }
    }
    
    private void Refresh(int playerNumber, int gunpowder, int life, int attacker)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i].ActorNumber == playerNumber)
            {
                UI_InGameProfileSlotList[i].Refresh(gunpowder,life,attacker);
            }
        }

    }

    public void PlayEmotion(string emotionName, int playerNumber)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i].ActorNumber == playerNumber)
            {
                UI_InGameProfileSlotList[i].PlayEmotion(emotionName);
            }
        }
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayEmotion += PlayEmotion;
            EventManager.Instance.OnDataChanged += Refresh;
            EventManager.Instance.OnTopPlayerChanged += SetTopPlayer;
            EventManager.Instance.OnProfileInit += Init;   
        }
    }

    private void UnsubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnDataChanged -= Refresh;
            EventManager.Instance.OnTopPlayerChanged -= SetTopPlayer;
            EventManager.Instance.OnPlayEmotion -= PlayEmotion;
        }
    }
}
