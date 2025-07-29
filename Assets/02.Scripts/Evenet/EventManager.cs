using System;
using UnityEngine;

public class EventManager : DontDestroySingleton<EventManager> // Start is called once before the first execution of Update after the MonoBehaviour is created
{
    public event Action<int> OnTopPlayerChanged;        // 순위 변경용  = 1등 체크용
    public void SetTopPlayer(int topActor)
    {
        OnTopPlayerChanged?.Invoke(topActor);
    }
    public event Action<int,int,int> OnDataChanged;    // 체력 감소할 때
    public void PlayerDataChange(int gunpowder, int life, int playerNumber)
    {
        OnDataChanged?.Invoke(playerNumber, gunpowder, life);
    }
    public event Action OnMapChanged;    // UI 변경 => 방장이 맵을 변경했을 때

    public void MapChanged()
    {
        OnMapChanged?.Invoke();
    }
    public event Action OnRoomDataChanged;  // UI 변경 => 플레이어들이 자리를 이동할 때

    public void RoomDataChanged()
    {
        OnRoomDataChanged?.Invoke();
    }
    public event Action OnReadyChanged; // UI 변경 => 플레이어들이 레디를 할 때.

    public void ReadyChange()
    {
        OnReadyChanged?.Invoke();
    }
    public event Action OnMasterChanged; // 방장 변경 => 방장 권한 버튼 못 누르게 하기

    public void MasterChanged()
    {
        OnMasterChanged?.Invoke();
    }
}
