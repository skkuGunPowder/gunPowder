using UnityEngine;

public class Player : MonoBehaviour
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
}
