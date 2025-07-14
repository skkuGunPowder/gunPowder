using RobustFSM.Base;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWalkState : MonoState
{
    private PlayerFSM _playerFSM;
    private Player _owner;

    // 대쉬 타이머
    private float _timer = 0f;
    

    // 더블탭 감지용 변수들
    private float _lastKeyPressTime = 0f;
    private bool _isKeyPressed = false;
    private float _keyReleaseTimer = 0f;
    private const float KEY_RELEASE_THRESHOLD = 0.1f; // 키를 떼고 이 시간 이내에 다시 누르면 더블탭으로 인식

    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        // 캐스팅
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        // 더블탭 변수 초기화
        _lastKeyPressTime = 0f;
        _isKeyPressed = false;
        _keyReleaseTimer = 0f;

        // 플레이어 상태
        _owner.MyMoveSpeed = _owner.PlayerStatSO.MoveSpeed;
        _owner.IsRunning = false;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Walk");
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
        _timer += Time.deltaTime;
        _keyReleaseTimer += Time.deltaTime;

        // 이동
        // 2D기 때문에 +x, -x로만 이동한다.
        // 오른쪽 화살표 -> 우측이동, 왼쪽 화살표 -> 좌측이동
        if(Input.GetKey(KeyCode.RightArrow))
        {
            _owner.SetFacingDirection(1);
            _owner.CharacterController.Move(Vector3.right * _owner.MyMoveSpeed * Time.deltaTime);
            
            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;
                
                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                if (_owner.FacingDirection == 1 && (currentTime - _lastKeyPressTime) <= _owner.PlayerStatSO.DoubleTapTime)
                {
                    Debug.Log("WalkState: 오른쪽 더블탭 감지 - DashState로 전환");
                    _playerFSM.ChangeState<PlayerDashState>();
                    return;
                }
                
                _lastKeyPressTime = currentTime;
            }
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            _owner.SetFacingDirection(-1);
            _owner.CharacterController.Move(Vector3.left * _owner.PlayerStatSO.MoveSpeed * Time.deltaTime);
            
            // 키 입력 감지
            if (!_isKeyPressed)
            {
                _isKeyPressed = true;
                float currentTime = Time.time;
                
                // 더블탭 체크 (같은 방향이고, 시간 간격이 짧을 때)
                if (_owner.FacingDirection == -1 && (currentTime - _lastKeyPressTime) <= _owner.PlayerStatSO.DoubleTapTime)
                {
                    Debug.Log("WalkState: 왼쪽 더블탭 감지 - DashState로 전환");
                    _playerFSM.ChangeState<PlayerDashState>();
                    return;
                }
                
                _lastKeyPressTime = currentTime;
            }
        }
        else
        {
            // 키를 떼었을 때
            if (_isKeyPressed)
            {
                _isKeyPressed = false;
                _keyReleaseTimer = 0f;
            }
            
            // 키를 떼고 일정 시간이 지나면 Idle로 전환
            if (_keyReleaseTimer >= KEY_RELEASE_THRESHOLD)
            {
                Debug.Log("WalkState: 키를 떼어서 IdleState로 전환");
                _playerFSM.ChangeState<PlayerIdleState>();
                return;
            }
        }

        // 점프 키 입력 체크
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _playerFSM.ChangeState<PlayerJumpState>();
            return;
        }
    }
}
