using System;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
public class EventManager : DontDestroySingleton<EventManager> // Start is called once before the first execution of Update after the MonoBehaviour is created
{
    public event Action<int> OnTopPlayerChanged;        // 순위 변경용  = 1등 체크용
    public void SetTopPlayer(int topActor)
    {
        OnTopPlayerChanged?.Invoke(topActor);
    }
    public event Action<int, int, int,int> OnDataChanged;    // 체력 감소할 때
    public void PlayerDataChange(int gunpowder, int life, int playerNumber,int attacker)
    {
        OnDataChanged?.Invoke(playerNumber, gunpowder, life,attacker);
    }
    public event Action<EMap> OnMapChanged;    // UI 변경 => 방장이 맵을 변경했을 때

    public void MapChanged(EMap currentMap)
    {
        OnMapChanged?.Invoke(currentMap);
    }
    public event Action OnRoomDataChanged;  // UI 변경 => 플레이어들이 자리를 이동할 때

    public void RoomDataChanged()
    {
        OnRoomDataChanged?.Invoke();
    }
    public event Action<PhotonPlayer> OnReadyChanged; // UI 변경 => 플레이어들이 레디를 할 때.

    public void ReadyChange(PhotonPlayer player)
    {
        OnReadyChanged?.Invoke(player);
    }
    public event Action OnMasterChanged; // 방장 변경 => 방장 권한 버튼 못 누르게 하기

    public void MasterChanged()
    {
        OnMasterChanged?.Invoke();
    }
    public event Action OnGameResult;

    public void ViewGameResult()
    {
        OnGameResult?.Invoke();
    }

    public event Action<PhotonPlayer> OnPlayerChanged;

    public void PlayerLeftRoom(PhotonPlayer player)
    {
        OnPlayerChanged?.Invoke(player);
    }

    public event Action OnTeamChanged;
    public void TeamChanged()
    {
        OnTeamChanged?.Invoke();
    }

    public event Action<int, bool, int> OnUpdateKillLog;
    public void OnUpdateLog(int kill, bool isNormal, int death)
    {
        OnUpdateKillLog?.Invoke(kill,isNormal, death);
    }

    public event Action OnPlayerItemChanged;
    public void PlayerItemChanged()
    {
        OnPlayerItemChanged?.Invoke();
    }
    
    public event Action OnLoadFinished;

    public void LoadFinished()
    {
        OnLoadFinished?.Invoke();   
    }
    
    public event Action OnProfileInit;
    
    public void ProfileInit()
    {
        OnProfileInit?.Invoke();
    }
    
    public event Action OnTargetChanged;
    
    public void TargetChanged()
    {
        OnTargetChanged?.Invoke();
    }
    
    public event Action<int,EInGameTeam> OnPlayerColorChanged;
    public void PlayerColorChanged(int playerNumber, EInGameTeam color)
    {
        OnPlayerColorChanged?.Invoke(playerNumber, color);
    }

    public event Action OnLoadEnd;
    public void LoadEnd()
    {
        OnLoadEnd?.Invoke();
    }
    public event Action<string, PhotonPlayer> OnUltimate;
    public void Ultimate(string bombName, PhotonPlayer player)
    {
        OnUltimate?.Invoke(bombName, player);
    }
    
    public event Action<string, int> OnPlayEmotion;
    public void PlayEmotion(string emotionName, int playerNumber)
    {
        OnPlayEmotion?.Invoke(emotionName,playerNumber);
    }

    public event Action OnPlayerFind;
    public void PlayerFind()
    {
        OnPlayerFind?.Invoke();
    }
    
    public event Action OnPlayerListUp;
    public void PlayerListUp()
    {
        OnPlayerListUp?.Invoke();
    }
    public event Action OnBackGroundFade;
    public void BackGroundFade()
    {
        OnBackGroundFade?.Invoke();
    }

    public event Action OnHitScreen;
    public void HitScreen()
    {
        OnHitScreen?.Invoke();
    }

    public event Action<int> OnLastAttack;

    public void LastAttack(int actorNumber)
    {
        OnLastAttack?.Invoke(actorNumber);
    }

    public event Action OnGameSet;

    public void GameSet()
    {
        OnGameSet?.Invoke();
    }
    public event Action OnRoomListUpdate;

    public void RoomListUpdate()
    {
        OnRoomListUpdate?.Invoke();
    }
    public event Action<PhotonPlayer> OnPlayerLeft;

    public void PlayerLeft(PhotonPlayer player)
    {
        OnPlayerLeft?.Invoke(player);
    }
    
    public event Action OnPlayObserve;

    public void PlayObserve()
    {
        OnPlayObserve?.Invoke();
    }
}
