using System;
using TMPro;
using UnityEngine;

public class UI_RoomSlot : MonoBehaviour
{
    public TextMeshProUGUI RoomName;
    public TextMeshProUGUI PlayerCount;

    public void Refresh(string roomName, int currentPlayerCount, int maxPlayerCount)
    {
        RoomName.text = roomName;
        PlayerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
    }
}
