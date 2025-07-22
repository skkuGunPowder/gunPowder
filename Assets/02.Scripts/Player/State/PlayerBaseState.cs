using System;
using Photon.Pun;
using RaycastPro.RaySensors2D;
using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

    private float _lastNormalBombTime = 0f;
    private float _lastSpecialBombTime = 0f;

    public float BombCoolTime = 0.2f;
    protected BoxRay2D _groundRay2D;




    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;
        Debug.Log($"Enter {this.GetType().Name} State");

        //
        _owner.OnHit += HandleHit;
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"Exit {this.GetType().Name} State");

        //
        _owner.OnHit -= HandleHit;
    }

    protected virtual void HandleHit()
    {
        _playerFSM.ChangeState<PlayerDamagedState>();
    }

    public virtual void MineUpdate()
    {
        JumpInput();
    }

    private void Update()
    {
        if(_owner.PhotonView.IsMine)
        {
            MineUpdate();
        }
    }
    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        if (_playerFSM.IsCurrentState<PlayerDashState>() 
        || _playerFSM.IsCurrentState<PlayerJumpDashState>())
            return;

        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            _owner.SetAnimatorTrigger("Jump");
            _playerFSM.ChangeState<PlayerJumpState>();
        }
    }

    // 2D Raycast로 바닥 체크
    protected virtual bool IsGrounded2D()
    {
        if(_groundRay2D == null)
        {
            return false;
        }

        _groundRay2D.Cast();
        return _groundRay2D.Performed;
    }

    protected virtual bool CanNormalBomb()
    {
        if(_owner.AttackTimer - _lastNormalBombTime < BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual bool CanSpecialBomb()
    {
        if(_owner.AttackTimer - _lastSpecialBombTime < BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual void SetLastNormalBombTime()
    {
        _lastNormalBombTime = _owner.AttackTimer;
    }

    protected virtual void SetLastSpecialBombTime()
    {
        _lastSpecialBombTime = _owner.AttackTimer;
    }

    protected virtual void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        _owner.ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 일반 폭탄을 배치하고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void PlaceNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.NormalBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().PlaceBomb(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            // 점프 공격
            _owner.SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.SetAnimatorTrigger("Attack");
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    /// <summary>
    /// 일반 폭탄을 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
            
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.NormalBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().ThrowBomb(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            // 점프 공격
            _owner.SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.SetAnimatorTrigger("Attack");
        }
        
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    /// <summary>
    /// 특수 폭탄을 배치하고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void PlaceSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.SpecialBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().PlaceBomb(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            // 점프 공격
            _owner.SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.SetAnimatorTrigger("Attack");
        }

        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    /// <summary>
    /// 특수 폭탄을 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.SpecialBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().ThrowBomb(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            // 점프 공격
            _owner.SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.SetAnimatorTrigger("Attack");
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    /// <summary>
    /// 일반 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.NormalBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().ThrowBombStraight(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.SetAnimatorTrigger("JumpUpStrongAttack");
            }
            else
            {
                _owner.SetAnimatorTrigger("JumpStrongAttack");
            }
        }
        else
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.SetAnimatorTrigger("UpStrongAttack");
            }
            else
            {
                _owner.SetAnimatorTrigger("StrongAttack");
            }
        }

        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    /// <summary>
    /// 특수 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        Transform bombSpawnPoint;
        if (spawnPoint.HasValue)
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint(spawnPoint.Value);
        }
        else
        {
            bombSpawnPoint = _owner.GetBombSpawnPoint();
        }
        GameObject bomb = Instantiate(_owner.SpecialBombPrefab, bombSpawnPoint.position, Quaternion.identity);
        bomb.GetComponent<Bomb>().ThrowBombStraight(bombSpawnPoint);

        if (_owner.PlayerStat.IsJumping)
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.SetAnimatorTrigger("JumpUpStrongAttack");
            }
            else
            {
                _owner.SetAnimatorTrigger("JumpStrongAttack");
            }
        }
        else
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.SetAnimatorTrigger("UpStrongAttack");
            }
            else
            {
                _owner.SetAnimatorTrigger("StrongAttack");
            }
        }
        
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }
}
