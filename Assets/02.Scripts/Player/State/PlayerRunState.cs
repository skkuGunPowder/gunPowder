using RobustFSM.Base;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.1f;

    public override void OnEnter()
    {
        base.OnEnter();
        _keyReleaseTimer = 0f;

        // 플레이어 상태
        _owner.MyMoveSpeed = _owner.PlayerStatSO.RunSpeed;
        _owner.IsRunning = true;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Run");
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void Update()
    {
        base.Update();

        if(Input.GetKey(KeyCode.RightArrow) && _owner.FacingDirection == 1
        || Input.GetKey(KeyCode.LeftArrow) && _owner.FacingDirection == -1)
        {
            _owner.CharacterController.Move(new Vector3(_owner.FacingDirection, 0, 0)
                                    * _owner.MyMoveSpeed * Time.deltaTime);
        }
        else if(Input.GetKeyUp(KeyCode.RightArrow) && _owner.FacingDirection == -1
        || Input.GetKeyUp(KeyCode.LeftArrow) && _owner.FacingDirection == 1)
        {
            _playerFSM.ChangeState<PlayerBreakState>();
        }
        else
        {
            _keyReleaseTimer += Time.deltaTime;
            if(_keyReleaseTimer >= _keyReleaseThreshold)
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }
        }
    }
} 