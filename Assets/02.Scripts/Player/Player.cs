using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Animator _myAnimator;
    public Animator MyAnimator => _myAnimator;
    private CharacterController _characterController;
    public CharacterController CharacterController => _characterController;
    private PlayerStat _playerStat;
    public PlayerStat PlayerStat => _playerStat;

    [Header("Bomb")]
    // 폭탄 스폰 위치 리스트
    // 0 45 90 135 180 225 270 315
    [SerializeField]
    private List<Transform> _bombSpawnPointList;
    [SerializeField]
    private Bomb _normalBomb;
    public Bomb NormalBomb => _normalBomb;
    [SerializeField]
    private Bomb _specialBomb;
    public Bomb SpecialBomb => _specialBomb;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStat = GetComponent<PlayerStat>();
    }

    public void SetFacingDirection(int direction)
    {
        _playerStat.SetFacingDirection(direction);
    }

    /// <summary>
    /// 키보드 입력에 따라 폭탄 스폰 위치 반환
    /// 8방향으로 나누어져 있다.
    /// </summary>
    /// <returns></returns>
    public Transform GetBombSpawnPoint()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Debug.Log($"h: {h}, v: {v}");
        switch ((h, v))
        {
            case (1, 0):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Right];
            case (1, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.RightUp];
            case (0, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Up];
            case (-1, 1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.LeftUp];
            case (-1, 0):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Left];
            case (-1, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.LeftDown];
            case (0, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.Down];
            case (1, -1):
                return _bombSpawnPointList[(int)EBombSpawnPoint.RightDown];
            default:
                return _bombSpawnPointList[(int)EBombSpawnPoint.Right];
        }
    }

    public Transform GetBombSpawnPoint(EBombSpawnPoint spawnPoint)
    {
        return _bombSpawnPointList[(int)spawnPoint];
    }
}
