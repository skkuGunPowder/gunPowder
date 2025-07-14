using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatSO", menuName = "Scriptable Objects/PlayerStatSO")]
public class PlayerStatSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    public float MoveSpeed => moveSpeed;

    [SerializeField] private float jumpForce;
    public float JumpForce => jumpForce;

    [SerializeField] private float dashTime;
    public float DashTime => dashTime;

    [SerializeField] private float doubleTapTime;
    public float DoubleTapTime => doubleTapTime;

    [SerializeField] private float dashSpeed;
    public float DashSpeed => dashSpeed;

    [SerializeField] private float breakTime;
    public float BreakTime => breakTime;

    [SerializeField] private float runSpeed;
    public float RunSpeed => runSpeed;

   
    
    
    
}
