using RobustFSM.Base;


public class PlayerFSM : MonoFSM<Player>
{
    public override void AddStates()
    {
        // 상태 등록
        AddState<PlayerIdleState>();
        AddState<PlayerWalkState>();
        AddState<PlayerJumpState>();
        AddState<PlayerDashState>();
        AddState<PlayerRunState>();
        AddState<PlayerBreakState>();
        AddState<PlayerJumpDashState>();
        AddState<PlayerRecoilState>();
        AddState<PlayerDamagedState>();
        AddState<PlayerNormalRecoilState>();

        // 초기 상태 설정
        SetInitialState<PlayerIdleState>();
    }
} 