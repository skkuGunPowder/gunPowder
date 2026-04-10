using System;
using System.Collections.Generic;
using Photon.Pun;
using RaycastPro.RaySensors2D;
using RobustFSM.Base;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerBaseState : MonoState
{
    protected PlayerFSM _playerFSM;
    protected Player _owner;
    protected CameraController _cameraController;
    protected BoxRay2D _groundRay2D;

    // 반동 관련 상수
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

    // 사망 효과 관련 상수
    private const float DIE_EFFECT_FORCE = 30f;            // 사망 시 신체 부위에 가해지는 힘
    
    // 방향 벡터 상수들
    private static readonly Vector2 HEAD_DIRECTION = new Vector2(0, 1).normalized;       // 머리 부위 방향 (위)
    private static readonly Vector2 BODY_DIRECTION = new Vector2(0, -1).normalized;     // 몸통 부위 방향 (아래)
    private static readonly Vector2 LEFT_ARM_DIRECTION = new Vector2(-1, 1).normalized; // 왼팔 방향 (왼쪽 위)
    private static readonly Vector2 LEFT_LEG_DIRECTION = new Vector2(-1, -1).normalized;// 왼다리 방향 (왼쪽 아래)
    private static readonly Vector2 RIGHT_ARM_DIRECTION = new Vector2(1, 1).normalized; // 오른팔 방향 (오른쪽 위)
    private static readonly Vector2 RIGHT_LEG_DIRECTION = new Vector2(1, -1).normalized;// 오른다리 방향 (오른쪽 아래)
    
    public override void OnEnter()
    {
        base.OnEnter();
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;
        _groundRay2D = _owner.GroundRay2D;

        
        // 사망상태에서는 히트에 의한 상태 전환을 막기 위해 구독하지 않음
        if (_playerFSM.IsCurrentState<PlayerDieState>() || _playerFSM.IsCurrentState<PlayerLastDieState>())
        {
            return;
        }
        
        PlayerEventManager.Instance.GetEvents(_owner.ActorNumber).OnHit += HandleHit;
    }

    public override void OnExit()
    {
        base.OnExit();

        PlayerEventManager.Instance.GetEvents(_owner.ActorNumber).OnHit -= HandleHit;
    }

    protected virtual void HandleHit()
    {
        // 슈퍼아머가 활성화되어 있으면 DamagedState로 들어가지 않음
        if (_owner.IsSuperArmorEnabled)
        {
            // 슈퍼아머: 데미지는 받지만 DamagedState로 전환하지 않음
            // 짧은 무적 시간만 설정하여 연속 피격 방지
            SuperArmorImmuneCoroutine().Forget();
            return;
        }

        // 피가 50이하라면 히트스탑 처리
        // 아니라면 Damage 상태로
        if(_owner.PlayerStat.CurrentHP <= _owner.PlayerStat.HitStopGunPowderCount)
        {
            // 이미 DamagedState이고 히트스탑이 활성화되어 있으면 추가 히트 처리
            if (_playerFSM.IsCurrentState<PlayerDamagedState>())
            {
                PlayerDamagedState currentDamagedState = _playerFSM.GetCurrentState<PlayerDamagedState>();
                if (currentDamagedState != null && currentDamagedState.IsHitStopActive())
                {
                    currentDamagedState.OnAdditionalHit();
                }
                else
                {
                    // DamagedState에 있지만 히트스탑이 비활성화된 상태면 새로운 DamagedState로 전환 (히트스탑 활성화)
                    PlayerDamagedState.SetPendingHitStop(true);
                    SyncStateChange<PlayerDamagedState>();
                }
            }
            else
            {
                // 다른 상태에서 피격 시 DamagedState로 전환 (히트스탑 활성화)
                PlayerDamagedState.SetPendingHitStop(true);
                SyncStateChange<PlayerDamagedState>();
            }
        }
        else
        {
            // 피가 50 초과면 일반 DamagedState로 전환 (히트스탑 비활성화)
            PlayerDamagedState.SetPendingHitStop(false);
            SyncStateChange<PlayerDamagedState>();
        }
    }

    /// <summary>
    /// 슈퍼아머 상태에서 짧은 무적 시간 설정 (연속 피격 방지)
    /// </summary>
    private async Cysharp.Threading.Tasks.UniTask SuperArmorImmuneCoroutine()
    {
        // 무적 상태 설정 (짧은 시간만)
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;
        
        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, true);
        }

        // 0.1초 무적 시간 (연속 피격 방지용)
        await Cysharp.Threading.Tasks.UniTask.WaitForSeconds(0.1f);

        // 무적 상태 해제
        _owner.gameObject.tag = _owner.PhotonView.IsMine ? "Player" : "Enemy";
        _owner.PlayerStat.IsImmune = false;
        
        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, false);
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

    protected virtual bool CanZSlotBomb()
    {
        return _owner.CanZSlotBomb();
    }

    protected virtual bool CanXSlotBomb()
    {
        if (!_owner.CanXSlotBomb())
        {
            return false;
        }

        if (_owner.PlayerStat.CurrentHP <= _owner.XSlotBombStat.Cost)
        {
            return false;
        }

        return true;
    }

    protected virtual void SetLastZSlotBombTime()
    {
        _owner.SetLastZSlotBombTime();
    }

    protected virtual void SetLastXSlotBombTime()
    {
        _owner.SetLastXSlotBombTime();
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

        // 특수 폭탄일 경우 건파우더 소모
        // 머리 폭탄은 일반 폭탄의 변형이므로 건파우더를 소모하지 않음
        if(prefabName != BASIC_BOMB_PREFAB)
        {
            // 머리 폭탄인지 확인 (건파우더 소모 제외)
            bool isHeadBomb = _owner.HeadBombPrefab != null && prefabName == _owner.HeadBombPrefab.name;
            
            if (!isHeadBomb)
            {
                // 건파우더가 부족한 경우 폭탄 생성 중단
                if (_owner.PlayerStat.CurrentHP <= _owner.XSlotBombStat.Cost)
                {
                    return;
                }
                
                int cost = _owner.XSlotBombStat.Cost;
                _owner.PlayerStat.DecreaseHP(cost, _owner.PhotonView.Owner.ActorNumber);
                
                // 건파우더 소모 파티클 생성 (모든 클라이언트에게 표시)
                _owner.RPC_SpawnGunPowderUseParticle();
            }
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
        
        // 4. RPC 호출 (SetOwner 먼저, 그 다음 폭탄 동작)
        try
        {      
            // 다른 클라이언트에게 SetOwner 전파
            bombComponent.PhotonView.RPC(nameof(Bomb.SetOwner), RpcTarget.All, ownerPhotonView.ViewID);
            bombComponent.PhotonView.RPC(rpcMethodName, RpcTarget.All, rpcArgs);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerBaseState] RPC 호출 중 오류 발생: {e.Message}");
            PhotonNetwork.Destroy(bomb); // 실패한 오브젝트 정리
        }
    }

    //일반 폭탄 처리 메서드
    protected virtual void HandleNormalBomb(BombActionType action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
        {
            return;
        }

        // 공통 처리
        ExecuteAttackCommonLogic();

        // 일반 공격 이벤트
        _owner.InvokeNormalAttack();
        
        // 스폰 포인트 설정
        (Transform bombSpawnPoint, EBombSpawnPoint finalSpawnPoint) = GetBombSpawnPointInfo(spawnPoint);
        
        // 프리팹 이름 결정
        string prefabName = GetNormalBombPrefabName(action, finalSpawnPoint);
        
        // 폭탄 생성 및 실행
        ExecuteBombAction(prefabName, bombSpawnPoint, action, _owner.ZSlotBombStat);

        // 후처리
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastZSlotBombTime();
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

    //특수 폭탄 처리 메서드
    protected virtual void HandleSpecialBomb(BombActionType action, EBombSpawnPoint? spawnPoint = null)
    {
        if(!_owner.PhotonView.IsMine)
            return;

        // 에어드롭 아이템 사용 체크
        // if (TryUseAirDropItem())
        // {
        //     return;
        // }

        // 특수 폭탄 사용 가능 여부 체크
        if (!CanXSlotBomb())
        {
            return;
        }

        // SubBomb 장착 시 SubBomb 사용, 없으면 메인 폭탄으로 대체
        string prefabName = _owner.EquipedItemDict.TryGetValue(EItemType.SubBomb, out ItemDTO subBomb) && subBomb?.Prefab != null
            ? subBomb.Prefab.name
            : _owner.EquipedItemDict[EItemType.Bomb].Prefab.name;
        
        // 박격포는 지상에서만 사용 가능
        if (IsMortarBomb(prefabName) && !CanUseMortarInCurrentState())
        {
            Debug.Log("[PlayerBaseState] 박격포는 공중에서 사용할 수 없습니다.");
            return;
        }

        // 공통 처리
        ExecuteAttackCommonLogic();

        // 특수 공격 이벤트
        _owner.InvokeSpecialAttack();
        
        // 스폰 포인트 설정
        (Transform bombSpawnPoint, EBombSpawnPoint finalSpawnPoint) = GetBombSpawnPointInfo(spawnPoint);
        
        // 폭탄 생성 및 실행
        ExecuteBombAction(prefabName, bombSpawnPoint, action, _owner.XSlotBombStat);

        // 후처리
        ResetGunPowderDecreaseWithoutAttackTimer();
        SetLastXSlotBombTime();
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
    // protected virtual bool TryUseAirDropItem()
    // {
    //     if (_owner.AirDropItem != null && _owner.AirDropItemLootVFX.IsSelected)
    //     {
    //         _owner.AirDropItemLootVFX.UseItem();
    //         _owner.AirDropItem.Use();
    //         _owner.RemoveAirDropItem();
    //         return true;
    //     }
    //     return false;
    // }

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
        // 머리 위로 던지기: 헤드봄
        if(spawnPoint == EBombSpawnPoint.Up && action == BombActionType.ThrowStraight)
        {
            _owner.RPC_HeadSpriteOnOff();
            return _owner.HeadBombPrefab.name;
        }

        // 장착된 메인 폭탄 반환 (없으면 BasicBomb 폴백)
        if (_owner.EquipedItemDict.TryGetValue(EItemType.Bomb, out ItemDTO bomb) && bomb?.Prefab != null)
            return bomb.Prefab.name;

        string prefabName = BASIC_BOMB_PREFAB;

        return prefabName;
    }

    /// <summary>
    /// 폭탄 액션 실행 (생성, RPC 호출, 애니메이션)
    /// </summary>
    protected virtual void ExecuteBombAction(string prefabName, Transform bombSpawnPoint, BombActionType action, BombStat bombStat)
    {
        string methodName = GetBombMethodName(action);

        SpawnAndRpcBomb(
            prefabName,
            bombSpawnPoint,
            methodName,
            new object[] { bombSpawnPoint.right, bombSpawnPoint.up, bombSpawnPoint.forward }
        );

        PlayBombAnimation(action, bombSpawnPoint, bombStat);
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
    protected virtual void PlayBombAnimation(BombActionType action, Transform bombSpawnPoint, BombStat bombStat)
    {
        switch (action)
        {
            case BombActionType.ThrowStraight:
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "JumpUpStrongAttack" : "JumpStrongAttack");
                else
                    _owner.RPC_SetAnimatorTrigger(InputHandler.GetKey(KeyCode.UpArrow) ? "UpStrongAttack" : "StrongAttack");
                ApplyRecoil(bombSpawnPoint, bombStat.RecoilAmount, Y_RECOIL_FORCE);
                break;
            case BombActionType.Throw:
                if (_owner.PlayerStat.IsJumping)
                    _owner.RPC_SetAnimatorTrigger("JumpAttack");
                else
                    _owner.RPC_SetAnimatorTrigger("Attack");
                ApplyRecoil(bombSpawnPoint, bombStat.RecoilAmount, Y_RECOIL_FORCE);
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
    
    /// <summary>
    /// 신체 부위별 사망 효과 적용 (각 부위를 다른 방향으로 흩어뜨림)
    /// </summary>
    private void ApplyBodyPartsDeathEffect()
    {
        // 각 신체 부위별로 지정된 방향으로 힘 적용
        ApplyForceToBodyParts(_owner.HeadPartList, HEAD_DIRECTION);
        ApplyForceToBodyParts(_owner.BodyPartList, BODY_DIRECTION);
        ApplyForceToBodyParts(_owner.LeftArmPartList, LEFT_ARM_DIRECTION);
        ApplyForceToBodyParts(_owner.LeftLegPartList, LEFT_LEG_DIRECTION);
        ApplyForceToBodyParts(_owner.RightArmPartList, RIGHT_ARM_DIRECTION);
        ApplyForceToBodyParts(_owner.RightLegPartList, RIGHT_LEG_DIRECTION);
    }

    /// <summary>
    /// 특정 신체 부위 리스트에 힘 적용
    /// </summary>
    private void ApplyForceToBodyParts(List<GameObject> bodyParts, Vector2 direction)
    {
        if (bodyParts == null) return;

        foreach (GameObject bodyPart in bodyParts)
        {
            ApplyForceToSingleBodyPart(bodyPart, direction);
        }
    }

    /// <summary>
    /// 개별 신체 부위에 힘 적용
    /// </summary>
    private void ApplyForceToSingleBodyPart(GameObject bodyPart, Vector2 direction)
    {
        if (bodyPart == null) return;

        Rigidbody2D rigidbody2D = bodyPart.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            bodyPart.SetActive(true);
            rigidbody2D.AddForce(direction * DIE_EFFECT_FORCE, ForceMode2D.Impulse);
        }
    }

    protected void ExecuteDeathEffects()
    {
        // 신체 부위 흩어지는 효과
        ApplyBodyPartsDeathEffect();

        // 사망 폭발 효과
        CreateDeathExplosion();

        // 플레이어 모습 숨기기
        SetSpriteRenderersVisibility(false);

        // 사망 사운드 재생
        // PlayDeathSound();
        
    }
    
    /// <summary>
    /// 사망 폭발 효과 생성
    /// </summary>
    private void CreateDeathExplosion()
    {
        Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
        dieExplosion.transform.position = _owner.transform.position;
        dieExplosion.Explode(true, _owner.PhotonView);
    }
    
    /// <summary>
    /// 스프라이트 렌더러들의 가시성 설정
    /// </summary>
    protected void SetSpriteRenderersVisibility(bool isVisible)
    {
        List<SpriteRenderer> spriteRenderers = _owner.PlayerStat.MySpriteREndererList;
        if (spriteRenderers == null) return;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isVisible;
            }
        }
    }
    
    /// <summary>
    /// 박격포 폭탄인지 확인
    /// </summary>
    private bool IsMortarBomb(string prefabName)
    {
        return prefabName.Contains("Mortar") || prefabName == "BM0002";
    }
    
    /// <summary>
    /// 현재 상태에서 박격포 사용 가능한지 확인 (지상에서만 가능)
    /// </summary>
    private bool CanUseMortarInCurrentState()
    {
        return !(_playerFSM.IsCurrentState<PlayerJumpState>() || 
                 _playerFSM.IsCurrentState<PlayerFallState>() || 
                 _playerFSM.IsCurrentState<PlayerJumpDashState>());
    }
    
    /// <summary>
    /// 사망 사운드 재생
    /// </summary>
    private void PlayDeathSound()
    {
        SoundManager.Instance.PlayLocalRandomSound("PlayerDeath", transform, 1, 2);
    }

}
