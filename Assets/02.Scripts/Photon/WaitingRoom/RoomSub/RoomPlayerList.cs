using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;
public class RoomPlayerList
{ 
    //리스트로 정보칸 들어가게 하기 => 플레이어 칸 정하기
    private List<int> _playerSlotList;
    public List<int> PlayerSlotList => _playerSlotList;
    
    
    public RoomPlayerList(int[] playerSlotList)
    {
        _playerSlotList = new List<int>(playerSlotList);
    }

    // 플레이어 리스트 재설정
    public void GetPlayerList(int[] playerSlotList)
    {
        _playerSlotList.Clear();
        _playerSlotList = new List<int>(playerSlotList);
    }
    // 게임 시작했을 때와 게임이 끝나고 돌아온 후 플레이어 리스트를 비교후 리스트 재조정
    public void PlayerListCheck()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            if (_playerSlotList.Contains(player.ActorNumber))
            {
                return;
            }
            
            SubPlayerPlacement(player);
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
    }
}
