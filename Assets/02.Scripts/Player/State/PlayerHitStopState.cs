using UnityEngine;
using DG.Tweening;

public class PlayerHitStopState : PlayerBaseState
{
    private float _timer = 0f;
    private float _hitStopTime = 0.5f;
    private Sequence _shakeSequence;
    
    // 히트스탑 중복 피격 처리
    private Vector2 _lastStoredVelocity = Vector2.zero;
    private bool _hasStoredVelocity = false;
    
    public override void OnEnter()
    {
        base.OnEnter();

        _timer = 0f;
        
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
        
        // 흔들림 시퀀스 생성
        _shakeSequence = DOTween.Sequence();
        
        // X축 흔들림 (좌우)
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, 1f, 20, 90, false, true));
        
        // Y축 흔들림 (상하) - 기존 Y 위치보다 아래로 내려가지 않도록
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, 0.5f, 10, 90, false, true)
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
