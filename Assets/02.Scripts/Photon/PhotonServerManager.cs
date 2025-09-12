using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
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
    
    private bool _isTutorial = false;
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
            TutorialMode(true);
        }
    }

    public void TutorialMode()
    {
        Debug.Log("TutorialMode");
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
        
        PhotonNetwork.CreateRoom("tutorial", roomOptions, TypedLobby.Default);
    }

    public void TutorialMode(bool isTutorial)
    {
        _isTutorial = isTutorial;
    }

    // 포톤 마스터 서버에 접속하면 호출되는 함수
    public override void OnConnected()
    {
    }

    //마스터 서버에 접속
    public override void OnConnectedToMaster()
    {
        if (_isTutorial)
        {
            TutorialMode();
        }
        
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Hashtable propertiesToRemove = new Hashtable
        {
            { EProperties.Team.ToString(), null }
        };
        
        _isTutorial = false;         
        PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToRemove);

        PhotonNetwork.LoadLevel(ESceneList.Lobby.ToString());
        

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("OnJoinRandomFailed");
    }

    public override void OnJoinedRoom()
    {
        if (_isTutorial)
        {
            Debug.Log("tutoOnJoinedRoom");
            PhotonNetwork.LoadLevel(ESceneList.Tutorial.ToString());
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        
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
        EventManager.Instance.PlayerFind();
        
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        EventManager.Instance.PlayerLeftRoom(otherPlayer);
    }
}
