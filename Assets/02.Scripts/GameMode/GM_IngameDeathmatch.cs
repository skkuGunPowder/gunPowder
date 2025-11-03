using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// 데스매치 모드: 모두 죽으면 끝 + 시간이 다 되면 끝
/// </summary>
public class GM_IngameDeathmatch : GM_IngameBase
{
    private bool _lastPlayerPhase = false;
    private bool _timerExpired = false;
    
    public override void Initialize(GameManager gm, System.Collections.Generic.List<PhotonPlayer> players)
    {
        base.Initialize(gm, players);
        // GameManager의 LastPlayer 상태와 동기화
        _lastPlayerPhase = gameManager != null && gameManager.LastPlayer;
        _timerExpired = false;
        CheckGameOverCondition();
    }
    
    public override void OnPlayerDead(PhotonPlayer player)
    {
        base.OnPlayerDead(player);
    }
    
    public override void OnTimerExpired()
    {
        _timerExpired = true;
        // 시간이 다 되면 게임 종료
        CheckGameOverCondition();
    }
    
    /// <summary>
    /// 게임 종료 조건 체크
    /// 1. 모두 죽으면 끝
    /// 2. 시간이 다 되면 끝
    /// 3. LastPlayer 상태에서 1명 남으면 끝
    /// </summary>
    public override bool CheckGameOverCondition()
    {
        // 시간이 다 되면 게임 종료
        if (_timerExpired)
        {
            return true;
        }

        // 혼자 남은 경우
        if (playerList.Count == 1)
        {
            return true;
        }
        
        int aliveCount = GetAlivePlayerCount();
        
        // 모두 죽으면 게임 종료
        if (aliveCount == 0)
        {
            return true;
        }
        
        // GameManager의 LastPlayer 상태와 동기화
        if (gameManager != null)
        {
            _lastPlayerPhase = gameManager.LastPlayer;
        }
        
        // 플레이어가 두명 남았는가? (LastPlayer 페이즈 시작)
        if (!_lastPlayerPhase && aliveCount == 2)
        {
            if (gameManager != null && gameManager.PhotonView != null)
            {
                gameManager.PhotonView.RPC(nameof(GameManager.RPC_LastPlayer), RpcTarget.All);
            }
            _lastPlayerPhase = true;
            return false;
        }
        
        // LastPlayer 페이즈에서 1명 남으면 게임 종료
        if (_lastPlayerPhase && aliveCount == 1)
        {
            return false;
        }
        
        // 2명 이상인데 살아있는 사람이 1명일 때 (LastPlayer 페이즈 전)
        if (!_lastPlayerPhase && aliveCount == 1)
        {
            return true;
        }
        
        return false;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
