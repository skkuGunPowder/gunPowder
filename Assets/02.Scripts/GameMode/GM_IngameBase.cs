using System.Collections.Generic;
using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;
using ExitGames.Client.Photon;

/// <summary>
/// 게임 모드의 기본 추상 클래스
/// 각 게임 모드가 상속받아 게임 종료 조건 및 로직을 구현합니다
/// </summary>
public abstract class GM_IngameBase
{
    protected GameManager gameManager;
    protected List<PhotonPlayer> playerList;
    
    /// <summary>
    /// 모드 초기화
    /// </summary>
    public virtual void Initialize(GameManager gm, List<PhotonPlayer> players)
    {
        gameManager = gm;
        playerList = players;
    }
    
    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public virtual void OnGameStart() { }
    
    /// <summary>
    /// 플레이어가 죽었을 때 호출
    /// </summary>
    public virtual void OnPlayerDead(PhotonPlayer player) { }
    
    /// <summary>
    /// 플레이어가 나갔을 때 호출
    /// </summary>
    public virtual void OnPlayerLeft(PhotonPlayer player)
    {
        if (playerList != null && player != null)
        {
            playerList.Remove(player);
            UpdatePlayerList();
        }
    }
    
    /// <summary>
    /// 플레이어 리스트 업데이트
    /// </summary>
    public virtual void UpdatePlayerList()
    {
        if (playerList == null) return;
        
        PhotonPlayer[] players = PhotonNetwork.PlayerList;
        playerList = new List<PhotonPlayer>(players);
    }
    
    /// <summary>
    /// 시간이 다 되었을 때 호출 (타이머 종료)
    /// </summary>
    public virtual void OnTimerExpired() { }
    
    /// <summary>
    /// 게임 종료 조건 체크
    /// true 반환 시 게임 종료
    /// </summary>
    public abstract bool CheckGameOverCondition();
    
    /// <summary>
    /// 업데이트 (필요한 경우)
    /// </summary>
    public virtual void Update() { }
    
    /// <summary>
    /// 정리 작업
    /// </summary>
    public virtual void OnDestroy() { }
    
    /// <summary>
    /// 살아있는 플레이어 수 반환
    /// </summary>
    protected int GetAlivePlayerCount()
    {
        if (playerList == null) return 0;
        
        int aliveCount = 0;
        foreach (PhotonPlayer p in playerList)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                          (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            if (!isDead)
            {
                aliveCount++;
            }
        }
        return aliveCount;
    }
    
    /// <summary>
    /// 죽은 플레이어 수 반환
    /// </summary>
    protected int GetDeadPlayerCount()
    {
        if (playerList == null) return 0;
        
        int deadCount = 0;
        foreach (PhotonPlayer p in playerList)
        {
            bool isDead = p.CustomProperties.ContainsKey(EProperties.IsDead.ToString()) &&
                          (bool)p.CustomProperties[EProperties.IsDead.ToString()];
            if (isDead)
            {
                deadCount++;
            }
        }
        return deadCount;
    }
}
