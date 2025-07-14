using RobustFSM.Base;
using UnityEngine;

public class PlayerRunState : MonoState
{
    private PlayerFSM _playerFSM;
    private Player _owner;

    private float _keyReleaseTimer = 0f;
    private float _keyReleaseThreshold = 0.1f;

    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        _keyReleaseTimer = 0f;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Run");
    }

    public override void OnExit()
    {
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.RightArrow) && _owner.FacingDirection == 1
        || Input.GetKey(KeyCode.LeftArrow) && _owner.FacingDirection == -1)
        {
            _owner.CharacterController.Move(new Vector3(_owner.FacingDirection, 0, 0)
                                    * _owner.PlayerStatSO.RunSpeed * Time.deltaTime);
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