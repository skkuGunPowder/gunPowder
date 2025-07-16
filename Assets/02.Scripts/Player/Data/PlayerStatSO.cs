using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatSO", menuName = "Scriptable Objects/PlayerStatSO")]
public class PlayerStatSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    public float MoveSpeed => moveSpeed;

    [SerializeField] private float _jumpForce;
    public float JumpForce => _jumpForce;

    [SerializeField] private float _dashTime;
    public float DashTime => _dashTime;

    [SerializeField] private float _doubleTapTime;
    public float DoubleTapTime => _doubleTapTime;

    [SerializeField] private float _dashSpeed;
    public float DashSpeed => _dashSpeed;

    [SerializeField] private float _breakTime;
    public float BreakTime => _breakTime;

    [SerializeField] private float _runSpeed;
    public float RunSpeed => _runSpeed;

    [SerializeField] private float _maxJumpCount;
    public float MaxJumpCount => _maxJumpCount;

   
    
    
    
}
