using System;
using TMPro;
using UnityEngine;

public class UI_CreateRoom : MonoBehaviour
{
    public TMP_InputField RoomName;
    public TMP_Dropdown MaxPlayers;
    // 방장 이름
    private string _master;

    public void OnclickCreateRoom()
    {
        string roomName = RoomName.text;
        int maxPlayers = int.Parse(MaxPlayers.options[MaxPlayers.value].text);
        
        Debug.Log(maxPlayers);
        
        LobbyManager.Instance.MakeRoom(roomName, maxPlayers);
    }
    
    public void Cancel()
    {
        gameObject.SetActive(false);    
    }
    
    // UI 초기화하기
    public void OnDisable()
    {
        RoomName.text = "";
        MaxPlayers.value = 0;
    }
}
