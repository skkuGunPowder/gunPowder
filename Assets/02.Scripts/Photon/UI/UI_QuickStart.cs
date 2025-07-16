using Photon.Pun;
using UnityEngine;
public class UI_QuickStart : MonoBehaviour
{
    // 개인으로 만들어진 방 랜덤으로 들어가기
    public void OnClickQuickJoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    // 팀으로 만들어진 방 중 랜덤으로 들어가기
    public void OnClickPartyLoadScene()
    {
        // LobbyManager의 party라는 변수가 null이 아니라면 이 함수를 호출한다.
        // 
    }
}
