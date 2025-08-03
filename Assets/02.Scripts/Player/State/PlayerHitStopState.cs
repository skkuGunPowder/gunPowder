using UnityEngine;
using DG.Tweening;

public class PlayerHitStopState : PlayerBaseState
{
    private float _timer = 0f;
    private float _hitStopTime = 0.3f;
    private Sequence _shakeSequence;
    
    // 히트스탑 중복 피격 처리
    private Vector2 _lastStoredVelocity = Vector2.zero;
    private bool _hasStoredVelocity = false;
    
    // 피의 비율에 따른 히트스탑 설정
    private const float MIN_HIT_STOP_TIME = 0.3f;
    private const float MAX_HIT_STOP_TIME = 0.6f;
    private const float MAX_SHAKE_X = 2f;
    private const float MAX_SHAKE_Y = 1f;
    
    public override void OnEnter()
    {
        base.OnEnter();

        _timer = 0f;
        
        // 피의 비율에 따른 히트스탑 시간 계산
        CalculateHitStopTime();
        
        // 현재 속도를 저장 (중복 피격 시 덮어쓰기)
        if (_owner.Rigidbody2D != null)
        {
            _lastStoredVelocity = _owner.Rigidbody2D.linearVelocity;
            _hasStoredVelocity = true;
            _owner.StoreVelocity();
        }
        
        // 히트스탑 중에는 속도를 0으로 설정
        if (_owner.Rigidbody2D != null)
        {
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
        
        // 캐릭터 흔들림 효과 시작 (PhotonTransformView가 자동 동기화)
        StartShakeEffect();
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 흔들림 효과 정리
        if (_shakeSequence != null)
        {
            _shakeSequence.Kill();
            _shakeSequence = null;
        }
        
        // 마지막 저장된 속도로 복원 (중복 피격 처리)
        if (_hasStoredVelocity)
        {
            _owner.RestoreVelocity();
            _hasStoredVelocity = false;
        }
    }

    public override void MineUpdate()
    {
        _timer += Time.deltaTime;

        if(_timer > _hitStopTime)
        {
            _playerFSM.ChangeState<PlayerDamagedState>();
        }
    }
    
    /// <summary>
    /// 피의 비율에 따른 히트스탑 시간 계산
    /// </summary>
    private void CalculateHitStopTime()
    {
        if (_owner == null || _owner.PlayerStat == null) return;
        
        // 현재 피의 비율 계산 (0~1), 최대 1.0으로 제한
        float healthRatio = Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
        
        // 피가 적을수록 히트스탑 시간이 길어짐 (1-healthRatio)
        float timeMultiplier = 1f - healthRatio;
        _hitStopTime = Mathf.Lerp(MIN_HIT_STOP_TIME, MAX_HIT_STOP_TIME, timeMultiplier);
    }
    
    /// <summary>
    /// 피의 비율에 따른 떨리는 크기 계산
    /// </summary>
    private Vector2 CalculateShakeIntensity()
    {
        if (_owner == null || _owner.PlayerStat == null) 
            return new Vector2(1f, 0.5f);
        
        // 현재 피의 비율 계산 (0~1), 최대 1.0으로 제한
        float healthRatio = Mathf.Clamp01((float)_owner.PlayerStat.CurrentPlayerGunPowderCount / _owner.PlayerStat.InitGunpowderCount);
        
        // 피가 적을수록 떨리는 크기가 커짐 (1-healthRatio)
        float intensityMultiplier = 1f - healthRatio;
        
        float shakeX = Mathf.Lerp(1f, MAX_SHAKE_X, intensityMultiplier);
        float shakeY = Mathf.Lerp(0.5f, MAX_SHAKE_Y, intensityMultiplier);
        
        return new Vector2(shakeX, shakeY);
    }
    
    private void StartShakeEffect()
    {
        if (_owner == null) return;
        
        // 기존 시퀀스가 있다면 정리
        if (_shakeSequence != null)
        {
            _shakeSequence.Kill();
        }
        
        // 현재 Y 위치 저장
        float originalY = _owner.transform.position.y;
        
        // 피의 비율에 따른 떨리는 크기 계산
        Vector2 shakeIntensity = CalculateShakeIntensity();
        
        // 흔들림 시퀀스 생성
        _shakeSequence = DOTween.Sequence();
        
        // X축 흔들림 (좌우)
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, shakeIntensity.x, 20, 90, false, true));
        
        // Y축 흔들림 (상하) - 기존 Y 위치보다 아래로 내려가지 않도록
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, shakeIntensity.y, 10, 90, false, true)
            .OnUpdate(() => {
                // Y 위치가 원래보다 아래로 내려가지 않도록 제한
                Vector3 currentPos = _owner.transform.position;
                if (currentPos.y < originalY)
                {
                    currentPos.y = originalY;
                    _owner.transform.position = currentPos;
                }
            }));
        
        // 시퀀스 완료 시 정리
        _shakeSequence.OnComplete(() => {
            _shakeSequence = null;
        });
    }
    
    /// <summary>
    /// 히트스탑 중에 추가 피격을 받았을 때 호출
    /// </summary>
    public void OnAdditionalHit()
    {
        Debug.Log("OnAdditionalHit");
        // 타이머 리셋
        _timer = 0f;
        
        // 피의 비율에 따른 히트스탑 시간 재계산
        CalculateHitStopTime();
        
        // 새로운 속도로 업데이트
        if (_owner.Rigidbody2D != null)
        {
            _lastStoredVelocity = _owner.Rigidbody2D.linearVelocity;
            _owner.StoreVelocity();
            _owner.Rigidbody2D.linearVelocity = Vector2.zero;
        }
        
        // 흔들림 효과 재시작
        StartShakeEffect();
    }
}
