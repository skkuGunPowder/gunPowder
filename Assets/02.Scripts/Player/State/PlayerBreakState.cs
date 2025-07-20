using RobustFSM.Base;
using UnityEngine;

public class PlayerBreakState : PlayerBaseState
{
    private float _breakTimer = 0f;
    private int _moveDirection = 1; // 브레이크 방향(반대방향)
    private bool _doubleTapReady = true; // 진입 시 이미 1회 입력된 것으로 간주
    private bool _isDoubleTapped = false;

    public override void OnEnter()
    {
        base.OnEnter();

        _breakTimer = 0f;
        _moveDirection = _owner.PlayerStat.FacingDirection;
        _doubleTapReady = true;
        _isDoubleTapped = false;

        // 플레이어 상태
        _owner.PlayerStat.IsRunning = false;

        _owner.SetFacingDirection(-_moveDirection);

        // 애니메이션 재생
        _owner.MyAnimator.SetTrigger("Break");
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.MyAnimator.ResetTrigger("Break");
    }

    public override void Update()
    {
        _breakTimer += Time.deltaTime;

        _owner.CharacterController.Move(new Vector3(_moveDirection, 0, 0)
                                    * _owner.PlayerStat.MyMoveSpeed/2 * Time.deltaTime);

        // 브레이크 타임 내에 같은 방향 키가 한 번 더 눌리면 Run
        if(_doubleTapReady && _breakTimer <= _owner.PlayerStat.DoubleTapTime)
        {
            if(Input.GetKeyDown(KeyCode.RightArrow) && _owner.PlayerStat.FacingDirection == 1 
            || Input.GetKeyDown(KeyCode.LeftArrow) && _owner.PlayerStat.FacingDirection == -1)
            {
                _isDoubleTapped = true;
                _doubleTapReady = false; // 더 이상 체크하지 않음
            }
        }

        if(_breakTimer >= _owner.PlayerStat.BreakTime)
        {
            if(_isDoubleTapped)
            {
                _playerFSM.ChangeState<PlayerRunState>();
            }
            else
            {
                _playerFSM.ChangeState<PlayerIdleState>();
            }
        }
    }
} 