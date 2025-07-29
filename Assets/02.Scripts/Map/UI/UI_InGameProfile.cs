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
                UI_InGameProfileSlotList[i].Init(_playerActorNumberList[i].ActorNumber);
                UI_InGameProfileSlotList[i].Refresh(RoomStatManager.Instance.PlayerGunpowder, RoomStatManager.Instance.PlayerLife);
            }
            else
            {
                UI_InGameProfileSlotList[i].gameObject.SetActive(false);
            }
        }
        
        GameManager.Instance.OnProfileInit -= Init;
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
    private void Refresh(int playerNumber, int gunpowder, int life)
    {
        for (int i = 0; i < _playerActorNumberList.Count; i++)
        {
            if (_playerActorNumberList[i].ActorNumber == playerNumber)
            {
                UI_InGameProfileSlotList[i].Refresh(gunpowder,life);
            }
        }

    }
    
    private void OnDisable()
    {
        Debug.Log("온 디스에이블 ");
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnDataChanged += Refresh;
            EventManager.Instance.OnTopPlayerChanged += SetTopPlayer;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProfileInit += Init;   
        }
    }

    private void UnsubscribeEvents()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnDataChanged -= Refresh;
            EventManager.Instance.OnTopPlayerChanged -= SetTopPlayer;
        }
    }
}
