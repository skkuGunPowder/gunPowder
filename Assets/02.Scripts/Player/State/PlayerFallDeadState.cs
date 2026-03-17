using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 플레이어 낙사 상태 클래스
/// 
/// 역할:
/// - 플레이어가 맵 밖으로 떨어졌을 때의 처리
/// - 곡선 경로를 따라 부활 지점으로 이동
/// - 낙사 중 시각적 효과 재생
/// - 부활 지점 도착 후 폭발 데미지 처리
/// 
/// 동작 방식:
/// 1. 떨어진 위치에 따라 좌우 구분 후 시작점 설정
/// 2. 시작점 → 중간점 → 부활지점 순으로 곡선 이동
/// 3. 이동 중 시각적 효과 반복 재생
/// 4. 부활 지점 도착 후 대기 시간 후 폭발 데미지
/// 5. 데미지 처리 후 일반 상태로 복귀
/// </summary>
public class PlayerFallDeadState : PlayerBaseState
{
    // 이동 관련 상수
    private const float TOTAL_MOVE_DURATION = 3.0f;         // 전체 이동 시간 (초)
    private const float TO_START_MOVE_DURATION = 0.6f;      // 시작점까지 선행 이동 시간 (초)
    private const float WAIT_DURATION_AT_GOAL = 1.0f;       // 부활 지점 도착 후 대기 시간 (초)

    // 효과 관련 상수
    private const float EFFECT_PLAY_INTERVAL = 0.5f;        // VFX 효과 재생 간격 (초)

    // 물리 관련 상수
    private const float MAX_FALL_SPEED = -20f;               // 최대 낙하 속도 (음수)

    // 데미지 관련 상수
    private const int FALL_DAMAGE_AMOUNT = 15;               // 낙사 시 받는 데미지
    private const int FALL_HEAL_PERCENT = 100;               // 낙사 시 받는 힐량

    private Vector3 _fallStartPoint;                         // 낙사 시작점
    private Vector3 _pathMiddlePoint;                        // 이동 경로 중간점
    private Vector3 _resurrectionEndPoint;                   // 부활 지점 (도착점)

    // 상태 관리 변수들
    private bool _hasReachedGoal = false;                    // 목표 지점 도달 여부
    private bool _hasTriggeredDeathEvents = false;          // 사망 이벤트 발생 여부 (중복 방지용)
    private bool _isInitialized = false;                     // 초기화 완료 여부

    // 타이머 변수들
    private float _waitTimer = 0f;                           // 대기 시간 측정용 타이머
    private float _effectTimer = 0f;                         // 효과 재생 간격 측정용 타이머

    // DOTween 관리
    private Tween _movementTween;                            // 이동 트윈 저장용

    /// <summary>
    /// 낙사 상태 진입 시 초기화 및 이동 경로 설정
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        // 
        _owner.SetPausedNoAttack();

        // 안전성 검사
        if (!ValidateGameManagerAndComponents())
        {
            return;
        }

        // 상태 초기화
        InitializeFallState();

        // 애니메이션 및 효과 시작
        StartAnimationAndEffects();

        // 무적 상태 설정
        SetImmuneState();

        // 이동 경로 계산 및 설정
        CalculateMovementPath();

        // DOTween 이동 시작
        StartMovementSequence();

        // 플레이어 모습 보이게 설정
        SetSpriteRenderersVisibility(true);

