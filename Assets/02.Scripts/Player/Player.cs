using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerStatSO _playerStatSO;
    public PlayerStatSO PlayerStatSO => _playerStatSO;

    [SerializeField]
    private Animator _myAnimator;
    public Animator MyAnimator => _myAnimator;
    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;

    private int _facingDirection = 1;
    public int FacingDirection => _facingDirection;     // 바라보는 방향
                                                        // -1: 왼쪽, 1: 오른쪽쪽
    [SerializeField]
    private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set => _isRunning = value; }
    [SerializeField]
    private bool _isJumping = false;
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }

    public float MyMoveSpeed { get; set; }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void SetFacingDirection(int direction)
    {
        _facingDirection = direction;
    }
}
