using System;
using Photon.Pun;
using UnityEngine;

public class PhotonServerManager : MonoBehaviourPunCallbacks
{   
    
    // 게임이 시작 될 때 연결되는 포톤 서버 매니저
    
   [Header("DataFrameRate")]
   [SerializeField] private int _sendRate = 30;
   [SerializeField] private int _serializationRate = 30;
   
   [Header("GameVersion")]
   [SerializeField] private string _gameVersion = "1.0.0";
    private void Start()
    {
        // 데이터 송수신 빈도
        PhotonNetwork.SendRate = _sendRate;
        PhotonNetwork.SerializationRate = _serializationRate;
        
        // 게임 버전 설정
        PhotonNetwork.GameVersion = _gameVersion;
        
        // 서버에 연결하기 
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

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }
}
