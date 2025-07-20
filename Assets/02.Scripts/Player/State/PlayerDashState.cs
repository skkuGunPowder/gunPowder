using RobustFSM.Base;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _dashTimer = 0f;

    public override void OnEnter()
    {
        base.OnEnter();

        _dashTimer = 0f;

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = true;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.DashSpeed;

        // 애니메이션 재생
        _owner.MyAnimator.SetTrigger("Dash");
    }
    public override void OnExit()
    {
        base.OnExit();
        _owner.MyAnimator.ResetTrigger("Dash");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void Update()
    {
        _dashTimer += Time.deltaTime;

        // 1. 대시 이동(관성)
        _owner.CharacterController.Move(new Vector3(_owner.PlayerStat.FacingDirection, 0, 0)
                                    * _owner.PlayerStat.MyMoveSpeed * Time.deltaTime);

        int dir = _owner.PlayerStat.FacingDirection;
        // 2. 대시 중 반대 방향 키 입력 체크 → BreakState로 전환
        if(Input.GetKeyDown(KeyCode.RightArrow) && dir == -1 || Input.GetKeyDown(KeyCode.LeftArrow) && dir == 1)
        {
            if(!IsGrounded())
            {
                return;
            }
            Debug.Log("DashState: 반대 방향 키 다운 - BreakState로 전환");
            _playerFSM.ChangeState<PlayerBreakState>();
            return;
        }

        // 3. 대시 시간 종료 후 상태 전이
        if(_dashTimer >= _owner.PlayerStat.DashTime)
        {
            // 같은 방향 키 누르고 있음 → Run
            if ((_owner.PlayerStat.FacingDirection == 1 && Input.GetKey(KeyCode.RightArrow)) 
            || (_owner.PlayerStat.FacingDirection == -1 && Input.GetKey(KeyCode.LeftArrow)))
            {
                Debug.Log("DashState: 같은 방향 입력 - RunState로 전환");
                _playerFSM.ChangeState<PlayerRunState>();
                return;
            }
            // 아무 키도 안 누름 → Idle
            else
            {
                if(IsGrounded())
                {
                    Debug.Log("DashState: 입력 없음 - IdleState로 전환");
                    _playerFSM.ChangeState<PlayerIdleState>();
                    return;
                }
                else
                {
                    _owner.PlayerStat.IsFallingFromLedge = true;
                    _owner.MyAnimator.SetTrigger("Dash");
                    _playerFSM.ChangeState<PlayerJumpState>();
                    return;
                }
            }
        }
    }
}
