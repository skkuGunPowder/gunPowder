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

    protected float _normalRecoilForce = 10f;
    protected float _strongRecoilForce = 20f;
    protected float _yRecoilForce = 5f;




    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;

        //
        _owner.OnHit += HandleHit;
    }

    public override void OnExit()
    {
        base.OnExit();

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

    private void SpawnAndRpcBomb(
        string prefabName,
        Transform bombSpawnPoint,
        string rpcMethodName,
        object[] rpcArgs)
    {
        GameObject bomb = InstantiateBomb(prefabName, bombSpawnPoint);
        Bomb bombComponent = bomb.GetComponent<Bomb>();
        bombComponent.PhotonView.RPC(nameof(Bomb.SetOwner), RpcTarget.All, _owner.GetComponent<PhotonView>().ViewID);
        bombComponent.PhotonView.RPC(rpcMethodName, RpcTarget.All, rpcArgs);
    }

    /// <summary>
    /// 일반 폭탄을 배치하고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void PlaceNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "BasicBomb",
            bombSpawnPoint,
            nameof(Bomb.PlaceBomb),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            _owner.RPC_SetAnimatorTrigger("PlaceAttack");
        }
        else
        {
            _owner.RPC_SetAnimatorTrigger("PlaceAttack");
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
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "BasicBomb",
            bombSpawnPoint,
            nameof(Bomb.ThrowBomb),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            _owner.RPC_SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.RPC_SetAnimatorTrigger("Attack");
        }
        
        ApplyRecoil(bombSpawnPoint, _normalRecoilForce, _yRecoilForce);
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    /// <summary>
    /// 특수 폭탄을 배치하고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void PlaceSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "Missile",
            bombSpawnPoint,
            nameof(Bomb.PlaceBomb),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            _owner.RPC_SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.RPC_SetAnimatorTrigger("Attack");
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
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "Missile",
            bombSpawnPoint,
            nameof(Bomb.ThrowBomb),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            _owner.RPC_SetAnimatorTrigger("JumpAttack");
        }
        else
        {
            _owner.RPC_SetAnimatorTrigger("Attack");
        }
        ApplyRecoil(bombSpawnPoint, _normalRecoilForce, _yRecoilForce);
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    /// <summary>
    /// 일반 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "BasicBomb",
            bombSpawnPoint,
            nameof(Bomb.ThrowBombStraight),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.RPC_SetAnimatorTrigger("JumpUpStrongAttack");
            }
            else
            {
                _owner.RPC_SetAnimatorTrigger("JumpStrongAttack");
            }
        }
        else
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.RPC_SetAnimatorTrigger("UpStrongAttack");
            }
            else
            {
                _owner.RPC_SetAnimatorTrigger("StrongAttack");
            }
        }

        ApplyRecoil(bombSpawnPoint, _strongRecoilForce, _yRecoilForce);
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    /// <summary>
    /// 특수 폭탄을 직선으로 던지고 자동으로 Reset을 호출합니다.
    /// </summary>
    /// <param name="spawnPoint">스폰 포인트 (기본값: 기본 스폰 포인트)</param>
    protected virtual void ThrowStraightSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        SpawnAndRpcBomb(
            "Missile",
            bombSpawnPoint,
            nameof(Bomb.ThrowBombStraight),
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        if (_owner.PlayerStat.IsJumping)
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.RPC_SetAnimatorTrigger("JumpUpStrongAttack");
            }
            else
            {
                _owner.RPC_SetAnimatorTrigger("JumpStrongAttack");
            }
        }
        else
        {
            if(Input.GetKey(KeyCode.UpArrow))
            {
                _owner.RPC_SetAnimatorTrigger("UpStrongAttack");
            }
            else
            {
                _owner.RPC_SetAnimatorTrigger("StrongAttack");
            }
        }
        
        ApplyRecoil(bombSpawnPoint, _strongRecoilForce, _yRecoilForce);
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    private GameObject InstantiateBomb(string prefabName, Transform bombSpawnPoint)
    {
        /*
        if(!PhotonNetwork.IsMasterClient)
        {
            return null;
        }*/
        GameObject bomb = ObjectPoolManager.Instance.GetObject(prefabName);
        bomb.transform.position = bombSpawnPoint.position;
        bomb.transform.rotation = Quaternion.Euler(0, _owner.PlayerStat.FacingDirection == 1 ? 0 : 180, 0);
        //GameObject bomb = PhotonNetwork.Instantiate(prefabName, bombSpawnPoint.position, Quaternion.Euler(0, _owner.PlayerStat.FacingDirection == 1 ? 0 : 180, 0));
        return bomb;
    }

    // 폭탄 반동 적용 함수
    protected virtual void ApplyRecoil(Transform bombSpawnPoint, float recoilPower = 5f, float upPower = 1f)
    {
        if (_owner.Rigidbody2D == null) return;
        // 폭탄 스폰 위치에서 플레이어까지의 방향 (x축 반대, y축 위)
        Vector2 dir = (_owner.transform.position - bombSpawnPoint.position).normalized;
        Vector2 recoil = new Vector2(dir.x, dir.y).normalized * recoilPower;
        recoil.y += upPower;
        _owner.Rigidbody2D.AddForce(recoil, ForceMode2D.Impulse);
    }
}
