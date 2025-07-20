using System;
using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;

    private float _lastNormalBombTime = 0f;
    private float _lastSpecialBombTime = 0f;



    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        Debug.Log($"Enter {this.GetType().Name} State");
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    public virtual void Update()
    {
        JumpInput();
    }

    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        if (_playerFSM.IsCurrentState<PlayerDashState>() || _playerFSM.IsCurrentState<PlayerJumpDashState>())
            return;

        if (Input.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            _owner.MyAnimator.SetTrigger("Jump");
            _playerFSM.ChangeState<PlayerJumpState>();
        }
    }

    // Raycast로 바닥 체크
    protected virtual bool IsGrounded()
    {
        float rayDistance = 0.2f;
        Vector3 origin = _owner.transform.position;
        var cc = _owner.GetComponent<CharacterController>();
        if (cc != null)
        {
            rayDistance = cc.height / 2f + 0.1f;
        }
        return Physics.Raycast(origin, Vector3.down, rayDistance);
    }

    protected virtual bool CanNormalBomb()
    {
        if(_owner.AttackTimer - _lastNormalBombTime < _owner.NormalBomb.BombCoolTime)
        {
            return false;
        }
        return true;
    }

    protected virtual bool CanSpecialBomb()
    {
        if(_owner.AttackTimer - _lastSpecialBombTime < _owner.SpecialBomb.BombCoolTime)
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
        if (spawnPoint.HasValue)
        {
            _owner.NormalBomb.PlaceBomb(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.NormalBomb.PlaceBomb(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 일반 폭탄을 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if (spawnPoint.HasValue)
        {
            _owner.NormalBomb.ThrowBomb(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.NormalBomb.ThrowBomb(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 특수 폭탄을 배치하고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void PlaceSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if (spawnPoint.HasValue)
        {
            _owner.SpecialBomb.PlaceBomb(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.SpecialBomb.PlaceBomb(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 특수 폭탄을 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if (spawnPoint.HasValue)
        {
            _owner.SpecialBomb.ThrowBomb(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.SpecialBomb.ThrowBomb(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 일반 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if (spawnPoint.HasValue)
        {
            _owner.NormalBomb.ThrowBombStraight(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.NormalBomb.ThrowBombStraight(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }

    /// <summary>
    /// 특수 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if (spawnPoint.HasValue)
        {
            _owner.SpecialBomb.ThrowBombStraight(_owner.GetBombSpawnPoint(spawnPoint.Value));
        }
        else
        {
            _owner.SpecialBomb.ThrowBombStraight(_owner.GetBombSpawnPoint());
        }
        ResetGunPowderDecreaseWithoutAttackTimer();
    }
}
