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

    public virtual void Update()
    {
        if(_owner.PhotonView.IsMine)
        {
            MineUpdate();
        }
    }

    /// <summary>
    /// 중요한 상태 변경을 네트워크로 동기화
    /// </summary>
    protected virtual void SyncStateChange<T>() where T : PlayerBaseState
    {
        Debug.Log($"SyncStateChange {typeof(T).Name}");
        if (_owner.PhotonView.IsMine)
        {
            Debug.Log($"SyncStateChange {typeof(T).Name} {_owner.PhotonView.IsMine}");
            _owner.PhotonView.RPC(nameof(_owner.RPC_ChangeState), RpcTarget.Others, typeof(T).Name);
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
            _owner.RPC_SetAnimatorTrigger("Jump");
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
        // 1. 폭탄 인스턴싱
        GameObject bomb = PhotonNetwork.Instantiate(prefabName, bombSpawnPoint.position,
         Quaternion.Euler(0, _owner.PlayerStat.FacingDirection == 1 ? 0 : 180, 0));
        
        if (bomb == null)
        {
            Debug.LogError($"Failed to instantiate bomb: {prefabName}");
            return;
        }
        
        // 2. Bomb 컴포넌트 가져오기
        Bomb bombComponent = bomb.GetComponent<Bomb>();
        if (bombComponent == null)
        {
            Debug.LogError($"Bomb component not found on instantiated object: {prefabName}");
            return;
        }
        
        // 3. Owner 설정
        PhotonView ownerPhotonView = _owner.GetComponent<PhotonView>();
        if (ownerPhotonView == null)
        {
            Debug.LogError("Owner PhotonView not found");
            return;
        }
        
        // 4. RPC 호출 (SetOwner 먼저, 그 다음 폭탄 동작)
        bombComponent.PhotonView.RPC(nameof(Bomb.SetOwner), RpcTarget.All, ownerPhotonView.ViewID);
        bombComponent.PhotonView.RPC(rpcMethodName, RpcTarget.All, rpcArgs);
    }

    // [리팩토링] 일반 폭탄 처리 메서드
    protected virtual void HandleNormalBomb(string action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();

        string methodName = action == "Place" ? nameof(Bomb.PlaceBomb)
                            : action == "Throw" ? nameof(Bomb.ThrowBomb)
                            : action == "ThrowStraight" ? nameof(Bomb.ThrowBombStraight)
                            : action == "Boost" ? nameof(Bomb.BoostBomb)
                            : null;

        SpawnAndRpcBomb(
            "BasicBomb",
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        switch (action)
        {
            case "ThrowStraight":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(Input.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(Input.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
                ApplyRecoil(bombSpawnPoint, _strongRecoilForce, _yRecoilForce);
                break;
            case "Throw":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpAttack");
                else
                    _owner.RPC_SetAnimatorTrigger("Attack");
                ApplyRecoil(bombSpawnPoint, _normalRecoilForce, _yRecoilForce);
                break;
            case "Place":
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            case "Boost":
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            default:
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
        }

        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastNormalBombTime();
    }

    // [리팩토링] 특수 폭탄 처리 메서드
    protected virtual void HandleSpecialBomb(string action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
            return;

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();

        string methodName = action == "Place" ? nameof(Bomb.PlaceBomb)
                            : action == "Throw" ? nameof(Bomb.ThrowBomb)
                            : action == "ThrowStraight" ? nameof(Bomb.ThrowBombStraight)
                            : action == "Boost" ? nameof(Bomb.BoostBomb)
                            : null;

        SpawnAndRpcBomb(
            "Missile",
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        switch (action)
        {
            case "ThrowStraight":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(Input.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(Input.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
                ApplyRecoil(bombSpawnPoint, _strongRecoilForce, _yRecoilForce);
                break;
            case "Throw":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpAttack");
                else
                    _owner.RPC_SetAnimatorTrigger("Attack");
                ApplyRecoil(bombSpawnPoint, _normalRecoilForce, _yRecoilForce);
                break;
            case "Place":
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            case "Boost":
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            default:
                _owner.RPC_SetAnimatorTrigger(_owner.PlayerStat.IsJumping ? "JumpAttack" : "Attack");
                break;
        }

        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    protected virtual void PlaceNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb("Place", spawnPoint);
    }

    protected virtual void ThrowNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb("Throw", spawnPoint);
    }
    protected virtual void ThrowStraightNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb("ThrowStraight", spawnPoint);
    }

    protected virtual void PlaceSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb("Place", spawnPoint);
    }

    protected virtual void ThrowSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb("Throw", spawnPoint);
    }

    protected virtual void ThrowStraightSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb("ThrowStraight", spawnPoint);
    }

    // 일반 폭탄 부스트
    protected virtual void BoostNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb("Boost", spawnPoint);  
    }

    // 특수 폭탄 부스트
    protected virtual void BoostSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb("Boost", spawnPoint);
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
