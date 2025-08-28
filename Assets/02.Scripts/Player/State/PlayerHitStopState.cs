using UnityEngine;
using DG.Tweening;

/// <summary>
/// 플레이어 히트스탑 상태 클래스
/// 
/// 역할:
/// - 피격 시 짧은 시간 동안 움직임을 정지시키는 효과
/// - 플레이어의 건파우더(체력) 비율에 따른 히트스탑 시간 조절
/// - 피격 시 캐릭터터 흔들림 효과 제공
/// - 히트스탑 중 추가 피격 처리
/// 
/// 동작 방식:
/// 1. 피격 시 현재 속도 저장 후 움직임 정지
/// 2. 건파우더 비율에 따른 히트스탑 시간 계산 (체력이 낮을수록 길어짐)
/// 3. 체력 비율에 따른 캐릭터터 흔들림 효과 적용
/// 4. 히트스탑 시간 완료 후 피격 상태(PlayerDamagedState)로 전환
/// 5. 히트스탑 중 추가 피격 시 시간 리셋 및 효과 재적용
/// </summary>
public class PlayerHitStopState : PlayerBaseState
{
    // 히트스탑 시간 관련 상수
    private const float MIN_HIT_STOP_TIME = 0.4f;              // 최소 히트스탑 시간 (초)
    private const float MAX_HIT_STOP_TIME = 0.7f;              // 최대 히트스탑 시간 (초)
    
    // 화면 흔들림 관련 상수
    private const float MIN_SHAKE_X = 1.5f;                      // 최소 X축 흔들림 강도
    private const float MAX_SHAKE_X = 2.5f;                    // 최대 X축 흔들림 강도
    private const float MIN_SHAKE_Y = 1f;                    // 최소 Y축 흔들림 강도
    private const float MAX_SHAKE_Y = 1.5f;                    // 최대 Y축 흔들림 강도
    private const int SHAKE_VIBRATO_X = 20;                    // X축 흔들림 진동 횟수
    private const int SHAKE_VIBRATO_Y = 10;                    // Y축 흔들림 진동 횟수
    private const float SHAKE_RANDOMNESS = 90;                 // 흔들림 무작위성 (도)
    
    // 상태 관련 변수들
    private float _hitStopTimer = 0f;                          // 히트스탑 타이머
    private float _currentHitStopDuration = 0.3f;              // 현재 히트스탑 지속시간
    
    // 속도 저장 관련 변수들
    private Vector2 _lastStoredVelocity = Vector2.zero;        // 마지막 저장된 속도
    private bool _hasStoredVelocity = false;                   // 속도 저장 여부
    
    // 효과 관련 변수들
    private Sequence _shakeSequence;                           // DOTween 흔들림 시퀀스
    
    /// <summary>
    /// 히트스탑 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        _hitStopTimer = 0f;
        
        // 건파우더 비율에 따른 히트스탑 시간 계산
        CalculateHitStopDuration();
        
        // 현재 속도 저장 및 정지
        StoreCurrentVelocityAndFreeze();
        
