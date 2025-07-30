using System;
using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _timer = 0f;
    private const float LANDING_GRACE_TIME = 0.2f;

    // 키 릴리즈 타이머 추가
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.3f;
    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    private float _originalGravity;

    public override void OnEnter()
    {
        base.OnEnter();

        _owner.PlayerStat.IsJumping = true;
        
        _yVelocity = 0;
        if (_owner.PlayerStat.IsFallingFromLedge)
        {
            _yVelocity = 0f; // 낙하
            _owner.PlayerStat.IsFallingFromLedge = false;
        }
        else if(!_playerFSM.IsPreviousState<PlayerJumpDashState>()
            && !_playerFSM.IsPreviousState<PlayerRecoilState>()
            && !_playerFSM.IsPreviousState<PlayerNormalRecoilState>()
            && !_playerFSM.IsPreviousState<PlayerBreakState>())
        {
            _owner.PlayerStat.IncrementJumpCount();
            _yVelocity = _owner.PlayerStat.JumpForce;
            // 점프 시에만 y속도 설정
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.y = _yVelocity;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
        _timer = 0f;
        _keyReleaseTimer = 0f;

        _originalGravity = _owner.Rigidbody2D.gravityScale;
    }

    public override void OnExit()
    {
        _owner.RPC_ResetAnimatorTrigger("Jump");
        //_owner.RPC_SetAnimatorTrigger("Land");

        _owner.Rigidbody2D.gravityScale = _originalGravity;
        base.OnExit();
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        //base.MineUpdate();

        _timer += Time.deltaTime;

        // 아래키를 누르는 동안 중력 증가
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            _owner.Rigidbody2D.gravityScale += 2;
        }
        if(Input.GetKeyUp(KeyCode.DownArrow)) 
        {
            _owner.Rigidbody2D.gravityScale = _originalGravity;
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
        // y축은 중력에만 맡김 (직접 제어하지 않음)

        // 좌우 이동 - 러닝 상태에 따른 처리
        if (_owner.PlayerStat.IsRunning)
        {
            if (Input.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1 || Input.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
            {
                _keyReleaseTimer = 0;
                _xVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed;
            }
            else if (Input.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1 || Input.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
            {
                _owner.PlayerStat.IsRunning = false;
                _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                _keyReleaseTimer = 0;
            }
            else
            {
                _keyReleaseTimer += Time.deltaTime;
                if (_keyReleaseTimer >= _keyReleaseThreshold)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                else
                {
                    _xVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (_owner.PlayerStat.FacingDirection == -1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = 1;
                _owner.RPC_SetFacingDirection(1);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (_owner.PlayerStat.FacingDirection == 1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = -1;
                _owner.RPC_SetFacingDirection(-1);
            }
            else
            {
                _xVelocity = 0;
            }
            _xVelocity *= _owner.PlayerStat.MyMoveSpeed;
        }

        // Rigidbody2D 기반 이동 적용
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        velocity.x = _xVelocity;
        // y축은 건드리지 않음 (중력에 맡김)
        _owner.Rigidbody2D.linearVelocity = velocity;

        // 더블 점프
        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            Debug.Log("Bomb Dash");
            _owner.PlayerStat.IncrementJumpCount();
            // 점프 시에만 y속도 설정
            /*
            velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.y = _owner.PlayerStat.JumpForce;
            _owner.Rigidbody2D.linearVelocity = velocity;*/
            Vector3 position = _owner.GetBombSpawnPoint().position;
            GameObject prefab = PhotonNetwork.Instantiate(nameof(_owner.DashExplosionPrefab), position, Quaternion.identity);
            if(prefab.TryGetComponent(out Explosion explosion))
            {
                explosion.Explode(false, _owner.PhotonView);
            }
        }

        // 방향키 더블 클릭 체크 (점프 대쉬)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - _lastLeftTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
            {
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return false;
            }
            _lastLeftTapTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - _lastRightTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
            {
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return false;
            }
            _lastRightTapTime = Time.time;
        }

        // 착지 체크 (유예 시간 이후에만, 2D Raycast 사용)
        if (_timer > LANDING_GRACE_TIME && IsGrounded2D())
        {
            _owner.SetAnimatorTrigger("Land");
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
                _playerFSM.ChangeState<PlayerNormalRecoilState>();
            }
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
                _playerFSM.ChangeState<PlayerNormalRecoilState>();
            }
        }
    }
} 