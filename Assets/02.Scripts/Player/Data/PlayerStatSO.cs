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

    [SerializeField] private float _recoilTime;
    public float RecoilTime => _recoilTime;

    [SerializeField] private float _recoilSpeed;
    public float RecoilSpeed => _recoilSpeed;

    [SerializeField] private float _normalRecoilTime;
    public float NormalRecoilTime => _normalRecoilTime;
    [SerializeField] private float _normalRecoilSpeed;
    public float NormalRecoilSpeed => _normalRecoilSpeed;

    [SerializeField] private int _maxGunPoderCount;
    public int MaxGunPoderCount => _maxGunPoderCount;
    [SerializeField] private float _gunPowderDecreaseTime;
    public float GunPowderDecreaseTime => _gunPowderDecreaseTime;
    [SerializeField] private int _attackPenaltyTime;
    public int AttackPenaltyTime => _attackPenaltyTime;
    [SerializeField] private int _attackPenaltyAmount;
    public int AttackPenaltyAmount => _attackPenaltyAmount;

    [Header("Die")]
    [SerializeField]private int _dieExplosionDamage;
    public int DieExplosionDamage => _dieExplosionDamage;
    [SerializeField]private float _dieExplosionRadius;
    public float DieExplosionRadius => _dieExplosionRadius;
    [SerializeField]private float _dieExplosionForce;
    public float DieExplosionForce => _dieExplosionForce;
    [SerializeField] private float _invincibleTime;
    public float InvincibleTime => _invincibleTime;

    [Header("Damaged")]
    [SerializeField] private float _damagedTime;
    public float DamagedTime => _damagedTime;
    
}
