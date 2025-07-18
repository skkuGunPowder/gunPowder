using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_RoomSlot : MonoBehaviour
{
    public TextMeshProUGUI RoomName;
    public TextMeshProUGUI PlayerCount;
    public Image MapIcon;

    public void Refresh(string roomName, int currentPlayerCount, int maxPlayerCount, Sprite mapIcon)
    {
        Debug.Log($"Refresh : {mapIcon.name}");
        RoomName.text = roomName;
        PlayerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
        MapIcon.sprite = mapIcon;
    }
    

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomName.text);
    }
}
