using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 플레이어 피격 상태 클래스
/// 
/// 역할:
/// - 플레이어가 피격당했을 때의 상태 처리
/// - 피격 시 무적 시간, 넉백 효과, 체력 비례 추가 힘 적용
/// - 피격 애니메이션 및 이펙트 관리
/// 
/// 동작 방식:
/// 1. 피격 시 무적 상태로 전환 및 태그 변경
/// 2. 체력 비율에 따른 넉백 효과 적용
/// 3. 히트 이펙트 활성화 및 방향 설정
/// 4. 최소 피격 시간 후 바닥 착지 시 Idle 상태로 전환
/// </summary>
public class PlayerDamagedState : PlayerBaseState
{
    // 넉백 효과 관련 상수
    private const float MIN_LINEAR_DAMPING = 0.01f;        // 최소 선형 감쇠값
    private const float MAX_LINEAR_DAMPING = 2.5f;         // 최대 선형 감쇠값
    private const float HEALTH_RATIO_THRESHOLD = 0.5f;     // 체력 비율 임계값 (50%)
    private const float DAMPING_LERP_START = 0.5f;         // 감쇠 보간 시작값
    private const float MIN_KNOCKBACK_RATIO = 0.5f;        // 최소 넉백 비율 (멀리서 맞았을 때 50%만 날아감)
    
    // 추가 힘 관련 상수
    private const float MAX_ADDITIONAL_FORCE = 10f;        // 최대 추가 힘
    private const float UPWARD_FORCE = 10f;                // 위쪽 힘
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;     // 최소 속도 임계값
    
    // 히트 이펙트 관련 상수
    private const float HIT_EFFECT_DURATION = 0.5f;        // 히트 이펙트 지속 시간
    private const float DEFAULT_HIT_EFFECT_ANGLE = 90f;    // 기본 히트 이펙트 각도
    
    // 상태 변수들
    private float _damagedTimer = 0f;           // 피격 지속 시간 타이머
    private float _actualDamagedTime = 0f;      // 실제 피격 시간 (거리 기반으로 조정된 값)
    private float _originalLinearDamping;       // 원본 선형 감쇠값
    private Tween _knockbackTween;              // 넉백 효과 트윈

    /// <summary>
    /// 피격 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        _damagedTimer = 0f;
        
        // 거리 기반 실제 피격 시간 계산
        CalculateActualDamagedTime();
        
        _owner.RPC_SetAnimatorTrigger("HitLoop");
        
        // 무적 상태 설정
        SetImmuneState(true);
        
        // 저장된 속도 복원 (히트스탑에서 온 경우)
        RestoreStoredVelocityIfExists();
        
        // 체력 비례 추가 힘 적용
        ApplyHealthBasedForce();
        
        // 체력 비율에 따른 넉백 효과 적용
        float currentHealthRatio = CalculateCurrentHealthRatio();
        ApplyKnockbackEffect(currentHealthRatio);

