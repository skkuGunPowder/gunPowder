using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class UI_CartridgeProfile : MonoBehaviour
{
    [SerializeField] private List<UI_CartridgeProfileSlot> _profileSlotList = new List<UI_CartridgeProfileSlot>();
    private List<int> _playerActorNumberList = new List<int>();
    private PhotonPlayer _currentTurnPlayer;
    

    private void OnEnable()
    {
        SubscribeEvents();
        RefreshAllGP();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _playerActorNumberList.Clear();
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        PhotonPlayer[] reorderedPlayers = new PhotonPlayer[players.Length];

        int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        int indexForOthers = 1;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].ActorNumber == localActorNumber)
            {
                reorderedPlayers[0] = players[i];
            }
            else
            {
                if (indexForOthers >= players.Length)
                {
                    return;
                }
                reorderedPlayers[indexForOthers] = players[i];
                indexForOthers++;
            }
        }

        for (int i = 0; i < _profileSlotList.Count; i++)
        {
            if (i < reorderedPlayers.Length && reorderedPlayers[i] != null)
            {
                PhotonPlayer player = reorderedPlayers[i];

                string bombId = player.GetCustomProperty<string>(EItemType.Bomb.ToString(), "BO0001");
                string subBombId = player.GetCustomProperty<string>(EItemType.SubBomb.ToString(), "BO0005");
                int gp = player.GetCustomProperty<int>(EProperties.GP.ToString(), RoomStatManager.Instance.InitGunpowder);

                ItemDTO item = ItemDatabase.Instance.GetItem(bombId);
                ItemDTO sub = ItemDatabase.Instance.GetItem(subBombId);
                EInGameTeam team = player.GetCustomProperty<EInGameTeam>(EProperties.Team);

                _profileSlotList[i].gameObject.SetActive(true);
                _profileSlotList[i].Init(item.Image, sub.Image, team, player, RoomStatManager.PlayerHP, RoomStatManager.Instance.PlayerLife, gp);
                _playerActorNumberList.Add(player.ActorNumber);
            }
            else
            {
                _profileSlotList[i].gameObject.SetActive(false);
            }
        }

        if (_currentTurnPlayer != null)
        {
            SetCurrentSlot(_currentTurnPlayer);
        }
    }

    private void RefreshAllGP()
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            PhotonPlayer player = PhotonNetwork.CurrentRoom?.GetPlayer(_playerActorNumberList[i]);
            if (player == null)
            {
                _profileSlotList[i].gameObject.SetActive(false);
                continue;
            }
            int gp = player.GetCustomProperty<int>(EProperties.GP.ToString(), RoomStatManager.Instance.InitGunpowder);
            _profileSlotList[i].RefreshGP(gp);
        }
    }

    private void GPDataChange(int playerNumber, int gp)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i] == playerNumber)
            {
                _profileSlotList[i].RefreshGP(gp);
                break;
            }
        }
    }

    public void SetCurrentSlot(PhotonPlayer player)
    {
        _currentTurnPlayer = player;
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            _profileSlotList[i].SetMyTurn(_playerActorNumberList[i] == player.ActorNumber);
        }
    }

    private void SubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGPDataChanged += GPDataChange;
            EventManager.Instance.OnCartridgeStart += SetCurrentSlot;
        }
    }

    private void UnsubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGPDataChanged -= GPDataChange;
            EventManager.Instance.OnCartridgeStart -= SetCurrentSlot;
        }
    }
}
