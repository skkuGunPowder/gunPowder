using UnityEngine;
using System.Collections.Generic;
using System;
using RaycastPro.RaySensors2D;
using Photon.Pun;

public class Player : MonoBehaviourPun, IDamagable
{
    [SerializeField]
    private List<Animator> _myAnimatorList;
    public List<Animator> MyAnimatorList => _myAnimatorList;

    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D => _rigidbody2D;

    private PlayerStat _playerStat;
    public PlayerStat PlayerStat => _playerStat;

    public PhotonView PhotonView;

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

    private BoxRay2D _groundRay2D;
    public BoxRay2D GroundRay2D => _groundRay2D;


    public GameObject NormalBombPrefab;
    public GameObject SpecialBombPrefab;
    public GameObject GunPowderPrefab;
    public GameObject DieExplosionPrefab;


    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _groundRay2D = GetComponent<BoxRay2D>();
        PhotonView = GetComponent<PhotonView>();
        LoadItems();
    }

    private void LoadItems()
    {
        // ItemStorage.Instance.Get
    }

    private void OnEnable()
    {
        _playerStat.OnGunPowderEmpty += HandleGunPowderEmpty;

        if (!photonView.IsMine)
        {
            _rigidbody2D.gravityScale = 0;
        }
    }

    public void InitializePlayer()
    {
        _playerStat.InitializeStats();
        _attackTimer = 0f;
        _gunPowderDecreaseTimer = 0f;
        _gunPowderDecreaseWithoutAttackTimer = 0f;
    }

    private void HandleGunPowderEmpty()
    {
        GetComponent<PlayerFSM>().ChangeState<PlayerDieState>();
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


    public void TakeDamage(int damage, Vector3 attackerBomb, int attackerViewId, bool isFallingOut)
    {
        PhotonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, attackerBomb, attackerViewId, isFallingOut);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage, Vector3 attackerBomb, int attackerViewId, bool isFallingOut,PhotonMessageInfo info)
    {
        // 체력 감소
        _playerStat.DecreaseGunPowderCount(damage);
        PlayerSettingManager.Instance.RequestTakeDamage(_playerStat.CurrentPlayerGunPowderCount, _playerStat.CurrentPlayerLife,info.Sender.ActorNumber);
        // Gunpowder 낙출
        ReleaseGunPowder(attackerBomb, attackerViewId, damage, _gunPowderSpreadAngle, _gunPowderSpreadDistance, isFallingOut);

        // 피격 횟수 증가
        _playerStat.IncreseDamagedCount();

        // 피격 이벤트 발생
        OnHit?.Invoke();
    }

    /// <summary>
    /// 피격시 건파우더 흩뿌리기
    /// </summary>
    [PunRPC]
    public void ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // attackerViewId로 Transform 찾기
        Transform attacker = null;
        PhotonView attackerView = PhotonView.Find(attackerViewId);
        if (attackerView != null)
            attacker = attackerView.transform;

        Vector3 baseDir = (transform.position - explosionOrigin).normalized;

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * spreadAngle;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * baseDir;
            Vector3 spawnPos = transform.position + dir * distance;
            spawnPos.z = 0f;
            object[] instData = new object[] { attackerViewId, isFallingOut };
            PhotonNetwork.Instantiate(GunPowderPrefab.name, spawnPos, Quaternion.identity, 0, instData);
        }
    }

    public void RPC_ReleaseGunPowder(Vector3 explosionOrigin, int attackerViewId, int count = 3, float spreadAngle = 30f,
     float distance = 1.0f, bool isFallingOut = true)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(ReleaseGunPowder), RpcTarget.All, explosionOrigin, attackerViewId, count, spreadAngle, distance, isFallingOut);
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
                return _playerStat.FacingDirection == 1 ? _bombSpawnPointList[(int)EBombSpawnPoint.Right] : _bombSpawnPointList[(int)EBombSpawnPoint.Left];
        }
    }

    public Transform GetBombSpawnPoint(EBombSpawnPoint spawnPoint)
    {
        return _bombSpawnPointList[(int)spawnPoint];
    }

    [PunRPC]
    public void SetAnimatorTrigger(string triggerName)
    {
        foreach (Animator animator in _myAnimatorList)
        {
            animator.SetTrigger(triggerName);
        }
    }


    [PunRPC]
    public void RPC_SetAnimatorTrigger(string triggerName)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(SetAnimatorTrigger), RpcTarget.All, triggerName);
    }

    [PunRPC]
    public void ResetAnimatorTrigger(string triggerName)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        foreach (Animator animator in _myAnimatorList)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    [PunRPC]
    public void RPC_ResetAnimatorTrigger(string triggerName)
    {
        if(!PhotonView.IsMine)
        {
            return;
        }

        PhotonView.RPC(nameof(ResetAnimatorTrigger), RpcTarget.All, triggerName);
    }

    public void RPC_SetFacingDirection(int direction)
    {
        PhotonView.RPC(nameof(SetFacingDirection), RpcTarget.All, direction);
    }

    [PunRPC]
    public void SetFacingDirection(int direction)
    {
        _playerStat.FacingDirection = direction;
        foreach (SpriteRenderer spriteRenderer in _playerStat.MySpriteREndererList)
        {
            if (_playerStat.FacingDirection == 1)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}
