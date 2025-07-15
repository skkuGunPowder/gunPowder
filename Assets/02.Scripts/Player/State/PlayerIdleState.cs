using RobustFSM.Base;
using UnityEngine;

public class PlayerIdleState : MonoState
{
    private PlayerFSM _playerFSM;
    private Player _owner;
    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        // 캐스팅
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        // 플레이어 상태
        _owner.IsRunning = false;
        _owner.IsJumping = false;
        _owner.MyMoveSpeed = _owner.PlayerStatSO.MoveSpeed;  // 기본 이동속도
        _owner.JumpCount = 0;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Idle");
    }
    public override void OnExit()
    {
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    private void Update()
    {
        // 이동키를 받으면 걷기 상태로 전환
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
        || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
         {
            _owner.SetFacingDirection(Input.GetKey(KeyCode.LeftArrow) ? -1 : 1);
            _playerFSM.ChangeState<PlayerWalkState>();
         }
        // 점프키(space)를 누르면 점프 상태로 전환
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _playerFSM.ChangeState<PlayerJumpState>();
        }
    
    }
}
