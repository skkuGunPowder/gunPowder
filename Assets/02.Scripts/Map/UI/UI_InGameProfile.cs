using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfile : MonoBehaviour
{
    public Image MyBomb;
    [SerializeField]
    private List<UI_InGameProfileSlot> UI_InGameProfileSlotList = new List<UI_InGameProfileSlot>();
    private List<int> _playerActorNumberList = new List<int>();

    private void Awake()
    {
        ItemDTO item = ItemDatabase.Instance.GetItem(PhotonNetwork.LocalPlayer.CustomProperties[EItemType.Bomb.ToString()].ToString());
        Sprite bomb = item.Image;

        MyBomb.sprite = bomb;
 
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Init()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        PhotonPlayer[] reorderedPlayers = new PhotonPlayer[players.Length];
        
        // 로컬 플레이어를 첫 번째 위치로 배치하고, 나머지 플레이어들을 이후 위치에 배치
        int localPlayerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int indexForOthers = 1; // 다른 플레이어들을 위한 인덱스
    
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].ActorNumber == localPlayerActorNumber)
            {
                reorderedPlayers[0] = players[i]; // 로컬 플레이어는 첫 번째 위치에 배치
            }
            else
            {
                if (indexForOthers > players.Length)
                {
                    return;
                }

                reorderedPlayers[indexForOthers] = players[i]; 
                indexForOthers++;
            }
        }
        
        for (int i = 0; i < UI_InGameProfileSlotList.Count; i++)
        {
            if (i < reorderedPlayers.Length)
            {
                ItemDTO item = ItemDatabase.Instance.GetItem(reorderedPlayers[i].CustomProperties[EItemType.Bomb.ToString()].ToString());
                Sprite bomb = item.Image;
                EInGameTeam team = (EInGameTeam)reorderedPlayers[i].CustomProperties[EProperties.Team.ToString()];
                
                UI_InGameProfileSlotList[i].gameObject.SetActive(true);
                // 후에 수정
                UI_InGameProfileSlotList[i].Init(bomb, team, reorderedPlayers[i],RoomStatManager.Instance.PlayerGunpowder, RoomStatManager.Instance.PlayerLife);
                _playerActorNumberList.Add(reorderedPlayers[i].ActorNumber);
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
        for(int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
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
            if (_playerActorNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].Refresh(gunpowder,life,attacker);
            }
        }

    }

    public void PlayEmotion(string emotionName, int playerNumber)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].PlayEmotion(emotionName);
            }
        }
    }

    private void PlayerLeftRefresh(PhotonPlayer leftPlayer)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == leftPlayer.ActorNumber)
            {
                UI_InGameProfileSlotList[i].LeftOverRefresh();
                _playerActorNumberList[i] = 0;
                break;
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
            EventManager.Instance.OnPlayerLeft += PlayerLeftRefresh;
            Debug.Log("SubscribeEvents");       
        }
    }

    private void UnsubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnDataChanged -= Refresh;
            EventManager.Instance.OnTopPlayerChanged -= SetTopPlayer;
            EventManager.Instance.OnPlayEmotion -= PlayEmotion;
            EventManager.Instance.OnPlayerLeft -= PlayerLeftRefresh;
            Debug.Log("UnsubscribeEvents");
        }
    }
}
