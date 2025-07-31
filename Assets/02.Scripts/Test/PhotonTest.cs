using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonTest : MonoBehaviourPunCallbacks
{
    public static PhotonTest Instance;
    
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
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Test");
    }
    
}
