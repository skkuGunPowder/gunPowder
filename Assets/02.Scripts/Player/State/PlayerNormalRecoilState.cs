using UnityEngine;

public class PlayerNormalRecoilState : PlayerBaseState
{
    private float _recoilTimer = 0f;
    [SerializeField] private float _yVelocitySpeed = 1f;

    [SerializeField] private float _damping = 5f;       // 감쇠 정도
    [SerializeField] private float _frequency = 10f;     // 반동 진동 빈도
    [SerializeField] private float _recoilOscillationScale = 0.2f;

    public override void OnEnter()
    {
        base.OnEnter();
        _recoilTimer = 0f;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void MineUpdate()
    {
        _recoilTimer += Time.deltaTime;
        
        // 시간이 지나면 아이들 상태로
        if(_recoilTimer >= _owner.PlayerStat.NormalRecoilTime)
        {
            if(_owner.PlayerStat.IsJumping)
            {
                _owner.RPC_SetAnimatorTrigger("Fall");
                _playerFSM.ChangeState<PlayerFallState>();
            }
            else
            {
                if (IsGrounded2D())
                {
                    _playerFSM.ChangeState<PlayerIdleState>();  
                }
                else
                {
                    _owner.RPC_SetAnimatorTrigger("Fall");
                    _playerFSM.ChangeState<PlayerFallState>();
                }
            }
            return;
        }
    }
}
