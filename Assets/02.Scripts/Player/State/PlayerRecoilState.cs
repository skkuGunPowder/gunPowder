using UnityEngine;

public class PlayerRecoilState : PlayerBaseState
{
    private float _recoilTimer = 0f;

    private float _xVelocity = 0f;
    public override void OnEnter()
    {
        base.OnEnter();
        _recoilTimer = 0f;
        _owner.PlayerStat.IsRunning = false;
        _owner.PlayerStat.MyMoveSpeed = _owner.PlayerStat.MoveSpeed;
        _xVelocity = -_owner.PlayerStat.FacingDirection * _owner.PlayerStat.RecoilSpeed;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {
        _recoilTimer += Time.deltaTime;

        _owner.CharacterController.Move(new Vector3(_xVelocity, 0, 0) * Time.deltaTime);
        
        // 시간이 지나면 아이들 상태로
        if(_recoilTimer >= _owner.PlayerStat.RecoilTime)
        {
            if(_owner.PlayerStat.IsJumping)
            {
                _owner.MyAnimator.SetTrigger("Fall");
                _playerFSM.ChangeState<PlayerJumpState>();
            }
            else
            {
                if (_owner.CharacterController.isGrounded)
                {
                    _playerFSM.ChangeState<PlayerIdleState>();  
                }
                else
                {
                    _owner.MyAnimator.SetTrigger("Fall");
                    _playerFSM.ChangeState<PlayerJumpState>();
                }
            }
            return;
        }
    }
}
