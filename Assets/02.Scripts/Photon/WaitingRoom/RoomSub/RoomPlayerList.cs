using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPlayerList
{ 
    //리스트로 정보칸 들어가게 하기 => 플레이어 칸 정하기
    private List<int> _playerSlotList;
    public List<int> PlayerSlotList => _playerSlotList;
    
    
    public RoomPlayerList(int[] playerSlotList)
    {
        _playerSlotList = new List<int>(playerSlotList);
    }

    // 플레이어 리스트 재설정, 정렬 : 로컬 플레이어가 1번으로 들어가게 함
    public void GetPlayerList(int[] playerSlotList)
    {
        _playerSlotList.Clear();
        int[] tempList = new int[playerSlotList.Length];
        tempList[0] = PhotonNetwork.LocalPlayer.ActorNumber;
        int tempIndex = 1; // 로컬 플레이어를 0번에 들어가게 하기 위함
        foreach (var slot in playerSlotList)
        {
            if (slot == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
            
            if (slot == 0)
            {
                continue;
            }
            
            if (tempIndex < playerSlotList.Length)
            {
                Debug.Log("temp"+ tempIndex);
                tempList[tempIndex] = slot;
                tempIndex++;   
            }
        }
        _playerSlotList = tempList.ToList();
    }
    
    // 게임 시작했을 때와 게임이 끝나고 돌아온 후 플레이어 리스트를 비교후 리스트 재조정
    public void PlayerListCheck()
    {
        Debug.Log("list check");

        for(int i = 0; i < _playerSlotList.Count; i++)
        {
            if (_playerSlotList[i] == 0)
            {
                continue;
            }

            if (PhotonNetwork.CurrentRoom.Players.ContainsKey(_playerSlotList[i]))
            {
                continue;
            }
            
            Debug.Log($"{_playerSlotList[i]} : subtract");
            _playerSlotList[i] = 0;
        }
        
    }
    public void AddPlayerPlacement(PhotonPlayer player)
    {
        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            // 들어온 경우
            if (_playerSlotList[i] == 0)
            {
                _playerSlotList[i] = player.ActorNumber;
                break;
            }
        }

        SetPlayerList();
    }

    public void SubPlayerPlacement(PhotonPlayer player)
    {
        Debug.Log("sub");
        
        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            // 들어온 경우
            if (_playerSlotList[i] == player.ActorNumber)
            {
                _playerSlotList[i] = 0;
                Debug.Log($"sub : {_playerSlotList[i]}");
                break;   
            }
        }
        
        SetPlayerList();
    }
    
    private void SetPlayerList()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        Room room = PhotonNetwork.CurrentRoom;
        Hashtable currentList = new Hashtable()
        {
            { EProperties.PlayerList.ToString(), _playerSlotList.ToArray() }
        };

        room.SetCustomProperties(currentList);
    }
    
}
