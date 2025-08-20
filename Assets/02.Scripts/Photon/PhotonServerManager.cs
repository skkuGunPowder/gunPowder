using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

public class PhotonServerManager : MonoBehaviourPunCallbacks
{
    public static PhotonServerManager Instance;

    public List<RoomInfo> CachedRoomList { get; set; } = new List<RoomInfo>();
    // 게임이 시작 될 때 연결되는 포톤 서버 매니저

    [Header("DataFrameRate")]
    [SerializeField] private int _sendRate = 30;
    [SerializeField] private int _serializationRate = 30;

    [Header("GameVersion")]
    [SerializeField] private string _gameVersion = "1.0.0";
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    private void Start()
    {
        // 데이터 송수신 빈도
        PhotonNetwork.SendRate = _sendRate;
        PhotonNetwork.SerializationRate = _serializationRate;

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // 서버를 연결하겠다.
    public void Connect()
    {
        // 게임 버전 설정
        PhotonNetwork.GameVersion = _gameVersion;
        PhotonNetwork.NickName = AccountManager.Instance.CurrencAccount.Nickname;
        Debug.Log( PhotonNetwork.NickName);
        PhotonNetwork.ConnectUsingSettings();
    }

    // 포톤 마스터 서버에 접속하면 호출되는 함수
    public override void OnConnected()
    {
        Debug.Log("OnConnected");
    }

    //마스터 서버에 접속
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        Hashtable propertiesToRemove = new Hashtable
        {
            { EProperties.Team.ToString(), null }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToRemove);

        PhotonNetwork.LoadLevel(ESceneList.Lobby.ToString());
        

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());
    }

    public void SetPhotonPrefabPool(Dictionary<string, Item> itemDict)
    {
        DefaultPool pool = (DefaultPool)PhotonNetwork.PrefabPool;
        foreach (var kvp in itemDict)
        {
            pool.ResourceCache.TryAdd(kvp.Value.Prefab.name, kvp.Value.Prefab);
        }
        Debug.Log("포톤 풀 등록 완료");
    }

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
    {
        EventManager.Instance.PlayerFind();
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        Debug.Log($"{otherPlayer.ActorNumber} 플레이어 나감요");
        EventManager.Instance.PlayerLeftRoom(otherPlayer);
    }
}
