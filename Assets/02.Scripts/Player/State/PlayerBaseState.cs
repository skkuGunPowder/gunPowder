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

    // 반동 관련 상수
    protected const float NORMAL_RECOIL_FORCE = 10f;
    protected const float STRONG_RECOIL_FORCE = 20f;
    protected const float Y_RECOIL_FORCE = 5f;
    
    // 프리팹 이름 상수
    protected const string BASIC_BOMB_PREFAB = "BasicBomb";

    // 폭탄 액션 타입 열거형
    protected enum BombActionType
    {
        Place,
        Throw,
        ThrowStraight,
        Boost
    }






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
                // 코요테 타임: 걷기 상태에서 잠깐 떠 있어도 점프 허용
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
        // 입력 검증
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("[PlayerBaseState] 프리팹 이름이 null이거나 비어있습니다.");
            return;
        }
        
        if (bombSpawnPoint == null)
        {
            Debug.LogError("[PlayerBaseState] 폭탄 스폰 포인트가 null입니다.");
            return;
        }
        
        if (string.IsNullOrEmpty(rpcMethodName))
        {
            Debug.LogError("[PlayerBaseState] RPC 메서드 이름이 null이거나 비어있습니다.");
            return;
        }

        // 1. 폭탄 인스턴싱
        GameObject bomb = PhotonNetwork.Instantiate(prefabName, bombSpawnPoint.position,
         Quaternion.Euler(0, _owner.PlayerStat.FacingDirection == 1 ? 0 : 180, 0));
        
        if (bomb == null)
        {
            Debug.LogError($"[PlayerBaseState] 폭탄 생성에 실패했습니다: {prefabName}");
            return;
        }
        
        // 2. Bomb 컴포넌트 가져오기
        Bomb bombComponent = bomb.GetComponent<Bomb>();
        if (bombComponent == null)
        {
            Debug.LogError($"[PlayerBaseState] Bomb 컴포넌트를 찾을 수 없습니다: {prefabName}");
            PhotonNetwork.Destroy(bomb); // 실패한 오브젝트 정리
            return;
        }
        
        // 3. Owner 설정
        PhotonView ownerPhotonView = _owner.GetComponent<PhotonView>();
        if (ownerPhotonView == null)
        {
            Debug.LogError("[PlayerBaseState] Owner PhotonView를 찾을 수 없습니다.");
            PhotonNetwork.Destroy(bomb); // 실패한 오브젝트 정리
            return;
        }
        
        // 특수 폭탄일 경우 건파우더 소모
        if(prefabName != BASIC_BOMB_PREFAB)
        {
            _owner.PlayerStat.DecreaseGunPowderCount(_owner.SpecialBombStat.Cost, _owner.PhotonView.Owner.ActorNumber);
        }
        
        // 4. RPC 호출 (SetOwner 먼저, 그 다음 폭탄 동작)
        try
        {
            bombComponent.PhotonView.RPC(nameof(Bomb.SetOwner), RpcTarget.All, ownerPhotonView.ViewID);
            bombComponent.PhotonView.RPC(rpcMethodName, RpcTarget.All, rpcArgs);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerBaseState] RPC 호출 중 오류 발생: {e.Message}");
            PhotonNetwork.Destroy(bomb); // 실패한 오브젝트 정리
        }
    }

    // [리팩토링] 일반 폭탄 처리 메서드
    protected virtual void HandleNormalBomb(BombActionType action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        // 공통 처리
        ExecuteAttackCommonLogic();
        
        // 스폰 포인트 설정
        (Transform bombSpawnPoint, EBombSpawnPoint finalSpawnPoint) = GetBombSpawnPointInfo(spawnPoint);
        
        // 프리팹 이름 결정
        string prefabName = GetNormalBombPrefabName(action, finalSpawnPoint);
        
        // 폭탄 생성 및 실행
        ExecuteBombAction(prefabName, bombSpawnPoint, action);

        // 후처리
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
    protected virtual void HandleSpecialBomb(BombActionType action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
            return;

        // 에어드롭 아이템 사용 체크
        if (TryUseAirDropItem())
        {
            return;
        }

        // 공통 처리
        ExecuteAttackCommonLogic();
        
        // 스폰 포인트 설정
        (Transform bombSpawnPoint, EBombSpawnPoint finalSpawnPoint) = GetBombSpawnPointInfo(spawnPoint);
        
        // 특수 폭탄 프리팹 이름 가져오기
        string prefabName = _owner.EquipedItemDict[EItemType.Bomb].Prefab.name;
        
        // 폭탄 생성 및 실행
        ExecuteBombAction(prefabName, bombSpawnPoint, action);

        // 후처리
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastSpecialBombTime();
    }

    protected virtual void PlaceNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb(BombActionType.Place, spawnPoint);
    }

    protected virtual void ThrowNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb(BombActionType.Throw, spawnPoint);
    }
    protected virtual void ThrowStraightNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb(BombActionType.ThrowStraight, spawnPoint);
    }

    protected virtual void PlaceSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb(BombActionType.Place, spawnPoint);
    }

    protected virtual void ThrowSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb(BombActionType.Throw, spawnPoint);
    }

    protected virtual void ThrowStraightSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb(BombActionType.ThrowStraight, spawnPoint);
    }

    // 일반 폭탄 부스트
    protected virtual void BoostNormalBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleNormalBomb(BombActionType.Boost, spawnPoint);  
    }

    // 특수 폭탄 부스트
    protected virtual void BoostSpecialBomb(EBombSpawnPoint? spawnPoint = null)
    {
        HandleSpecialBomb(BombActionType.Boost, spawnPoint);
    }


    /// <summary>
    /// 공격 시 공통 로직 실행 (이벤트 발생, 사운드 재생)
    /// </summary>
    protected virtual void ExecuteAttackCommonLogic()
    {
        _owner.InvokeAttack();
        SoundManager.Instance.PlayLocalRandomSound("PlayerShot", transform, 1, 2);
    }

    /// <summary>
    /// 에어드롭 아이템 사용 시도
    /// </summary>
    protected virtual bool TryUseAirDropItem()
    {
        if (_owner.AirDropItem != null && _owner.AirDropItemLootVFX.IsSelected)
        {
            _owner.AirDropItemLootVFX.UseItem();
            _owner.AirDropItem.Use();
            _owner.RemoveAirDropItem();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 폭탄 스폰 포인트 정보 가져오기
    /// </summary>
    protected virtual (Transform bombSpawnPoint, EBombSpawnPoint finalSpawnPoint) GetBombSpawnPointInfo(EBombSpawnPoint? spawnPoint)
    {
        Transform bombSpawnPoint = spawnPoint.HasValue
            ? _owner.GetBombSpawnPoint(spawnPoint.Value)
            : _owner.GetBombSpawnPoint();
        
        EBombSpawnPoint finalSpawnPoint = spawnPoint ?? _owner.GetBombSpawnInfo().point;
        
        return (bombSpawnPoint, finalSpawnPoint);
    }

    /// <summary>
    /// 일반 폭탄 프리팹 이름 결정
    /// </summary>
    protected virtual string GetNormalBombPrefabName(BombActionType action, EBombSpawnPoint spawnPoint)
    {
        string prefabName = BASIC_BOMB_PREFAB;
        
        if(spawnPoint == EBombSpawnPoint.Up && action == BombActionType.ThrowStraight)
        {
            prefabName = _owner.HeadBombPrefab.name;
            _owner.RPC_HeadSpriteOnOff();
        }
        
        return prefabName;
    }

    /// <summary>
    /// 폭탄 액션 실행 (생성, RPC 호출, 애니메이션)
    /// </summary>
    protected virtual void ExecuteBombAction(string prefabName, Transform bombSpawnPoint, BombActionType action)
    {
        string methodName = GetBombMethodName(action);
        
        SpawnAndRpcBomb(
            prefabName,
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        PlayBombAnimation(action, bombSpawnPoint);
    }

    /// <summary>
    /// 액션에 따른 폭탄 메서드 이름 반환
    /// </summary>
    protected virtual string GetBombMethodName(BombActionType action)
    {
        switch (action)
        {
            case BombActionType.Place:
                return nameof(Bomb.PlaceBomb);
            case BombActionType.Throw:
                return nameof(Bomb.ThrowBomb);
            case BombActionType.ThrowStraight:
                return nameof(Bomb.ThrowBombStraight);
            case BombActionType.Boost:
                return nameof(Bomb.BoostBomb);
            default:
                Debug.LogWarning("[PlayerBaseState] 알 수 없는 폭탄 액션: " + action);
                return nameof(Bomb.PlaceBomb); // 기본값으로 Place 사용
        }
    }

    /// <summary>
    /// 폭탄 액션에 따른 애니메이션 및 반동 처리
    /// </summary>
    protected virtual void PlayBombAnimation(BombActionType action, Transform bombSpawnPoint)
    {
        switch (action)
        {
            case BombActionType.ThrowStraight:
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
                ApplyRecoil(bombSpawnPoint, STRONG_RECOIL_FORCE, Y_RECOIL_FORCE);
                break;
            case BombActionType.Throw:
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpAttack");
                else
                    _owner.RPC_SetAnimatorTrigger("Attack");
                ApplyRecoil(bombSpawnPoint, NORMAL_RECOIL_FORCE, Y_RECOIL_FORCE);
                break;
            case BombActionType.Place:
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpDropAttack");
                else
                    _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            case BombActionType.Boost:
                _owner.RPC_SetAnimatorTrigger("PlaceAttack");
                break;
            default:
                _owner.RPC_SetAnimatorTrigger(_owner.PlayerStat.IsJumping ? "JumpAttack" : "Attack");
                break;
        }
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
