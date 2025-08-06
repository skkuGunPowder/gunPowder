using System;
using Photon.Pun;
using RobustFSM.Base;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _timer = 0f;
    
    // Y축 속도 제한
    private const float MAX_JUMP_SPEED = 40f;  // 최대 점프 속도

    // 키 릴리즈 타이머 추가
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.3f;
    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    private float _originalGravity;
    
    // 방향 변경 감지용
    private int _lastFacingDirection = 0;

    private float _explosionOverrideTimer = 0f;
    private const float EXPLOSION_OVERRIDE_DURATION = 0.3f; // n초 동안 velocity.x 덮어쓰기 차단

    // 점프 최고점 감지용
    private float _lastYVelocity = 0f;
    private bool _hasReachedPeak = false;

    public override void OnEnter()
    {
        base.OnEnter();

        _owner.RPC_SetAnimatorBool("LandBool", false);
        _owner.RPC_SetAnimatorTrigger("Jump");

        _owner.PlayerStat.IsJumping = true;
        _yVelocity = 0;
        
        if(!_playerFSM.IsPreviousState<PlayerFallState>())
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
        _hasReachedPeak = false;
        _lastYVelocity = _owner.Rigidbody2D.linearVelocity.y;
        
        SoundManager.Instance.PlayLocalSound("PlayerJump_1", transform);
    }

    public override void OnExit()
    {
        _owner.RPC_ResetAnimatorTrigger("Jump");
        _owner.Rigidbody2D.gravityScale = _originalGravity;
        //_owner.PlayerStat.IsJumping = false;
        base.OnExit();
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    public override void MineUpdate()
    {
        _timer += Time.deltaTime;
        if (_explosionOverrideTimer > 0f)
        {
            _explosionOverrideTimer -= Time.deltaTime;
        }

        // Y축 속도 제한 적용
        LimitYVelocity();

        // 점프 최고점 감지 및 FallState로 전환
        if (HandleJumpPeakDetection())
        {
            _playerFSM.ChangeState<PlayerFallState>();
            return;
        }

        bool flowControl = JumpMove();
        if (!flowControl)
        {
            return;
        }

        JumpAttack();
    }

    /// <summary>
    /// 점프 최고점 감지 로직
    /// </summary>
    private bool HandleJumpPeakDetection()
    {
        float currentYVelocity = _owner.Rigidbody2D.linearVelocity.y;
        
        // 이전 프레임에서 양수였는데 지금 음수가 되면 최고점 도달
        if (_lastYVelocity > 0f && currentYVelocity <= 0f && !_hasReachedPeak)
        {
            _hasReachedPeak = true;
            return true; // FallState로 전환
        }
        
        _lastYVelocity = currentYVelocity;
        return false;
    }

    private bool JumpMove()
    {
        // y축은 중력에만 맡김 (직접 제어하지 않음)

        // 좌우 이동 - 러닝 상태에 따른 처리
        if (_owner.PlayerStat.IsRunning)
        {
            if (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1
            || InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
            {
                _keyReleaseTimer = 0;
                _xVelocity = _owner.PlayerStat.FacingDirection * _owner.PlayerStat.RunSpeed;
            }
            else if (InputHandler.GetKey(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == -1 
            || InputHandler.GetKey(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == 1)
            {
                _owner.PlayerStat.IsRunning = false;
                _owner.RPC_SetFacingDirection(-_owner.PlayerStat.FacingDirection);
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
            if (InputHandler.GetKey(KeyCode.RightArrow))
            {
                if (_owner.PlayerStat.FacingDirection == -1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = 1;
                // 방향이 바뀔 때만 RPC 호출
                if (_lastFacingDirection != 1)
                {
                    _owner.RPC_SetFacingDirection(1);
                    _lastFacingDirection = 1;
                }
            }
            else if (InputHandler.GetKey(KeyCode.LeftArrow))
            {
                if (_owner.PlayerStat.FacingDirection == 1)
                {
                    _owner.PlayerStat.IsRunning = false;
                    _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
                }
                _xVelocity = -1;
                // 방향이 바뀔 때만 RPC 호출
                if (_lastFacingDirection != -1)
                {
                    _owner.RPC_SetFacingDirection(-1);
                    _lastFacingDirection = -1;
                }
            }
            else
            {
                _xVelocity = 0;
            }
            _xVelocity *= _owner.PlayerStat.MyMoveSpeed;
        }

        // Rigidbody2D 기반 이동 적용
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;

        if (_explosionOverrideTimer <= 0f)
        {
            velocity.x = Mathf.Lerp(velocity.x, _xVelocity, 1 * Time.deltaTime);
            // y축은 건드리지 않음 (중력에 맡김)
            _owner.Rigidbody2D.linearVelocity = velocity;
        }

        // 폭탄 대쉬
        if (InputHandler.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            _owner.PlayerStat.IncrementJumpCount();

            Vector3 position = _owner.GetExplosionSpawnPoint().position;
            GameObject prefab = PhotonNetwork.Instantiate("BasicBomb", position, Quaternion.identity);
            if(prefab.TryGetComponent(out Bomb bomb))
            {
                bomb.PhotonView.RPC(nameof(bomb.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
                bomb.PhotonView.RPC(nameof(bomb.Explode), RpcTarget.All);
                _explosionOverrideTimer = EXPLOSION_OVERRIDE_DURATION;
            }
        }

        // 방향키 더블 클릭 체크 (점프 대쉬)
        if (InputHandler.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - _lastLeftTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
            {
                _owner.RPC_SetFacingDirection(-1);
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return false;
            }
            _lastLeftTapTime = Time.time;
        }
        if (InputHandler.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - _lastRightTapTime <= _owner.PlayerStat.DoubleTapTime && _owner.PlayerStat.CanJumpDash())
            {
                _owner.RPC_SetFacingDirection(1);
                _playerFSM.ChangeState<PlayerJumpDashState>();
                return false;
            }
            _lastRightTapTime = Time.time;
        }

        return true;
    }
    
    /// <summary>
    /// Y축 속도를 제한하여 너무 빠르게 올라가는 것을 방지
    /// </summary>
    private void LimitYVelocity()
    {
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        
        // 점프 속도 제한 (양수)
        if (velocity.y > MAX_JUMP_SPEED)
        {
            velocity.y = MAX_JUMP_SPEED;
        }
        
        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    private void JumpAttack()
    {
        if (InputHandler.GetKeyDown(KeyCode.Z) && CanNormalBomb())
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
        if (InputHandler.GetKeyDown(KeyCode.X) && CanSpecialBomb())
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