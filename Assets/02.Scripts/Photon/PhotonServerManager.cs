using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonPlayer = Photon.Realtime.Player;

public class PhotonServerManager : MonoBehaviourPunCallbacks
{
    public static PhotonServerManager Instance;
    // 게임이 시작 될 때 연결되는 포톤 서버 매니저

    [Header("DataFrameRate")]
    [SerializeField] private int _sendRate = 30;
    [SerializeField] private int _serializationRate = 30;

    [Header("GameVersion")]
    [SerializeField] private string _gameVersion = "1.0.0";
    
    private List<RoomInfo> _roomInfoList = new List<RoomInfo>();
    public List<RoomInfo> RoomInfoList => _roomInfoList;

    
    private bool _isTutorial = false;
    private bool _isFirst = false;
    public bool IsFirst => _isFirst;
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
    public void Connect(bool first = false)
    {
        // 게임 버전 설정
        PhotonNetwork.GameVersion = _gameVersion;
        PhotonNetwork.NickName = AccountManager.Instance.CurrencAccount.Nickname;
        PhotonNetwork.ConnectUsingSettings();
        
        if (first)
        { 
            _isFirst = true;   
        }
    }

    public void TutorialMode()
    {
        _isTutorial = true;
        
        Hashtable roomProperties = new Hashtable
        {
            {ERoomProperties.PlayTime.ToString(), 100},
            {ERoomProperties.Life.ToString(), 3},
            {ERoomProperties.Gunpowder.ToString(), 100},
            {ERoomProperties.DeclinePowder.ToString(), 1},
        };
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 1;
        roomOptions.CustomRoomProperties = roomProperties; 
        
        string room = PhotonNetwork.LocalPlayer.UserId + " " + "Tutorial";
        PhotonNetwork.CreateRoom(room, roomOptions, TypedLobby.Default);
    }
    
    // 포톤 마스터 서버에 접속하면 호출되는 함수
    public override void OnConnected()
    {
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError("이제 연결 끊기면 포톤 씬으로 보냅니다.");
        SceneManager.LoadScene(ESceneList.Photon.ToString());
    }

    //마스터 서버에 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        _roomInfoList.Clear();
        _isTutorial = false;         
        
        PhotonNetwork.LoadLevel(ESceneList.Lobby.ToString());
        Hashtable propertiesToRemove = new Hashtable
        {
            { EProperties.Team.ToString(), null },
            { EProperties.RoomInitial.ToString(), null}
        };
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToRemove);
        
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("OnJoinRandomFailed");
    }

    public override void OnJoinedRoom()
    {
        if (_isTutorial)
        {
            PhotonNetwork.LoadLevel(ESceneList.Tutorial.ToString());
        }
    }

    public override void OnCreatedRoom()
    {
        if (!_isTutorial)
        {
            PopupManager.Instance.Close(EPopupType.UI_RoomSearchPopup);
            PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());   
        }
    }

    public void SetPhotonPrefabPool(Dictionary<string, Item> itemDict)
    {
        DefaultPool pool = (DefaultPool)PhotonNetwork.PrefabPool;
        foreach (var kvp in itemDict)
        {
            pool.ResourceCache.TryAdd(kvp.Value.Prefab.name, kvp.Value.Prefab);
        }
    }

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
    {
     
        EventManager.Instance.PlayerLeft(otherPlayer);
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        EventManager.Instance.PlayerLeftRoom(otherPlayer);   
        EventManager.Instance.PlayerFind();
    }

    public void SetFirst(bool isFirst)
    {
        _isFirst = isFirst;
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (PhotonNetwork.InLobby == false)
        {
            return;
        }
        
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList)
            {
                _roomInfoList.RemoveAll(x => x.Name == roomInfo.Name);
            }
            else
            {
                int index = _roomInfoList.FindIndex(x => x.Name == roomInfo.Name);
                if (index >= 0)
                {
                    _roomInfoList[index] = roomInfo;
                }
                else
                {
                    _roomInfoList.Add(roomInfo);
                }
            }
            
        }
        EventManager.Instance.RoomListUpdate();
    }

}
