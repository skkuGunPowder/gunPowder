using RobustFSM.Base;
using UnityEngine;

public class PlayerDashState : MonoState
{
    private PlayerFSM _playerFSM;
    private Player _owner;

    private float _dashTimer = 0f;

    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        // 캐스팅
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        _dashTimer = 0f;
        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Dash");
    }
    public override void OnExit()
    {
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    private void Update()
    {
        _dashTimer += Time.deltaTime;

        // 1. 대시 이동(관성)
        _owner.CharacterController.Move(new Vector3(_owner.FacingDirection, 0, 0)
                                    * _owner.PlayerStatSO.DashSpeed * Time.deltaTime);

        int dir = _owner.FacingDirection;
        // 2. 대시 중 반대 방향 키 입력 체크 → BreakState로 전환
        if(Input.GetKeyDown(KeyCode.RightArrow) && dir == -1 || Input.GetKeyDown(KeyCode.LeftArrow) && dir == 1)
        {
            Debug.Log("DashState: 반대 방향 키 다운 - BreakState로 전환");
            _playerFSM.ChangeState<PlayerBreakState>();
            return;
        }

        // 3. 대시 시간 종료 후 상태 전이
        if(_dashTimer >= _owner.PlayerStatSO.DashTime)
        {
            // 같은 방향 키 누르고 있음 → Run
            if ((_owner.FacingDirection == 1 && Input.GetKey(KeyCode.RightArrow)) || (_owner.FacingDirection == -1 && Input.GetKey(KeyCode.LeftArrow)))
            {
                Debug.Log("DashState: 같은 방향 입력 - RunState로 전환");
                _playerFSM.ChangeState<PlayerRunState>();
                return;
            }
            // 아무 키도 안 누름 → Idle
            else
            {
                Debug.Log("DashState: 입력 없음 - IdleState로 전환");
                _playerFSM.ChangeState<PlayerIdleState>();
                return;
            }
        }
    }
}
