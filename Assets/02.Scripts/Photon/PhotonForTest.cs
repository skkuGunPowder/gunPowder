// using Photon.Pun;
// using UnityEngine;
// using Photon.Realtime;
// public class PhotonForTest : MonoBehaviourPunCallbacks
// {    
//     public static PhotonServerManager Instance;
//
//     // 게임이 시작 될 때 연결되는 포톤 서버 매니저
//
//     public PlayerSpawner Spawner;
//     [Header("DataFrameRate")]
//     [SerializeField] private int _sendRate = 30;
//     [SerializeField] private int _serializationRate = 30;
//    
//     [Header("GameVersion")]
//     [SerializeField] private string _gameVersion = "1.0.0";
//     [SerializeField] private string _nickName = "Lets Go Home";
//
//     public string RoomName; 
//
//     private void Start()
//     {
//         // 데이터 송수신 빈도
//         PhotonNetwork.SendRate = _sendRate;
//         PhotonNetwork.SerializationRate = _serializationRate;
//
//         PhotonNetwork.AutomaticallySyncScene = true;
//
//         Connect();
//     }
//     
//     // 서버를 연결하겠다.
//     public void Connect()
//     {
//         // 게임 버전 설정
//         PhotonNetwork.GameVersion = _gameVersion;
//         PhotonNetwork.NickName = _nickName;
//         PhotonNetwork.ConnectUsingSettings();
//     }
//     
//     // 포톤 마스터 서버에 접속하면 호출되는 함수
//     public override void OnConnected()
//     {
//         Debug.Log("OnConnected");
//     }
//
//     //마스터 서버에 접속
//     public override void OnConnectedToMaster()
//     {
//         Debug.Log("OnConnectedToMaster");
//         PhotonNetwork.JoinLobby(TypedLobby.Default);
//     }
//
//     public override void OnJoinedLobby()
//     {
//         Debug.Log("OnJoinedLobby");
//         RoomOptions roomOptions = new RoomOptions();
//         roomOptions.MaxPlayers = 1;
//         roomOptions.IsVisible = true;
//         roomOptions.IsOpen = true;
//         PhotonNetwork.CreateRoom(RoomName, roomOptions, TypedLobby.Default);
//     }
//
//     public override void OnJoinedRoom()
//     {
//         Debug.Log("OnJoinedRoom");
//         Spawner.GeneratePlayers(0);
//     }
//     public override void OnJoinRandomFailed(short returnCode, string message)
//     {
//     }
//
//     public override void OnCreatedRoom()
//     {
//     }
// }
//
