using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    private Room _room;
    public int MaxPlayers => _room.MaxPlayers;    // 이 방에 최대 인원
    public int CurrentPlayer; // 이 방에 현재 인원

    // 현재 준비 완료의 갯수
    public int AgreeCount = 0;// 현재 시작을 동의한 사람들

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
    // 방 세팅 시작
    private void Start()
    {
        SetRoom();
    }
    // 준비가 다 되었다면 마스터가 정한 맵으로 이동시킴
    
    // 현재 이 방의 최고 인원은 몇인가?

    //현재 이 방에 있는 플레이어들의 계정 정보
    private void SetRoom()
    {
        _room = PhotonNetwork.CurrentRoom;
    }
}
