using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// 무한 모드: 목숨 무한 + 시간이 다 되면 끝
/// </summary>
public class GM_IngameInfinite : GM_IngameBase
{
    private bool _timerExpired = false;
    
    public override void Initialize(GameManager gm, System.Collections.Generic.List<PhotonPlayer> players)
    {
        base.Initialize(gm, players);
        _timerExpired = false;
    }
    
    public override void OnPlayerDead(PhotonPlayer player)
    {
        // 무한 모드는 사망해도 게임이 끝나지 않음
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
    /// 시간이 다 되면 끝
    /// </summary>
    public override bool CheckGameOverCondition()
    {
        // 시간이 다 되면 게임 종료
        if (_timerExpired)
        {
            return true;
        }
        
        // 무한 모드는 사망으로 인한 게임 종료 없음
        return false;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
