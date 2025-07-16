using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    private Room _room;
    public int MaxPlayers => _room.MaxPlayers;    // 이 방에 최대 인원
    public int CurrentPlayer; // 이 방에 현재 인원
    
    // 우리는 어떤 맵으로 가야하는 가?
    public ESceneList SelectedMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // 플레이어가 체크했는지 알아보는 커스텀 프로퍼티
    private void SetReady()
    {
        Hashtable ready = new Hashtable{{"isReady", false}};
        PhotonNetwork.LocalPlayer.SetCustomProperties(ready);
        Debug.Log(ready["isReady"]);
    }
    
    // 방 세팅 시작
    private void Start()
    {
        SetRoom();
        SetReady();
    }
    
    // 준비가 다 되었다면 마스터가 정한 맵으로 이동시킴
    // 확인이 필요한 것 : 1. 방장인가?
    //                  2. 모두 준비가 되었는 가? 
    public void GameStart()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }

        if (IsPlayerReady() == false)
        {
            return;
        }
        
        PhotonNetwork.LoadLevel(SelectedMap.ToString());
    }
    // 사람들이 모두 눌렀는가?
    public bool IsPlayerReady()
    {
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        
        foreach (PhotonPlayer player in players)
        {
            Debug.Log(player.NickName + $"{player.CustomProperties.ContainsKey("isReady")}");
            if (player.IsMasterClient)
            {
                continue;
            }
            if (player.CustomProperties.ContainsKey("isReady") == false)
            {
                return false;
            }
        }
        
        return true;
    }
    // 현재 이 방의 최고 인원은 몇인가?

    //현재 이 방에 있는 플레이어들의 계정 정보
    private void SetRoom()
    {
        _room = PhotonNetwork.CurrentRoom;
    }
    
    
    
}
