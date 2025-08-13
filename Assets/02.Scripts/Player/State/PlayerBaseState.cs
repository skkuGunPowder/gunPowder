using System;
using Photon.Pun;
using RaycastPro.RaySensors2D;
using RobustFSM.Base;
using UnityEngine;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;
    protected CameraController _cameraController;
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

        
        // 사망상태에서는 히트에 의한 상태 전환을 막기 위해 구독하지 않음
        if (!_playerFSM.IsCurrentState<PlayerDieState>())
        {
            _owner.OnHit += HandleHit;
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        _owner.OnHit -= HandleHit;
    }

    protected virtual void HandleHit()
    {
        // 피가 50이하라면 히트스탑 상태로
        // 아니라면 Damage 상태로
        if(_owner.PlayerStat.CurrentPlayerGunPowderCount <= _owner.PlayerStat.HitStopGunPowderCount)
        {
            // 이미 히트스탑 상태라면 추가 히트 처리
            if (_playerFSM.IsCurrentState<PlayerHitStopState>())
            {
                PlayerHitStopState currentHitStopState = _playerFSM.GetCurrentState<PlayerHitStopState>();
                if (currentHitStopState != null)
                {
                    currentHitStopState.OnAdditionalHit();
                }
            }
            else
            {
                // 새로운 히트스탑 상태로 전환
                SyncStateChange<PlayerHitStopState>();
            }
        }
        else
        {
            SyncStateChange<PlayerDamagedState>();
        }

        
    }

    public virtual void MineUpdate()
    {
        JumpInput();
        UltimateInput();
    }

    public virtual void Update()
    {
        if(_owner.PhotonView.IsMine)
        {
            MineUpdate();
        }
    }

    protected virtual void UltimateInput()
    {
        if(InputHandler.GetKeyDown(KeyCode.C))
        {
            _owner.ExecuteUltimate();
        }
    }

    /// <summary>
    /// 중요한 상태 변경을 네트워크로 동기화
    /// </summary>
    protected virtual void SyncStateChange<T>() where T : PlayerBaseState
    {
        if (_owner.PhotonView.IsMine)
        {
            // 모든 클라이언트에서 상태 변경 (자신 포함)
            _owner.PhotonView.RPC(nameof(_owner.RPC_ChangeState), RpcTarget.All, typeof(T).Name);
        }
    }
    // 하위에서 사용하고 싶은 것만 사용한다.
    protected virtual void JumpInput()
    {
        if (_playerFSM.IsCurrentState<PlayerDashState>() 
        || _playerFSM.IsCurrentState<PlayerJumpDashState>())
            return;

        if (InputHandler.GetKeyDown(KeyCode.Space) && _owner.PlayerStat.CanJump())
        {
            if(InputHandler.GetKey(KeyCode.DownArrow) && IsOneWayPlatform())
            {
                _owner.PlayerStat.IsDownJump = true;
                _playerFSM.ChangeState<PlayerFallState>();
                _owner.SetDownJump();
            }
            else if(InputHandler.GetKey(KeyCode.DownArrow))
            {
            }
            else
            {
                _playerFSM.ChangeState<PlayerJumpState>();
            }
            
        }
    }

    protected virtual bool IsOneWayPlatform()
    {
        if (!_owner.PhotonView.IsMine)
        {
            return false;
        }
        if(_groundRay2D == null)
        {
            return false;
        }

        // 레이캐스트 실행
        _groundRay2D.Cast();
        
        // 레이캐스트가 성공했는지 확인
        if (!_groundRay2D.Performed)
        {
            return false;
        }

        // 레이캐스트로 탐지된 오브젝트의 태그 확인
        RaycastHit2D hit = _groundRay2D.Hit;
        if (hit.collider != null)
        {
            // OneWayPlatform 태그인지 확인
            return hit.collider.CompareTag("OneWayPlatform");
        }

        return false;
    }

    // 2D Raycast로 바닥 체크
    protected virtual bool IsGrounded2D()
    {
        // 레이캐스트는 로컬 플레이어에서만 실행
        if (!_owner.PhotonView.IsMine)
        {
            return false;
        }

        if(_groundRay2D == null)
        {
            return false;
        }

        _groundRay2D.Cast();
        bool isGrounded = _groundRay2D.Performed;
        
        return isGrounded;
    }

    protected virtual bool CanNormalBomb()
    {
        return _owner.CanNormalBomb();
    }

    protected virtual bool CanSpecialBomb()
    {
        return _owner.CanSpecialBomb();
    }

    protected virtual void SetLastNormalBombTime()
    {
        _owner.SetLastNormalBombTime();
    }

    protected virtual void SetLastSpecialBombTime()
    {
        _owner.SetLastSpecialBombTime();
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
            return;
        }
        
        // 2. Bomb 컴포넌트 가져오기
        Bomb bombComponent = bomb.GetComponent<Bomb>();
        if (bombComponent == null)
        {
            return;
        }
        
        // 3. Owner 설정
        PhotonView ownerPhotonView = _owner.GetComponent<PhotonView>();
        if (ownerPhotonView == null)
        {
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

        // 공격 이벤트 발생
        _owner.InvokeAttack();
        SoundManager.Instance.PlayLocalRandomSound("PlayerShot", transform, 1, 2);

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        if(!spawnPoint.HasValue)
        {
            spawnPoint = _owner.GetBombSpawnInfo().point;
        }

        string methodName = action == "Place" ? nameof(Bomb.PlaceBomb)
                            : action == "Throw" ? nameof(Bomb.ThrowBomb)
                            : action == "ThrowStraight" ? nameof(Bomb.ThrowBombStraight)
                            : action == "Boost" ? nameof(Bomb.BoostBomb)
                            : null;
        string prefabName = "BasicBomb";

        
        if(spawnPoint == EBombSpawnPoint.Up && action == "ThrowStraight")
        {
            prefabName = _owner.HeadBombPrefab.name;
            _owner.RPC_HeadSpriteOnOff();
        }

        SpawnAndRpcBomb(
            prefabName,
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        switch (action)
        {
            case "ThrowStraight":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
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
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpDropAttack");
                else
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

    /// <summary>
    /// 방향키(좌/우/상/하) 또는 축 입력이 있는지 여부를 반환한다.
    /// </summary>
    protected bool HasDirectionalInput()
    {
        if (InputHandler.GetKey(KeyCode.LeftArrow) || InputHandler.GetKey(KeyCode.RightArrow)
            || InputHandler.GetKey(KeyCode.UpArrow) || InputHandler.GetKey(KeyCode.DownArrow))
        {
            return true;
        }
        float h = InputHandler.GetAxisRaw("Horizontal");
        float v = InputHandler.GetAxisRaw("Vertical");
        return Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
    }

    // [리팩토링] 특수 폭탄 처리 메서드
    protected virtual void HandleSpecialBomb(string action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
            return;

        // 공격 이벤트 발생
        _owner.InvokeAttack();
        SoundManager.Instance.PlayLocalRandomSound("PlayerShot", transform, 1, 2);

        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();

        string methodName = action == "Place" ? nameof(Bomb.PlaceBomb)
                            : action == "Throw" ? nameof(Bomb.ThrowBomb)
                            : action == "ThrowStraight" ? nameof(Bomb.ThrowBombStraight)
                            : action == "Boost" ? nameof(Bomb.BoostBomb)
                            : null;


        SpawnAndRpcBomb(
            _owner.EquipedItemDict[EItemType.Bomb].Prefab.name,
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        switch (action)
        {
            case "ThrowStraight":
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
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
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpDropAttack");
                else
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