        _isInitialized = true;
    }

    /// <summary>
    /// 낙사 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        // 목표 지점에 도달했지만 아직 사망 이벤트가 발생하지 않았다면 강제로 실행
        if (_hasReachedGoal && !_hasTriggeredDeathEvents)
        {
            _hasTriggeredDeathEvents = true;
            ExecuteDeathEvents();
        }

        base.OnExit();

        //
        _owner.ResetPausedNoAttack();

        // 애니메이션 및 효과 정리
        CleanupAnimationAndEffects();

        // DOTween 이동 중단
        CleanupMovementTween();

        // 플레이어 상태 복원
        RestorePlayerState();

        // 플레이어 태그 복원
        RestorePlayerTag();

        // 물리 상태 초기화
        ResetPhysicsState();
    }

    /// <summary>
    /// 낙사 상태의 메인 업데이트 로직
    /// </summary>
    public override void Update()
    {
        // 초기화 체크
        if (!_isInitialized)
        {
            return;
        }

        // 목표 지점 도달 여부에 따른 처리 분기
        if (_hasReachedGoal)
        {
            HandleGoalReachedState();
        }
        else
        {
            HandleFallingState();
        }
    }

    /// <summary>
    /// Y축 낙하 속도 제한
    /// </summary>
    private void LimitYVelocity()
    {
        if (_owner.Rigidbody2D == null) return;

        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;

        // 낙하 속도 제한 (음수)
        if (velocity.y < MAX_FALL_SPEED)
        {
            velocity.y = MAX_FALL_SPEED;
        }

        _owner.Rigidbody2D.linearVelocity = velocity;
    }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// GameManager와 핵심 컴포넌트들의 유효성 검사
    /// </summary>
    private bool ValidateGameManagerAndComponents()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerFallDeadState] GameManager.Instance is null");
            return false;
        }

        if (_owner == null || _owner.PhotonView == null)
        {
            Debug.LogError("[PlayerFallDeadState] Owner or PhotonView is null");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 낙사 상태 초기화
    /// </summary>
    private void InitializeFallState()
    {
        _waitTimer = 0f;
        _effectTimer = 0f;
        _hasReachedGoal = false;
        _hasTriggeredDeathEvents = false;
        _owner.PlayerStat.IsFallingDead = true;
    }

    /// <summary>
    /// 애니메이션 및 효과 시작
    /// </summary>
    private void StartAnimationAndEffects()
    {
        _owner.RPC_SetAnimatorTrigger("HitLoop");

        // 네트워크 동기화된 효과 시작 (본인 클라이언트에서만)
        if (_owner.PhotonView.IsMine)
        {
            _owner.RPC_SetHitEffect(true);
            _owner.RPC_PlayFallDeadVFX();
        }
    }

    /// <summary>
    /// 무적 상태 설정
    /// </summary>
    private void SetImmuneState()
    {
        _owner.gameObject.tag = "Immune";

        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, true);
        }
    }

    /// <summary>
    /// 이동 경로 계산 및 설정
    /// </summary>
    private void CalculateMovementPath()
    {
        // 각 지점 설정
        SetMovementPoints();
    }

    /// <summary>
    /// 이동 지점들 설정
    /// </summary>
    private void SetMovementPoints()
    {
        FallDeadPathData fallDeadPathData = FallDeadPathManager.Instance.GetFallDeadPathData(_owner.transform.position.x);

        _fallStartPoint = fallDeadPathData.FallDeadStartPoint.position;
        _pathMiddlePoint = fallDeadPathData.FallDeadPath.position;
        _resurrectionEndPoint = fallDeadPathData.FallDeadEntPoint.position;
    }

    /// <summary>
    /// DOTween 이동 시퀀스 시작
    /// </summary>
    private void StartMovementSequence()
    {
        // 이동 경로 배열 생성
        Vector3[] movementPath = new Vector3[] { _fallStartPoint, _pathMiddlePoint, _resurrectionEndPoint };

        // DOTween 시퀀스 생성
        Sequence movementSequence = DOTween.Sequence();
        movementSequence.Append(_owner.transform.DOMove(_fallStartPoint, TO_START_MOVE_DURATION).SetEase(Ease.InOutSine));
        movementSequence.Append(_owner.transform.DOPath(movementPath, TOTAL_MOVE_DURATION, PathType.CatmullRom).SetEase(Ease.InOutSine));
        movementSequence.OnComplete(OnMovementComplete);

        _movementTween = movementSequence;
    }

    /// <summary>
    /// 이동 완료 시 콜백
    /// </summary>
    private void OnMovementComplete()
    {
        _hasReachedGoal = true;
        _owner.transform.position = _resurrectionEndPoint;
        ResetVelocity();
    }

    /// <summary>
    /// 스프라이트 렌더러들의 가시성 설정
    /// </summary>
    private void SetSpriteRenderersVisibility(bool isVisible)
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
    /// 애니메이션 및 효과 정리
    /// </summary>
    private void CleanupAnimationAndEffects()
    {
        _owner.RPC_ResetAnimatorTrigger("HitLoop");

        if (_owner.PhotonView.IsMine)
        {
            _owner.RPC_SetHitEffect(false);
        }
    }

    /// <summary>
    /// DOTween 이동 정리
    /// </summary>
    private void CleanupMovementTween()
    {
        if (_movementTween != null && _movementTween.IsActive())
        {
            _movementTween.Kill();
            _movementTween = null;
        }
    }

    /// <summary>
    /// 플레이어 상태 복원
    /// </summary>
    private void RestorePlayerState()
    {
        _owner.PlayerStat.IsImmune = false;
        _owner.PlayerStat.IsFallingDead = false;
        _hasTriggeredDeathEvents = false;
    }

    /// <summary>
    /// 플레이어 태그 복원
    /// </summary>
    private void RestorePlayerTag()
    {
        _owner.gameObject.tag = _owner.PhotonView.IsMine ? "Player" : "Enemy";
    }

    /// <summary>
    /// 물리 상태 초기화
    /// </summary>
    private void ResetPhysicsState()
    {
        ResetVelocity();
    }

    /// <summary>
    /// 목표 지점 도달 후 상태 처리
    /// </summary>
    private void HandleGoalReachedState()
    {
        // 위치 고정
        transform.position = _resurrectionEndPoint;

        // 속도 제한 적용
        LimitYVelocity();

        // 대기 시간 후 사망 이벤트 처리
        HandleDeathEventTiming();
    }

    /// <summary>
    /// 낙하 중 상태 처리
    /// </summary>
    private void HandleFallingState()
    {
        // 낙하 중 주기적으로 효과 재생
        HandleFallingEffects();
    }

    /// <summary>
    /// 사망 이벤트 타이밍 처리
    /// </summary>
    private void HandleDeathEventTiming()
    {
        if (_hasTriggeredDeathEvents) return;

        _waitTimer += Time.deltaTime;

        if (_waitTimer >= WAIT_DURATION_AT_GOAL)
        {
            _hasTriggeredDeathEvents = true;
            ExecuteDeathEvents();
        }
    }

    /// <summary>
    /// 낙하 중 효과 처리
    /// </summary>
    private void HandleFallingEffects()
    {
        _effectTimer += Time.deltaTime;

        if (_effectTimer >= EFFECT_PLAY_INTERVAL)
        {
            _effectTimer = 0f;
            PlayFallingEffect();
        }
    }

    /// <summary>
    /// 낙하 중 효과 재생
    /// </summary>
    private void PlayFallingEffect()
    {
        if (_owner.PhotonView.IsMine)
        {
            _owner.RPC_PlayFallDeadExplosionVFX();
        }
    }

    /// <summary>
    /// 사망 이벤트 실행 (폭발 + 데미지)
    /// </summary>
    private void ExecuteDeathEvents()
    {
        // 폭발 효과 생성
        CreateDeathExplosion();

        // 무적 해제 및 데미지 적용
        ApplyFallDamage();
    }

    /// <summary>
    /// 사망 폭발 효과 생성
    /// </summary>
    private void CreateDeathExplosion()
    {
        // 안전성 검사
        if (ExplosionPool.Instance == null || _owner.DieExplosionPrefab == null)
        {
            Debug.LogError("[PlayerFallDeadState] ExplosionPool or DieExplosionPrefab is null");
            return;
        }

        Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
        if (dieExplosion != null)
        {
            dieExplosion.transform.position = _owner.transform.position;
            dieExplosion.Explode(true, _owner.PhotonView);
        }
        else
        {
            Debug.LogWarning("[PlayerFallDeadState] 폭발 효과를 풀에서 가져오지 못함");
        }
    }

    /// <summary>
    /// 낙사 데미지 적용
    /// </summary>
    private void ApplyFallDamage()
    {
        // 무적 상태 해제
        _owner.PlayerStat.IsImmune = false;

        if (_owner.PhotonView.IsMine)
        {
            // 네트워크 동기화로 무적 상태 해제
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, false);

            EventManager.Instance.HitScreen();

            // 낙사 데미지 적용
            _owner.TakeDamage(
                FALL_DAMAGE_AMOUNT,
                FALL_DAMAGE_AMOUNT,
                FALL_HEAL_PERCENT,
                _owner.transform.position,
                _owner.GetComponent<PhotonView>().ViewID,
                _owner.GetComponent<PhotonView>().OwnerActorNr,
                0f,
                true,
                true
            );
        }
    }

    /// <summary>
    /// Rigidbody2D 속도 초기화
    /// </summary>
    private void ResetVelocity()
    {
        if (_owner.Rigidbody2D == null) return;

        _owner.Rigidbody2D.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// 낙사 상태에서의 피격 처리
    /// 슈퍼아머 상태여도 데미지 상태로 전환하도록 오버라이드
    /// </summary>
    protected override void HandleHit()
    {
        // 낙사 상태에서는 슈퍼아머를 무시하고 데미지 상태로 전환
        // 피가 50이하라면 히트스탑 처리
        // 아니라면 Damage 상태로
        if(_owner.PlayerStat.CurrentPlayerGunPowderCount <= _owner.PlayerStat.HitStopGunPowderCount)
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
}
