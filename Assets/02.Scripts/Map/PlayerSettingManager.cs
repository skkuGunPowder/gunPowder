using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

[RequireComponent(typeof(PhotonView))]
public class PlayerSettingManager : MonoBehaviour
{
    private List<int> _playerList = new List<int>(); //현재 있는 플레이어들
    public List<int> PlayerList => _playerList;
    
    private PhotonView _photonView;
    
    public PlayerSpawner Spawner;
    public LoadSceneChecker LoadSceneChecker;
    
    // 현재 룸 프로퍼티 가져오기 => 플레이어 세팅해주기
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        LoadSceneChecker.OnLoadFinished += Init;
    }

    public void Init()
    {
        Hashtable dead = new Hashtable()
        {
            { EProperties.IsDead.ToString(), false }
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(dead);
        // 캐릭터 순번 세팅
        SpawnSetting();

        // 플레이어 스탯 추가해주기   
    }
    
    [PunRPC]
    private void Rpc_SpawnPlayer(int[] playerList)
    {
        _playerList = new List<int>(playerList);
        SpawnPlayer();
    }
    
    // 프로퍼티 불러오기  => 플레이어 리스트
    private void SpawnSetting()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        
        if (PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] != null)
        {
            int[] players = PhotonNetwork.CurrentRoom.CustomProperties[EProperties.PlayerList.ToString()] as int[];   
            List<int> currentPlayerList = new List<int>(players);
            
            foreach (int actorNumber in currentPlayerList)
            {
                if (actorNumber == 0)
                {
                    continue;
                }

                int score = RoomStatManager.Instance.PlayerLife * RoomStatManager.Instance.PlayerGunpowder;
                _playerList.Add(actorNumber);
            }
        }
        else
        {
            List<PhotonPlayer> currentPlayerList = new List<PhotonPlayer>(PhotonNetwork.PlayerList);
            foreach (PhotonPlayer player in currentPlayerList)
            {
                _playerList.Add(player.ActorNumber);
            }
        }


        _photonView.RPC(nameof(Rpc_SpawnPlayer), RpcTarget.All, _playerList.ToArray());
    }
    
    private void SpawnPlayer()
    {
        for (int i = 0; i < _playerList.Count; i++)
        {
            Debug.Log(_playerList[i]);
            Debug.Log( PhotonNetwork.LocalPlayer.ActorNumber);
            if (_playerList[i] != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                continue;
            }
        
            Debug.Log($"{PhotonNetwork.LocalPlayer.ActorNumber} 가 소환한당");    
            Spawner.GeneratePlayers(i);
        }
    }

    
}
