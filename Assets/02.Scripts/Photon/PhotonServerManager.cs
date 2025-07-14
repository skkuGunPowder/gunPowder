using System;
using System.Security.Cryptography.X509Certificates;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonServerManager : MonoBehaviourPunCallbacks
{   
    public static PhotonServerManager Instance;
    
    // 게임이 시작 될 때 연결되는 포톤 서버 매니저
    
   [Header("DataFrameRate")]
   [SerializeField] private int _sendRate = 30;
   [SerializeField] private int _serializationRate = 30;
   
   [Header("GameVersion")]
   [SerializeField] private string _gameVersion = "1.0.0";
   [SerializeField] private string _nickName = "Lets Go Home";
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
            
        // 서버에 연결한다.
        Connect();
        
        
    }
    
    // 서버를 연결하겠다.
    public void Connect()
    {
        // 게임 버전 설정
        PhotonNetwork.GameVersion = _gameVersion;
        PhotonNetwork.NickName = _nickName;
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
    }
    
    //로비에 접속 ( when? 로그인을 성공했을 때)
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        SceneManager.LoadScene(ESceneList.Lobby.ToString());

    }

    // 방에 접속
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.LoadLevel(ESceneList.WaitingRoom.ToString());
    }
}