        // 히트 이펙트 활성화
        ActivateHitEffect();
    }

    /// <summary>
    /// 피격 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        
        // 애니메이션 정리 및 전환
        ResetAnimationsAndTriggerHit();
        
        // 히트 이펙트 비활성화 (코루틴으로 지연 처리)
        _owner.StartCoroutine(DeactivateHitEffectWithDelay());

        // 무적 상태 해제
        SetImmuneState(false);
        
        // 저장된 속도 상태 초기화
        _owner.ClearStoredVelocity();
        
        // 넉백 효과 정리
        CleanupKnockbackEffect();
    }

    /// <summary>
    /// 피격 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        _damagedTimer += Time.deltaTime;

        // 최소 피격 시간이 지나지 않았으면 상태 전환하지 않음
        if (!IsMinimumDamagedTimeElapsed())
        {
            return;
        }

        // 최소 시간이 지난 후 바닥 착지 시 Idle 상태로 전환
        // 공중에 있을 시 Fall 상태로 전환
        if (IsGrounded2D())
        {
            SyncStateChange<PlayerIdleState>();
        }
        else
        {
            SyncStateChange<PlayerFallState>();
        }
    }
    
    /// <summary>
    /// 체력 비율에 따른 넉백 효과 적용 (거리 기반 데미지 비율도 고려)
    /// </summary>
    private void ApplyKnockbackEffect(float currentHealthRatio)
    {
        _originalLinearDamping = _owner.Rigidbody2D.linearDamping;

        // 거리 기반 데미지 비율 가져오기 (가까이서 맞으면 1.0, 멀리서 맞으면 작은 값)
        float damageRatio = _owner.LastDamageRatio;
        
        float startDamping = CalculateStartDamping(currentHealthRatio, damageRatio);
        _owner.Rigidbody2D.linearDamping = startDamping;
        
        // 시간에 따라 감쇠값을 증가시켜 점진적으로 감속
        ApplyDampingTween();
    }
    
    /// <summary>
    /// 넉백 효과 정리
    /// </summary>
    private void CleanupKnockbackEffect()
    {
        // 트윈 정리
        _knockbackTween?.Kill();
        
        // 원본 선형 감쇠값으로 복원
        _owner.Rigidbody2D.linearDamping = _originalLinearDamping;
    }
    
    /// <summary>
    /// 체력 비율과 거리에 따라 추가 힘을 적용
    /// 가까이서 맞으면 기존 힘 유지 (100%), 멀리서 맞으면 최소 50% 힘 적용
    /// </summary>
    private void ApplyHealthBasedForce()
    {
        if (_owner.Rigidbody2D == null) return;
        
        float currentHealthRatio = CalculateCurrentHealthRatio();
        float damageRatio = _owner.LastDamageRatio; // 거리 기반 데미지 비율
        
        // 체력 비율에 따른 기본 힘
        float additionalForceMagnitude = (1.0f - currentHealthRatio) * MAX_ADDITIONAL_FORCE;
        
        // 거리 기반 스케일 적용:
        // damageRatio = 1.0 (가까이) -> distanceScale = 1.0 (100% 힘)
        // damageRatio = 0.0 (멀리) -> distanceScale = 0.5 (50% 힘)
        float distanceScale = Mathf.Lerp(MIN_KNOCKBACK_RATIO, 1.0f, damageRatio);
        additionalForceMagnitude *= distanceScale;
        
        // 현재 속도 방향으로 추가 힘 적용
        ApplyForceBasedOnVelocity(additionalForceMagnitude);
    }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// 무적 상태 설정
    /// </summary>
    private void SetImmuneState(bool isImmune)
    {
        // 태그 설정
        if (isImmune)
        {
            _owner.gameObject.tag = "Immune";
        }
        else
        {
            _owner.gameObject.tag = _owner.PhotonView.IsMine ? "Player" : "Enemy";
        }
        
        // 무적 상태 설정
        _owner.PlayerStat.IsImmune = isImmune;
        
        // 네트워크 동기화
        if (_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, isImmune);
        }
    }

    /// <summary>
    /// 저장된 속도가 있다면 복원
    /// </summary>
    private void RestoreStoredVelocityIfExists()
    {
        if (_owner.HasStoredVelocity)
        {
            _owner.RestoreVelocity();
        }
    }

    /// <summary>
    /// 현재 체력 비율 계산 (0~1 범위)
    /// </summary>
    private float CalculateCurrentHealthRatio()
    {
        return Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
    }

    /// <summary>
    /// 히트 이펙트 활성화
    /// </summary>
    private void ActivateHitEffect()
    {
        _owner.HitEffectPrefab.SetActive(true);
        // 필요 시 방향 설정
        // SetHitEffectDirection();
    }

    /// <summary>
    /// 애니메이션 리셋 및 히트 트리거 실행
    /// </summary>
    private void ResetAnimationsAndTriggerHit()
    {
        _owner.RPC_SetAnimatorTrigger("Hit");
        _owner.RPC_ResetAnimatorTrigger("HitLoop");
        _owner.RPC_ResetAnimatorTrigger("Walk");
        _owner.RPC_ResetAnimatorTrigger("Run");
        _owner.RPC_ResetAnimatorTrigger("Idle");
        _owner.RPC_ResetAnimatorTrigger("Dash");
        _owner.RPC_ResetAnimatorTrigger("Fall");
    }

    /// <summary>
    /// 지연 후 히트 이펙트 비활성화
    /// </summary>
    private IEnumerator DeactivateHitEffectWithDelay()
    {
        yield return new WaitForSeconds(HIT_EFFECT_DURATION);
        _owner.HitEffectPrefab.SetActive(false);
    }

    /// <summary>
    /// 최소 피격 시간이 경과했는지 확인
    /// </summary>
    private bool IsMinimumDamagedTimeElapsed()
    {
        return _damagedTimer >= _actualDamagedTime;
    }

    /// <summary>
    /// 거리 기반 데미지 비율에 따라 실제 피격 시간 계산
    /// 가까이서 맞으면 최대 시간 (100%), 멀리서 맞으면 최소 절반 시간 (50%)
    /// </summary>
    private void CalculateActualDamagedTime()
    {
        float damageRatio = _owner.LastDamageRatio; // 거리 기반 데미지 비율
        float baseDamagedTime = _owner.PlayerStat.DamagedTime;

        // damageRatio = 1.0 (가까이) -> 100% 시간 (baseDamagedTime)
        // damageRatio = 0.0 (멀리) -> 30% 시간 (baseDamagedTime * 0.5)
        _actualDamagedTime = Mathf.Lerp(baseDamagedTime * 0.3f, baseDamagedTime, damageRatio);
    }

    /// <summary>
    /// 체력 비율과 거리 기반 데미지 비율에 따른 시작 감쇠값 계산
    /// 가까이서 맞으면 기존 넉백 유지 (100%), 멀리서 맞으면 최소 50% 넉백
    /// </summary>
    private float CalculateStartDamping(float currentHealthRatio, float damageRatio)
    {
        float baseDamping;
        
        if (currentHealthRatio >= HEALTH_RATIO_THRESHOLD)
        {
            baseDamping = _originalLinearDamping;
        }
        else
        {
            // 50%에서 0%까지 시작값이 0.5에서 0.01로 변화
            float ratio = (HEALTH_RATIO_THRESHOLD - currentHealthRatio) / HEALTH_RATIO_THRESHOLD;
            baseDamping = Mathf.Lerp(DAMPING_LERP_START, MIN_LINEAR_DAMPING, ratio);
        }

        // 거리 기반 감쇠값 조정: 
        // damageRatio = 1.0 (가까이) -> dampingMultiplier = 1.0 (100% 날아감)
        // damageRatio = 0.0 (멀리) -> dampingMultiplier = 2.0 (50% 날아감, 감쇠값 2배로 빠르게 멈춤)
        damageRatio = 1.0f;
        float dampingMultiplier = Mathf.Lerp(1.0f / MIN_KNOCKBACK_RATIO, 1.0f, damageRatio);
        return baseDamping * dampingMultiplier;
    }

    /// <summary>
    /// 감쇠값 트윈 적용
    /// </summary>
    private void ApplyDampingTween()
    {
        _knockbackTween?.Kill();
        _knockbackTween = DOTween.To(() => _owner.Rigidbody2D.linearDamping, 
            x => _owner.Rigidbody2D.linearDamping = x, 
            MAX_LINEAR_DAMPING, 
            _actualDamagedTime)
            .SetEase(Ease.InOutBack);
    }

    /// <summary>
    /// 속도에 기반한 힘 적용
    /// </summary>
    private void ApplyForceBasedOnVelocity(float forceMagnitude)
    {
        Vector2 currentVelocity = _owner.Rigidbody2D.linearVelocity;
        
        if (currentVelocity.magnitude > MIN_VELOCITY_THRESHOLD)
        {
            Vector2 velocityDirection = currentVelocity.normalized;
            Vector2 additionalForce = velocityDirection * forceMagnitude;
            
            // 수평 및 수직 힘 적용
            _owner.Rigidbody2D.AddForce(additionalForce, ForceMode2D.Impulse);
            _owner.Rigidbody2D.AddForce(Vector2.up * UPWARD_FORCE, ForceMode2D.Impulse);
        }
    }
}
