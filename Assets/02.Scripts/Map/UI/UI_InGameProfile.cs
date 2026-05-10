using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfile : MonoBehaviour
{
    [SerializeField] private Image _mainBomb;
    [SerializeField] private Image _subBomb;
    
    [SerializeField]
    private List<UI_InGameProfileSlot> UI_InGameProfileSlotList = new List<UI_InGameProfileSlot>();
    private List<int> _playerActorNumberList = new List<int>();

    /// <summary>
    /// 로컬 플레이어의 프로필 슬롯 (index 0)
    /// </summary>
    public UI_InGameProfileSlot LocalPlayerSlot =>
        UI_InGameProfileSlotList != null && UI_InGameProfileSlotList.Count > 0
            ? UI_InGameProfileSlotList[0]
            : null;

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Init()
    {
        _playerActorNumberList.Clear();
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
                PhotonPlayer player = reorderedPlayers[i];
                string bombKey = EItemType.Bomb.ToString();
                string subBombKey = EItemType.SubBomb.ToString();

                string bombId = player.GetCustomProperty<string>(bombKey, "BO0001");
                string subBombId = player.GetCustomProperty<string>(subBombKey, "BO0005");
                // 플레이어의 gp
                int gp = player.GetCustomProperty<int>(EProperties.GP.ToString(), RoomStatManager.Instance.InitGunpowder);

                ItemDTO item = ItemDatabase.Instance.GetItem(bombId);
                ItemDTO sub = ItemDatabase.Instance.GetItem(subBombId);

                Sprite bomb = item.Image;
                Sprite subImage = sub.Image;

                if (reorderedPlayers[i].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    // Z슬롯 = 메인 폭탄
                    Sprite zSlotSprite = bomb;
                    // X슬롯 = SubBomb이 설정되어 있으면 그것, 없으면 메인 폭탄으로 fallback (Player.cs 슬롯 로직과 동일)
                    Sprite xSlotSprite = player.CustomProperties.ContainsKey(subBombKey) ? subImage : bomb;

                    _mainBomb.sprite = zSlotSprite;
                    _subBomb.sprite = xSlotSprite;
                }
                
                EInGameTeam team = reorderedPlayers[i].GetCustomProperty<EInGameTeam>(EProperties.Team);
                
                UI_InGameProfileSlotList[i].gameObject.SetActive(true);
                // 후에 수정
                UI_InGameProfileSlotList[i].Init(bomb, subImage, team, reorderedPlayers[i], RoomStatManager.PlayerHP,
                    RoomStatManager.Instance.PlayerLife, gp);
                _playerActorNumberList.Add(reorderedPlayers[i].ActorNumber);

                // 궁극기 게이지 바: 로컬 플레이어(index 0)만 활성화
                if (UI_InGameProfileSlotList[i].UltimateGaugeBarFill != null)
                {
                    UI_InGameProfileSlotList[i].UltimateGaugeBarFill.gameObject.SetActive(i == 0);
                }
            }
            else
            {
                UI_InGameProfileSlotList[i].gameObject.SetActive(false);    
            }
        }
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
    
    private void Refresh(int playerNumber, int hp, int life, int attacker)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].Refresh(hp,life,attacker);
            }
        }
    }

    private void RefreshGP(int playerNumber, int gp)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
            {
                UI_InGameProfileSlotList[i].RefreshGP(gp);
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

    private void RefreshCartridges(PhotonPlayer player)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == player.ActorNumber)
            {
                UI_InGameProfileSlotList[i].RefreshCartridges(player);
                break;
            }
        }
    }

    private void PlayerBombChange(PhotonPlayer changedPlayer)
    {
        string bombKey = EItemType.Bomb.ToString();
        string subBombKey = EItemType.SubBomb.ToString();

        string bombId = changedPlayer.GetCustomProperty<string>(bombKey, "BO0001");
        string subBombId = changedPlayer.GetCustomProperty<string>(subBombKey, "BO0005");

        ItemDTO item = ItemDatabase.Instance.GetItem(bombId);
        ItemDTO sub = ItemDatabase.Instance.GetItem(subBombId);

        if (changedPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // _mainBomb = Z슬롯 = 메인 폭탄
            _mainBomb.sprite = item.Image;
            // _subBomb = X슬롯 = SubBomb이 설정되어 있으면 그것, 없으면 메인 폭탄으로 fallback
            _subBomb.sprite = changedPlayer.CustomProperties.ContainsKey(subBombKey) ? sub.Image : item.Image;
        }
        
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == changedPlayer.ActorNumber)
            {
                UI_InGameProfileSlotList[i].RefreshBomb(item.Image, sub.Image);
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
            EventManager.Instance.OnGPDataChanged += RefreshGP;
            EventManager.Instance.OnTopPlayerChanged += SetTopPlayer;
            EventManager.Instance.OnProfileInit += Init;
            EventManager.Instance.OnPlayerLeft += PlayerLeftRefresh;
            EventManager.Instance.OnReadyChanged += PlayerBombChange;
            EventManager.Instance.OnCartridgesChanged += RefreshCartridges;
            Debug.Log("SubscribeEvents");       
        }
    }

    private void UnsubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnDataChanged -= Refresh;
            EventManager.Instance.OnGPDataChanged -= RefreshGP;
            EventManager.Instance.OnTopPlayerChanged -= SetTopPlayer;
            EventManager.Instance.OnPlayEmotion -= PlayEmotion;
            EventManager.Instance.OnPlayerLeft -= PlayerLeftRefresh;
            EventManager.Instance.OnProfileInit -= Init;
            EventManager.Instance.OnReadyChanged -= PlayerBombChange;
            EventManager.Instance.OnCartridgesChanged -= RefreshCartridges;
            Debug.Log("UnsubscribeEvents");
        }
    }
}
