using UnityEngine;
using DG.Tweening;

public class PlayerHitStopState : PlayerBaseState
{
    private float _timer = 0f;
    private float _hitStopTime = 1f;
    private Sequence _shakeSequence;
    
    public override void OnEnter()
    {
        base.OnEnter();
        
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
    }

    public override void MineUpdate()
    {
        _timer += Time.deltaTime;

        if(_timer > _hitStopTime)
        {
            //_playerFSM.ChangeState<PlayerDamagedState>();
            _playerFSM.ChangeState<PlayerIdleState>();
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
        
        // 흔들림 시퀀스 생성
        _shakeSequence = DOTween.Sequence();
        
        // X축 흔들림 (좌우)
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, 1f, 20, 90, false, true));
        
        // Y축 흔들림 (상하) - 더 작은 강도
        _shakeSequence.Join(_owner.transform.DOShakePosition(_hitStopTime, 1f, 10, 90, false, true));
        
        // 시퀀스 완료 시 정리
        _shakeSequence.OnComplete(() => {
            _shakeSequence = null;
        });
    }
}
