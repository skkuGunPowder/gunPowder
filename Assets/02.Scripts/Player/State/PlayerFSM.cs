using RobustFSM.Base;
using Photon.Pun;
using UnityEngine;

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
        AddState<PlayerDieState>();
        AddState<PlayerFallDeadState>();
        AddState<PlayerHitStopState>();

        // 초기 상태 설정
        SetInitialState<PlayerIdleState>();
    }

    /// <summary>
    /// 중요한 상태 변경을 네트워크로 동기화
    /// </summary>
    public void SyncStateChange<T>() where T : PlayerBaseState
    {
        if (Owner.PhotonView.IsMine)
        {
            // 모든 클라이언트에서 상태 변경 (자신 포함)
            Owner.PhotonView.RPC(nameof(Owner.RPC_ChangeState), RpcTarget.All, typeof(T).Name);
        }
    }
} 