        // 캐릭터 흔들림 효과 시작
        StartScreenShakeEffect();
    }

    /// <summary>
    /// 히트스탑 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        base.OnExit();
        
        // 캐릭터 흔들림 효과 정리
        CleanupShakeEffect();
        
        // 저장된 속도 복원
        RestoreStoredVelocity();
    }

    /// <summary>
    /// 히트스탑 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {
        _hitStopTimer += Time.deltaTime;

        // 히트스탑 시간 완료 시 피격 상태로 전환
        if (_hitStopTimer > _currentHitStopDuration)
        {
            SyncStateChange<PlayerDamagedState>();
        }
    }
    
    /// <summary>
    /// 건파우더 비율에 따른 히트스탑 시간 계산
    /// </summary>
    private void CalculateHitStopDuration()
    {
        if (!ValidateOwnerAndStats()) return;
        
        float healthRatio = GetCurrentHealthRatio();
        
        // 건파우더가 적을수록 히트스탑 시간이 길어짐
        float timeMultiplier = 1f - healthRatio;
        _currentHitStopDuration = Mathf.Lerp(MIN_HIT_STOP_TIME, MAX_HIT_STOP_TIME, timeMultiplier);
    }
    
    /// <summary>
    /// 건파우더 비율에 따른 화면 흔들림 강도 계산
    /// </summary>
    private Vector2 CalculateShakeIntensity()
    {
        if (!ValidateOwnerAndStats()) 
            return new Vector2(MIN_SHAKE_X, MIN_SHAKE_Y);
        
        float healthRatio = GetCurrentHealthRatio();
        
        // 건파우더가 적을수록 흔들림이 강해짐
        float intensityMultiplier = 1f - healthRatio;
        
        float shakeX = Mathf.Lerp(MIN_SHAKE_X, MAX_SHAKE_X, intensityMultiplier);
        float shakeY = Mathf.Lerp(MIN_SHAKE_Y, MAX_SHAKE_Y, intensityMultiplier);
        
        return new Vector2(shakeX, shakeY);
    }
    
    /// <summary>
    /// 화면 흔들림 효과 시작
    /// </summary>
    private void CreateScreenShakeEffect()
    {
        if (_owner == null) return;
        
        // 기존 흔들림 효과 정리
        CleanupExistingShakeSequence();
        
        // Y축 위치 제한을 위한 원본 위치 저장
        float originalY = _owner.transform.position.y;
        
        // 건파우더 비율에 따른 흔들림 강도 계산
        Vector2 shakeIntensity = CalculateShakeIntensity();
        
        // 흔들림 시퀀스 생성 및 실행
        CreateShakeSequence(originalY, shakeIntensity);
    }
    
    /// <summary>
    /// 히트스탑 중 추가 피격 처리
    /// </summary>
    public void OnAdditionalHit()
    {
        // 타이머 리셋 및 시간 재계산
        ResetTimerAndRecalculateDuration();
        
        // 새로운 속도로 업데이트
        UpdateStoredVelocity();
        
        // 흔들림 효과 재시작
        StartScreenShakeEffect();
    }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// Owner와 PlayerStat의 유효성 검사
    /// </summary>
    private bool ValidateOwnerAndStats()
    {
        return _owner != null && _owner.PlayerStat != null;
    }

    /// <summary>
    /// 현재 건파우더 비율 계산 (0~1)
    /// </summary>
    private float GetCurrentHealthRatio()
    {
        return Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
    }

    /// <summary>
    /// 현재 속도 저장 및 움직임 정지
    /// </summary>
    private void StoreCurrentVelocityAndFreeze()
    {
        if (_owner.Rigidbody2D != null)
        {
            _lastStoredVelocity = _owner.Rigidbody2D.linearVelocity;
            _hasStoredVelocity = true;
            _owner.StoreVelocity();
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 화면 흔들림 효과 시작
    /// </summary>
    private void StartScreenShakeEffect()
    {
        CreateScreenShakeEffect();
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
    /// 저장된 속도 복원
    /// </summary>
    private void RestoreStoredVelocity()
    {
        if (_hasStoredVelocity)
        {
            _owner.RestoreVelocity();
            _hasStoredVelocity = false;
        }
    }

    /// <summary>
    /// 기존 흔들림 시퀀스 정리
    /// </summary>
    private void CleanupExistingShakeSequence()
    {
        if (_shakeSequence != null)
        {
            _shakeSequence.Kill();
        }
    }

    /// <summary>
    /// 흔들림 시퀀스 생성 및 실행
    /// </summary>
    private void CreateShakeSequence(float originalY, Vector2 shakeIntensity)
    {
        _shakeSequence = DOTween.Sequence();
        
        // X축 흔들림 (좌우)
        AddHorizontalShake(shakeIntensity.x);
        
        // Y축 흔들림 (상하) - Y 위치 제한 적용
        AddVerticalShakeWithConstraint(originalY, shakeIntensity.y);
        
        // 시퀀스 완료 시 정리
        _shakeSequence.OnComplete(() => {
            _shakeSequence = null;
        });
    }

    /// <summary>
    /// X축 흔들림 효과 추가
    /// </summary>
    private void AddHorizontalShake(float intensity)
    {
        _shakeSequence.Join(_owner.transform.DOShakePosition(
            _currentHitStopDuration, 
            intensity, 
            SHAKE_VIBRATO_X, 
            SHAKE_RANDOMNESS, 
            false, 
            true
        ));
    }

    /// <summary>
    /// Y축 흔들림 효과 추가 (Y 위치 제한 포함)
    /// </summary>
    private void AddVerticalShakeWithConstraint(float originalY, float intensity)
    {
        _shakeSequence.Join(_owner.transform.DOShakePosition(
            _currentHitStopDuration, 
            intensity, 
            SHAKE_VIBRATO_Y, 
            SHAKE_RANDOMNESS, 
            false, 
            true
        ).OnUpdate(() => {
            ConstrainYPosition(originalY);
        }));
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

    /// <summary>
    /// 타이머 리셋 및 히트스탑 시간 재계산
    /// </summary>
    private void ResetTimerAndRecalculateDuration()
    {
        _hitStopTimer = 0f;
        CalculateHitStopDuration();
    }

    /// <summary>
    /// 저장된 속도 업데이트
    /// </summary>
    private void UpdateStoredVelocity()
    {
        if (_owner.Rigidbody2D != null)
        {
            _lastStoredVelocity = _owner.Rigidbody2D.linearVelocity;
            _owner.StoreVelocity();
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
}
