using UnityEngine;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour, IDamagable
{
    [SerializeField]
    private List<Animator> _myAnimatorList;
    public List<Animator> MyAnimatorList => _myAnimatorList;


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

    [Header("Timer")]
    [SerializeField]
    private float _attackTimer = 0f;
    public float AttackTimer => _attackTimer;
    [SerializeField]
    private float _gunPowderDecreaseTimer = 0f;
    public float GunPowderDecreaseTimer => _gunPowderDecreaseTimer;
    [SerializeField]
    private float _gunPowderDecreaseWithoutAttackTimer;
    public float GunPowderDecreaseWithoutAttackTimer => _gunPowderDecreaseWithoutAttackTimer;

    [Header("GunPowder")]
    [SerializeField]
    private float _gunPowderSpreadAngle = 90f;
    private float _gunPowderSpreadDistance = 1.0f;

    public event Action OnHit;



    // 테스트용
    public GameObject TestBomb;
    public GameObject TestGunPowder;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStat = GetComponent<PlayerStat>();
    }

    private void Update()
    {
        _attackTimer += Time.deltaTime;

        _gunPowderDecreaseTimer += Time.deltaTime;
        DecreaseGunPowderPeriodically();

        _gunPowderDecreaseWithoutAttackTimer += Time.deltaTime;
        DecreaseGunPowderWithoutAttack();
    }

    /// <summary>
    /// 주기적으로 건파우더 감소
    /// </summary>
    private void DecreaseGunPowderPeriodically()
    {
        if (_gunPowderDecreaseTimer >= PlayerStat.GunPowderDecreaseTime)
        {
            _gunPowderDecreaseTimer = 0f;
            _playerStat.DecreaseGunPowderCount(1);
        }
    }

    /// <summary>
    /// 공격을 일정시간 하지 않으면 건파우더 감소
    /// </summary>
    private void DecreaseGunPowderWithoutAttack()
    {
        if (_gunPowderDecreaseWithoutAttackTimer >= PlayerStat.AttackPenaltyTime)
        {
            _gunPowderDecreaseWithoutAttackTimer = 0f;
            _playerStat.DecreaseGunPowderCount(PlayerStat.AttackPenaltyAmount);
        }
    }

    /// <summary>
    /// 공격을 하면 타이머 초기화
    /// </summary>
    public void ResetGunPowderDecreaseWithoutAttackTimer()
    {
        _gunPowderDecreaseWithoutAttackTimer = 0f;
    }


    public void SetFacingDirection(int direction)
    {
        _playerStat.SetFacingDirection(direction);
    }

    public void TakeDamage(int damage, Vector3 attacker, bool isFallingOut)
    {
        // TODO: 피격 처리
        // 피 달기
        _playerStat.DecreaseGunPowderCount(damage);
        // 폭탄 맞은 위치 반 대 방향으로 건파우터 낙출
        ReleaseGunPowder(attacker, damage, _gunPowderSpreadAngle, _gunPowderSpreadDistance, isFallingOut);

        // 피격 상태 돌입

        // 피격 이벤트 발생
        OnHit?.Invoke();
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    public void ReleaseGunPowder(Vector3 explosionOrigin, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        Vector3 baseDir = (transform.position - explosionOrigin).normalized;

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * spreadAngle;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * baseDir;
            Vector3 spawnPos = transform.position + dir * distance;
            spawnPos.z = 0f;
            GunPowderBezierCurve gunPowder = Instantiate(TestGunPowder, spawnPos, Quaternion.identity).GetComponent<GunPowderBezierCurve>();

            //TODO: isFallingout에 따라 뭔가 설정
            if (isFallingOut)
            {
                gunPowder.GetComponent<GunPowderRelease>().enabled = true;
                gunPowder.GetComponent<GunPowderBezierCurve>().enabled = false;
            }
            else
            {
                gunPowder.GetComponent<GunPowderRelease>().enabled = false;
                gunPowder.GetComponent<GunPowderBezierCurve>().enabled = true;
            }
        }
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

    public void SetAnimatorTrigger(string triggerName)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void ResetAnimatorTrigger(string triggerName)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.ResetTrigger(triggerName);
        }
    }
}
