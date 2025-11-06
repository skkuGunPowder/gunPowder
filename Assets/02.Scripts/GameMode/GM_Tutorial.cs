using Photon.Pun;
using PhotonPlayer = Photon.Realtime.Player;

/// <summary>
/// 튜토리얼 모드: 게임 종료 로직 없음
/// </summary>
public class GM_Tutorial : GM_IngameBase
{
    public override void Initialize(GameManager gm, System.Collections.Generic.List<PhotonPlayer> players)
    {
        base.Initialize(gm, players);
    }
    
    /// <summary>
    /// 게임 종료 조건 체크
    /// 튜토리얼은 게임 종료 조건이 없음
    /// </summary>
    public override bool CheckGameOverCondition()
    {
        // 튜토리얼은 게임 종료 조건 없음
        return false;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
