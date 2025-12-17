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
/// - 히트스탑 효과 통합 (건파우더 50 이하일 때)
/// 
/// 동작 방식:
/// 1. 피격 시 히트스탑 조건 확인 (건파우더 50 이하)
/// 2. 히트스탑이면 속도 저장 및 흔들림 효과 적용 후 Damaged 로직 진행
/// 3. 히트스탑이 아니면 기존 Damaged 로직만 진행
/// 4. 피격 시 무적 상태로 전환 및 태그 변경
/// 5. 체력 비율에 따른 넉백 효과 적용
/// 6. 히트 이펙트 활성화 및 방향 설정
/// 7. 최소 피격 시간 후 바닥 착지 시 Idle 상태로 전환
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
    
    // 히트스탑 관련 상수
    private const float MIN_HIT_STOP_TIME = 0.4f;          // 최소 히트스탑 시간 (초)
    private const float MAX_HIT_STOP_TIME = 0.7f;         // 최대 히트스탑 시간 (초)
    private const float MIN_SHAKE_X = 1.5f;               // 최소 X축 흔들림 강도
    private const float MAX_SHAKE_X = 2.5f;                // 최대 X축 흔들림 강도
    private const float MIN_SHAKE_Y = 1f;                  // 최소 Y축 흔들림 강도
    private const float MAX_SHAKE_Y = 1.5f;                // 최대 Y축 흔들림 강도
    private const int SHAKE_VIBRATO_X = 20;                // X축 흔들림 진동 횟수
    private const int SHAKE_VIBRATO_Y = 10;                // Y축 흔들림 진동 횟수
    private const float SHAKE_RANDOMNESS = 90;              // 흔들림 무작위성 (도)
    
    // 상태 변수들
    private float _damagedTimer = 0f;           // 피격 지속 시간 타이머
    private float _actualDamagedTime = 0f;      // 실제 피격 시간 (거리 기반으로 조정된 값)
    private float _originalLinearDamping;       // 원본 선형 감쇠값
    private Tween _knockbackTween;              // 넉백 효과 트윈
    
    // 히트스탑 관련 변수들
    private bool _isHitStopActive = false;      // 히트스탑 활성화 여부
    private float _hitStopTimer = 0f;           // 히트스탑 타이머
    private float _currentHitStopDuration = 0f; // 현재 히트스탑 지속시간
    private Sequence _shakeSequence;           // DOTween 흔들림 시퀀스
    private float _originalYPosition;          // 히트스탑 시작 시 Y 위치
    
    // 히트스탑 여부를 전달하기 위한 static 변수
    private static bool _pendingHitStop = false;

    /// <summary>
    /// 다음 상태 진입 시 히트스탑 여부 설정 (PlayerBaseState에서 호출)
    /// </summary>
    public static void SetPendingHitStop(bool shouldHitStop)
    {
        _pendingHitStop = shouldHitStop;
    }

    /// <summary>
    /// 현재 히트스탑 활성화 여부 확인 (외부에서 호출)
    /// </summary>
    public bool IsHitStopActive()
    {
        return _isHitStopActive;
    }

    /// <summary>
    /// 피격 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        _damagedTimer = 0f;
        _hitStopTimer = 0f;
        
        // PlayerBaseState에서 설정한 히트스탑 여부 사용
        _isHitStopActive = _pendingHitStop;
        _pendingHitStop = false; // 사용 후 리셋
        
        if (_isHitStopActive)
        {
            // 히트스탑 로직 실행
            InitializeHitStop();
        }
        else
        {
            // 기존 Damaged 로직 실행
            InitializeDamaged();
        }
    }
    
    /// <summary>
    /// 히트스탑 초기화
    /// </summary>
    private void InitializeHitStop()
    {
        // 건파우더 비율에 따른 히트스탑 시간 계산
        CalculateHitStopDuration();
        
        // 현재 속도 저장 및 정지
        StoreCurrentVelocityAndFreeze();
        
        // Y 위치 저장 (흔들림 제한용)
        _originalYPosition = _owner.transform.position.y;
        
        // 캐릭터 흔들림 효과 시작
        StartScreenShakeEffect();
        
        // 히트 이펙트 활성화
        ActivateHitEffect();
    }
    
    /// <summary>
    /// 기존 Damaged 로직 초기화
    /// </summary>
    private void InitializeDamaged()
    {
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
        
        // 히트스탑 효과 정리
        if (_isHitStopActive)
        {
            CleanupShakeEffect();
            RestoreStoredVelocity();
        }
        
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
        // 히트스탑이 활성화되어 있으면 히트스탑 타이머 업데이트
        if (_isHitStopActive)
        {
            _hitStopTimer += Time.deltaTime;
            
            // 히트스탑 시간이 완료되면 Damaged 로직으로 전환
            if (_hitStopTimer >= _currentHitStopDuration)
            {
                // 히트스탑 완료 후 Damaged 로직 시작
                OnHitStopComplete();
                _isHitStopActive = false;
            }
            else
            {
                // 히트스탑 중에는 다른 로직 실행 안 함
                return;
            }
        }
        
        // Damaged 로직 진행
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
    /// 히트스탑 완료 후 Damaged 로직 시작
    /// </summary>
    private void OnHitStopComplete()
    {
        // 저장된 속도 복원
        RestoreStoredVelocity();
        
        // 거리 기반 실제 피격 시간 계산
        CalculateActualDamagedTime();
        
        _owner.RPC_SetAnimatorTrigger("HitLoop");
        
        // 무적 상태 설정 (히트스탑 완료 후 Damaged 로직으로 전환될 때 설정)
        SetImmuneState(true);
        
        // 체력 비례 추가 힘 적용
        ApplyHealthBasedForce();

        // 체력 비율에 따른 넉백 효과 적용
        float currentHealthRatio = CalculateCurrentHealthRatio();
        ApplyKnockbackEffect(currentHealthRatio);
    }
    
    /// <summary>
    /// 추가 피격 처리 (히트스탑 중 또는 일반 피격 중)
    /// </summary>
    public void OnAdditionalHit()
    {
        // 히트스탑이 활성화되어 있으면 히트스탑 타이머 리셋
        if (_isHitStopActive)
        {
            // 타이머 리셋 및 시간 재계산
            _hitStopTimer = 0f;
            CalculateHitStopDuration();
            
            // 새로운 속도로 업데이트
            UpdateStoredVelocity();
            
            // 흔들림 효과 재시작
            StartScreenShakeEffect();
        }
        // 히트스탑이 아니고 Damaged 로직 진행 중이면 새로운 DamagedState로 전환
        // (PlayerBaseState에서 이미 처리됨)
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
    
    // ====== 히트스탑 관련 메서드들 ======
    
    /// <summary>
    /// 건파우더 비율에 따른 히트스탑 시간 계산
    /// </summary>
    private void CalculateHitStopDuration()
    {
        if (_owner == null || _owner.PlayerStat == null) return;
        
        float healthRatio = CalculateCurrentHealthRatio();
        
        // 건파우더가 적을수록 히트스탑 시간이 길어짐
        float timeMultiplier = 1f - healthRatio;
        _currentHitStopDuration = Mathf.Lerp(MIN_HIT_STOP_TIME, MAX_HIT_STOP_TIME, timeMultiplier);
    }
    
    /// <summary>
    /// 건파우더 비율에 따른 화면 흔들림 강도 계산
    /// </summary>
    private Vector2 CalculateShakeIntensity()
    {
        if (_owner == null || _owner.PlayerStat == null)
            return new Vector2(MIN_SHAKE_X, MIN_SHAKE_Y);
        
        float healthRatio = CalculateCurrentHealthRatio();
        
        // 건파우더가 적을수록 흔들림이 강해짐
        float intensityMultiplier = 1f - healthRatio;
        
        float shakeX = Mathf.Lerp(MIN_SHAKE_X, MAX_SHAKE_X, intensityMultiplier);
        float shakeY = Mathf.Lerp(MIN_SHAKE_Y, MAX_SHAKE_Y, intensityMultiplier);
        
        return new Vector2(shakeX, shakeY);
    }
    
    /// <summary>
    /// 현재 속도 저장 및 움직임 정지
    /// </summary>
    private void StoreCurrentVelocityAndFreeze()
    {
        if (_owner.Rigidbody2D != null)
        {
            _owner.StoreVelocity();
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 저장된 속도 복원
    /// </summary>
    private void RestoreStoredVelocity()
    {
        if (_owner.HasStoredVelocity)
        {
            _owner.RestoreVelocity();
        }
    }
    
    /// <summary>
    /// 저장된 속도 업데이트
    /// </summary>
    private void UpdateStoredVelocity()
    {
        if (_owner.Rigidbody2D != null)
        {
            _owner.StoreVelocity();
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 화면 흔들림 효과 시작
    /// </summary>
    private void StartScreenShakeEffect()
    {
        if (_owner == null) return;
        
        // 기존 흔들림 효과 정리
        CleanupShakeEffect();
        
        // 건파우더 비율에 따른 흔들림 강도 계산
        Vector2 shakeIntensity = CalculateShakeIntensity();
        
        // 흔들림 시퀀스 생성 및 실행
        CreateShakeSequence(shakeIntensity);
    }
    
    /// <summary>
    /// 화면 흔들림 효과 정리
    /// </summary>
    private void CleanupShakeEffect()
    {
        if (_shakeSequence != null)
        {
            _shakeSequence.Kill();
            _shakeSequence = null;
        }
    }
    
    /// <summary>
    /// 흔들림 시퀀스 생성 및 실행
    /// </summary>
    private void CreateShakeSequence(Vector2 shakeIntensity)
    {
        _shakeSequence = DOTween.Sequence();
        
        // X축 흔들림 (좌우)
        _shakeSequence.Join(_owner.transform.DOShakePosition(
            _currentHitStopDuration, 
            shakeIntensity.x, 
            SHAKE_VIBRATO_X, 
            SHAKE_RANDOMNESS, 
            false, 
            true
        ));
        
        // Y축 흔들림 (상하) - Y 위치 제한 적용
        _shakeSequence.Join(_owner.transform.DOShakePosition(
            _currentHitStopDuration, 
            shakeIntensity.y, 
            SHAKE_VIBRATO_Y, 
            SHAKE_RANDOMNESS, 
            false, 
            true
        ).OnUpdate(() => {
            ConstrainYPosition(_originalYPosition);
        }));
        
        // 시퀀스 완료 시 정리
        _shakeSequence.OnComplete(() => {
            _shakeSequence = null;
        });
    }
    
    /// <summary>
    /// Y 위치가 원본보다 아래로 내려가지 않도록 제한
    /// </summary>
    private void ConstrainYPosition(float originalY)
    {
        Vector3 currentPos = _owner.transform.position;
        if (currentPos.y < originalY)
        {
            currentPos.y = originalY;
            _owner.transform.position = currentPos;
        }
    }
}
