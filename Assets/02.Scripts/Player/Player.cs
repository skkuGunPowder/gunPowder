using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{
    [SerializeField]
    private Animator _myAnimator;
    public Animator MyAnimator => _myAnimator;
    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    private PlayerStat _playerStat;
    public PlayerStat PlayerStat => _playerStat;


    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStat = GetComponent<PlayerStat>();
    }

    public void SetFacingDirection(int direction)
    {
        _playerStat.SetFacingDirection(direction);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("TakeDamage: " + damage);
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    private void ReleaseGunPowder()
    {

    }
}
