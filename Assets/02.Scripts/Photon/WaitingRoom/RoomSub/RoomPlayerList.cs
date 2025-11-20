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
    
        // 1. 로컬 플레이어를 먼저 추가
        _playerSlotList.Add(PhotonNetwork.LocalPlayer.ActorNumber);
    
        // 2. 나머지 플레이어들을 추가 (로컬 플레이어 제외)
        for (int i = 0; i < playerSlotList.Length; i++)
        {
            // 로컬 플레이어는 이미 추가했으므로 스킵
            if (playerSlotList[i] == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
        
            // 0이 아닌 값만 추가
            if (playerSlotList[i] != 0)
            {
                _playerSlotList.Add(playerSlotList[i]);
            }
        }
    
        // 3. 4칸을 채우기 위해 부족한 만큼 0으로 채움
        while (_playerSlotList.Count < 4)
        {
            _playerSlotList.Add(0);
        }

    }
    
    // 게임 시작했을 때와 게임이 끝나고 돌아온 후 플레이어 리스트를 비교후 리스트 재조정
    public void PlayerListCheck()
    {
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
        
        for (int i = 0; i < _playerSlotList.Count; i++)
        {
            // 들어온 경우
            if (_playerSlotList[i] == player.ActorNumber)
            {
                _playerSlotList[i] = 0;
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
