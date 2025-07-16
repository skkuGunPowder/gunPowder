using System;
using RobustFSM.Base;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float _gravity = -40f; // 더 강한 중력 추천
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _timer = 0f;
    private const float LANDING_GRACE_TIME = 0.1f;


    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        _owner.PlayerStat.IncrementJumpCount();

        // 점프 시작 시 Y속도에 점프 파워를 부여
        _yVelocity = _owner.PlayerStat.JumpForce;
        _timer = 0f;
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
        _timer += Time.deltaTime;

        // 중력 적용
        _yVelocity += _gravity * Time.deltaTime;
        _yVelocity = Mathf.Clamp(_yVelocity, _gravity * 3, _owner.PlayerStat.JumpForce);

        // 좌우 이동
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _xVelocity = 1;
            _owner.SetFacingDirection(1);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            _xVelocity = -1;
            _owner.SetFacingDirection(-1);
        }
        else
        {
            _xVelocity = 0;
        }
        _xVelocity *= _owner.PlayerStat.MyMoveSpeed;

        _owner.CharacterController.Move(new Vector3(_xVelocity, _yVelocity, 0) * Time.deltaTime);

        // 더블 점프
        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            _owner.PlayerStat.IncrementJumpCount();
            _yVelocity = _owner.PlayerStat.JumpForce;
        }

        // 방향키 더블 클릭 체크
        // 점프 대쉬상태로 전환
        // 방향키 더블탭 체크 (점프 대쉬)
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
        if (Time.time - _lastLeftTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
        {
            Debug.Log("점프 중 왼쪽 더블탭 - 점프 대쉬 상태로 전환");
            _playerFSM.ChangeState<PlayerJumpDashState>(); // 점프 대쉬 상태로 전환
            return;
        }
        _lastLeftTapTime = Time.time;
    }
    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
        if (Time.time - _lastRightTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
        {
            Debug.Log("점프 중 오른쪽 더블탭 - 점프 대쉬 상태로 전환");
            _playerFSM.ChangeState<PlayerJumpDashState>(); // 점프 대쉬 상태로 전환
        }
        _lastRightTapTime = Time.time;
    }



        // 착지 체크 (유예 시간 이후에만)
        if (_timer > LANDING_GRACE_TIME && _owner.CharacterController.isGrounded)
        {
            Debug.Log("착지!");
            _playerFSM.ChangeState<PlayerIdleState>();
            return;
        }
    }
}
