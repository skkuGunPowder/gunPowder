using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// 대기 모드: 게임 종료 로직 없음
/// </summary>
public class GM_Waiting : GM_IngameBase
{
    public override void Initialize(GameManager gm, System.Collections.Generic.List<PhotonPlayer> players)
    {
        base.Initialize(gm, players);
    }
    
    /// <summary>
    /// 게임 종료 조건 체크
    /// 대기 모드는 게임 종료 조건이 없음
    /// </summary>
    public override bool CheckGameOverCondition()
    {
        // 대기 모드는 게임 종료 조건 없음
        return false;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
