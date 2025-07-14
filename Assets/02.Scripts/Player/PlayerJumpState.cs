using RobustFSM.Base;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerJumpState : MonoState
{
    private PlayerFSM _playerFSM;
    private Player _owner;

    private float _gravity = -40f; // 더 강한 중력 추천
    private float _yVelocity = 0f;
    private float _xVelocity = 0f;
    private float _jumpCoyoteTimer = 0f;
    private const float LANDING_GRACE_TIME = 0.1f;

    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        // 점프 시작 시 Y속도에 점프 파워를 부여
        _yVelocity = _owner.PlayerStatSO.JumpForce; // 예: 10f 등
        _jumpCoyoteTimer = 0f;
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
        _jumpCoyoteTimer += Time.deltaTime;

        // 중력 적용
        _yVelocity += _gravity * Time.deltaTime;
        // Clamp: 상승 최대치(점프파워), 하강 최대치(-20f 등)
        _yVelocity = Mathf.Clamp(_yVelocity, _gravity * 3, _owner.PlayerStatSO.JumpForce);

        // 좌우 이동
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _xVelocity = 1;
            _owner.SetFacingDirection(1);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            _xVelocity = -1;
            _owner.SetFacingDirection(-1);
        }
        else
        {
            _xVelocity = 0;
        }
        _xVelocity *= _owner.MyMoveSpeed;

        _owner.CharacterController.Move(new Vector3(_xVelocity, _yVelocity, 0) * Time.deltaTime);

        // 착지 체크 (유예 시간 이후에만)
        if (_jumpCoyoteTimer > LANDING_GRACE_TIME && _owner.CharacterController.isGrounded)
        {
            Debug.Log("착지!");
            _playerFSM.ChangeState<PlayerIdleState>();
            return;
        }
    }
}
