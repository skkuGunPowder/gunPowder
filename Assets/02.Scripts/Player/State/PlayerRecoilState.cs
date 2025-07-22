using System;
using UnityEngine;

public class PlayerRecoilState : PlayerBaseState
{
    private float _recoilTimer;
    [SerializeField] private float _yVelocitySpeed = 0.5f;

    [SerializeField] private float _damping = 5f;       // 감쇠 정도
    [SerializeField] private float _frequency = 10f;     // 반동 진동 빈도
    [SerializeField] private float _recoilOscillationScale = 0.2f;

    public override void OnEnter()
    {
        base.OnEnter();
        _recoilTimer = 0f;
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {
        _recoilTimer += Time.deltaTime;

        float t = _recoilTimer / _owner.PlayerStat.RecoilTime;

        // 감속 (기본 뒤로 이동)
        float baseSpeed = Mathf.Lerp(_owner.PlayerStat.RecoilSpeed, 0, t);

        // 탄성 (작은 X축 진동)
        float oscillation = Mathf.Sin(_recoilTimer * _frequency) 
                            * Mathf.Exp(-_recoilTimer * _damping) 
                            * _recoilOscillationScale 
                            * _owner.PlayerStat.RecoilSpeed;

        // Y축 반동
        float yBounce = Mathf.Sin(t * Mathf.PI * 2f) * _yVelocitySpeed; 
        Debug.Log(yBounce);

        // 최종 이동 벡터
        Vector2 recoilMove = new Vector2(-_owner.PlayerStat.FacingDirection * (baseSpeed + oscillation), yBounce);
        _owner.Rigidbody2D.linearVelocity = recoilMove;
        
        // 시간이 지나면 아이들 상태로
        if(_recoilTimer >= _owner.PlayerStat.RecoilTime)
        {
            if(_owner.PlayerStat.IsJumping)
            {
                _owner.SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerJumpState>();
            }
            else
            {
                if (IsGrounded2D())
                {
                    _playerFSM.ChangeState<PlayerIdleState>();  
                }
                else
                {
                    _owner.SetAnimatorTrigger("Fall");
                    _playerFSM.ChangeState<PlayerJumpState>();
                }
            }
            return;
        }
    }
}
