using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [Header("Scriptable Object Reference")]
    [SerializeField] private PlayerStatSO _playerStatSO;
    public PlayerStatSO PlayerStatSO => _playerStatSO;

    [Header("Movement Stats")]
    [SerializeField] private float _moveSpeed;
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

    [SerializeField] private float _jumpForce;
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }

    [SerializeField] private float _dashTime;
    public float DashTime { get => _dashTime; set => _dashTime = value; }

    [SerializeField] private float _doubleTapTime;
    public float DoubleTapTime { get => _doubleTapTime; set => _doubleTapTime = value; }

    [SerializeField] private float _dashSpeed;
    public float DashSpeed { get => _dashSpeed; set => _dashSpeed = value; }

    [SerializeField] private float _breakTime;
    public float BreakTime { get => _breakTime; set => _breakTime = value; }

    [SerializeField] private float _runSpeed;
    public float RunSpeed { get => _runSpeed; set => _runSpeed = value; }

    [SerializeField] private float _maxJumpCount;
    public float MaxJumpCount { get => _maxJumpCount; set => _maxJumpCount = value; }

    [SerializeField] private float _jumpDashCount;
    public float JumpDashCount { get => _jumpDashCount; set => _jumpDashCount = value; }

    [Header("Player State Stats")]
    [SerializeField] private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }

    [SerializeField] private bool _isJumping = false;
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }

    [SerializeField] private float _myMoveSpeed;
    public float MyMoveSpeed { get => _myMoveSpeed; set => _myMoveSpeed = value; }

    [SerializeField] private float _jumpCount = 0;
    public float JumpCount { get => _jumpCount; set => _jumpCount = value; }

    [SerializeField] private int _facingDirection = 1;
    public int FacingDirection { get => _facingDirection; set => _facingDirection = value; }

    public bool IsFallingFromLedge = false;

    void Start()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        if (_playerStatSO != null)
        {
            _moveSpeed = _playerStatSO.MoveSpeed;
            _jumpForce = _playerStatSO.JumpForce;
            _dashTime = _playerStatSO.DashTime;
            _doubleTapTime = _playerStatSO.DoubleTapTime;
            _dashSpeed = _playerStatSO.DashSpeed;
            _breakTime = _playerStatSO.BreakTime;
            _runSpeed = _playerStatSO.RunSpeed;
            _maxJumpCount = _playerStatSO.MaxJumpCount;
        }
    }

    public void SetFacingDirection(int direction)
    {
        _facingDirection = direction;
    }

    public void ResetJumpCount()
    {
        _jumpCount = 0;
    }

    public void IncrementJumpCount()
    {
        _jumpCount++;
    }

    public bool CanJump()
    {
        return _jumpCount < _maxJumpCount;
    }

    public void IncrementJumpDashCount()
    {
        _jumpDashCount++;
    }

    public bool CanJumpDash()
    {
        return _jumpDashCount < 1;
    }

    public void ResetJumpDashCount()
    {
        _jumpDashCount = 0;
    }
}
