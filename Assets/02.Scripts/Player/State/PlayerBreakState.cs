using RobustFSM.Base;
using UnityEngine;

public class PlayerBreakState : PlayerBaseState
{
    private float _breakTimer = 0f;
    private int _moveDirection = 1; // 브레이크 방향(반대방향)
    private bool _doubleTapReady = true; // 진입 시 이미 1회 입력된 것으로 간주
    private bool _isDoubleTapped = false;

    public override void OnEnter()
    {
        base.OnEnter();

        _breakTimer = 0f;
        _moveDirection = _owner.PlayerStat.FacingDirection;
        _doubleTapReady = true;
        _isDoubleTapped = false;

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;

        

        // 애니메이션 재생
        _owner.SetAnimatorTrigger("Break");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.SetFacingDirection(-_moveDirection);
        _owner.ResetAnimatorTrigger("Break");
    }

    public override void MineUpdate()
    {
        _breakTimer += Time.deltaTime;

        // Rigidbody2D 기반 이동 (브레이크 시 느리게 이동)
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _moveDirection * _owner.PlayerStat.MyMoveSpeed / 2f;
        _owner.Rigidbody2D.linearVelocity = velocity;

        // 브레이크 타임 내에 같은 방향 키가 한 번 더 눌리면 Run
        if(_doubleTapReady && _breakTimer <= _owner.PlayerStat.DoubleTapTime)
        {
            if(Input.GetKeyDown(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1 
            || Input.GetKeyDown(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
            {
                _isDoubleTapped = true;
                _doubleTapReady = false; // 더 이상 체크하지 않음
            }
        }

        if(_breakTimer >= _owner.PlayerStat.BreakTime)
        {
            if(_isDoubleTapped)
            {
                _playerFSM.ChangeState<PlayerRunState>();
            }
            else
            {
                if (IsGrounded2D())
                {
                    _playerFSM.ChangeState<PlayerIdleState>();  
                }
                else
                {
                    _owner.SetAnimatorTrigger("Fall");
                    _playerFSM.ChangeState<PlayerJumpState>();
                }
            }
        }
    }
} 