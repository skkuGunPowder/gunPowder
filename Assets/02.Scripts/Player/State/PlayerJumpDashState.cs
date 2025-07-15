using UnityEngine;
using RobustFSM.Base;

public class PlayerJumpDashState : PlayerBaseState
{
    private float _dashTimer = 0f;
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _gravity = -40f;

    public override void OnEnter()
    {
        base.OnEnter();
        // 플레이어 상태
        _owner.IsRunning = false;
        _owner.IsJumping = true;
        _owner.MyMoveSpeed = _owner.PlayerStatSO.DashSpeed;

        _yVelocity = 0f;
        _xVelocity = 0f;
        _dashTimer = 0f;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Dash");
    }
    public override void OnExit()
    {
        base.OnExit();
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void Update()
    {
        
        // 대쉬 시간 종료 후 점프 상태와 같이 움직임
        if(_dashTimer >= _owner.PlayerStatSO.DashTime)
        {
            _yVelocity += _gravity * Time.deltaTime;
            _yVelocity = Mathf.Clamp(_yVelocity, _gravity * 3, _owner.PlayerStatSO.JumpForce);

            // 좌우 이동
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if(_owner.FacingDirection == -1)
                {
                    _owner.MyMoveSpeed = _owner.PlayerStatSO.MoveSpeed;
                }
                _xVelocity = 1;
                _owner.SetFacingDirection(1);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if(_owner.FacingDirection == 1)
                {
                    _owner.MyMoveSpeed = _owner.PlayerStatSO.MoveSpeed;
                }
                _xVelocity = -1;
                _owner.SetFacingDirection(-1);
            }
            else
            {
                _xVelocity = 0;
            }
            _xVelocity *= _owner.MyMoveSpeed;

            _owner.CharacterController.Move(new Vector3(_xVelocity, _yVelocity, 0) * Time.deltaTime);

            // 땅에 닿으면 Idle 상태 전환
            if(_owner.CharacterController.isGrounded)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }

            // 더블 점프가 가능하다면 점프 상태로 전환
            if(Input.GetKeyDown(KeyCode.Space) && _owner.JumpCount < _owner.PlayerStatSO.MaxJumpCount)
            {
                _playerFSM.ChangeState<PlayerJumpState>();
            }
        }
        else
        {
            // 대쉬 이동후 낙하
            _dashTimer += Time.deltaTime;
            _owner.CharacterController.Move(new Vector3(_owner.FacingDirection, 0, 0) 
                                            * _owner.MyMoveSpeed * Time.deltaTime);
        }
    }
}
