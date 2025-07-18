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

    // 키 릴리즈 타이머 추가
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.3f;
    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        
        _gravity = -40f;
        _yVelocity = 0;
        if (_owner.PlayerStat.IsFallingFromLedge)
        {
            _yVelocity = 0f; // 낙하
            _owner.PlayerStat.IsFallingFromLedge = false;
        }
        else if(!_playerFSM.IsPreviousState<PlayerJumpDashState>()
            && !_playerFSM.IsPreviousState<PlayerRecoilState>())
        {
            _owner.PlayerStat.IncrementJumpCount();
            _yVelocity = _owner.PlayerStat.JumpForce;
        }
        _timer = 0f;
        _keyReleaseTimer = 0f;
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
        base.Update();

        _timer += Time.deltaTime;

        // 아래키를 누르는 동안 중력 증가
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            _gravity = -60f;
        }
        if(Input.GetKeyUp(KeyCode.DownArrow))
        {
            _gravity = -40f;
        }

        bool flowControl = JumpMove();
        if (!flowControl)
        {
            return;
        }

        JumpAttack();
    }

    private bool JumpMove()
    {
        // 중력 적용
        _yVelocity += _gravity * Time.deltaTime;
        _yVelocity = Mathf.Clamp(_yVelocity, _gravity * 3, _owner.PlayerStat.JumpForce);

        // 좌우 이동 - 러닝 상태에 따른 처리
        if (_owner.PlayerStat.IsRunning)
        {
            // 러닝 상태일 때의 이동 처리
            if (Input.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1 || Input.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
            {
                _keyReleaseTimer = 0; // 키를 누르고 있으면 타이머 리셋
                _xVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed;
            }
            else if (Input.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1 || Input.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
            {
                // 반대 방향키를 누르면 즉시 러닝 상태 해제
                _owner.PlayerStat.IsRunning = false;
                _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                _keyReleaseTimer = 0; // 타이머도 리셋
            }
            else
            {
                _keyReleaseTimer += Time.deltaTime;
                if (_keyReleaseTimer >= _keyReleaseThreshold)
                {
                    // 키 릴리즈 시간이 지나면 러닝 상태 해제
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                else
                {
                    // 관성 유지
                    _xVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed;
                }
            }
        }
        else
        {
            // 일반 점프 상태일 때의 이동 처리
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (_owner.PlayerStat.FacingDirection == -1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = 1;
                _owner.SetFacingDirection(1);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (_owner.PlayerStat.FacingDirection == 1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = -1;
                _owner.SetFacingDirection(-1);
            }
            else
            {
                _xVelocity = 0;
            }
            _xVelocity *= _owner.PlayerStat.MyMoveSpeed;
        }

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
                return false;
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
            return false;
        }

        return true;
    }

    private void JumpAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z) && CanNormalBomb())
        {
            if(_owner.PlayerStat.IsRunning)
            {
                ThrowStraightNormalBomb();
                _playerFSM.ChangeState<PlayerRecoilState>();
            }
            else
            {
                ThrowNormalBomb();
            }
            SetLastNormalBombTime();
        }
        if (Input.GetKeyDown(KeyCode.X) && CanSpecialBomb())
        {
            if(_owner.PlayerStat.IsRunning)
            {
                ThrowStraightSpecialBomb();
                _playerFSM.ChangeState<PlayerRecoilState>();
            }
            else
            {
                ThrowSpecialBomb();
            }
            SetLastSpecialBombTime();
        }
    }
} 