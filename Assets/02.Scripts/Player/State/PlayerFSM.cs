using RobustFSM.Base;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 플레이어 상태 머신(FSM) 등록/초기화 클래스
/// 
/// 역할:
/// - 모든 플레이어 상태 등록 및 초기 상태 설정
/// - 중요한 상태 전환의 네트워크 동기화 제공
/// 
/// 동작 방식:
/// 1. AddStates에서 상태들을 등록하고 초기 상태를 설정
/// 2. SyncStateChange<T>() 호출 시 본인 클라이언트에서 RPC로 전 클라이언트에 상태 전파
/// </summary>
public class PlayerFSM : MonoFSM<Player>
{
    /// <summary>
    /// 플레이어 상태 등록 및 초기 상태 설정
    /// </summary>
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
        AddState<PlayerFallState>();
        AddState<PlayerObserveState>();
        AddState<PlayerConfuseState>();
        AddState<PlayerLastDieState>();
        AddState<PlayerCrabHoldedState>();

        // 초기 상태 설정
        SetInitialState<PlayerIdleState>();
    }

    /// <summary>
    /// 중요한 상태 변경을 네트워크로 동기화
    /// </summary>
    public void SyncStateChange<T>() where T : PlayerBaseState
    {
        if (Owner == null || Owner.PhotonView == null)
        {
            return;
        }

        if (Owner.PhotonView.IsMine)
        {
            // 모든 클라이언트에서 상태 변경 (자신 포함)
            Owner.PhotonView.RPC(nameof(Owner.RPC_ChangeState), RpcTarget.All, typeof(T).Name);
        }
    }
} 