using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class GunPowderBezierCurve : MonoBehaviour
{
    private Vector2[] _points = new Vector2[4];

    [SerializeField]
    private float _timerMax = 0;
    [SerializeField]
    private float _timerCurrent = 0;
    [SerializeField]
    private float _speed;
    public float RandomTimerMax = 1.5f;
    public float RandomTimerMin = 0.8f;

    [Header("Init")]
    [SerializeField]
    private Transform _start;
    [SerializeField]
    private Transform _end;
    [SerializeField]
    private float _newPointDistanceFromStart;
    [SerializeField]
    private float _newPointDistanceFromEnd;
    [SerializeField]
    private float _gunPowderSpeed = 5f;

    private bool _bezierFinished = false;
    private float _followSpeed = 10f;

    //private const int RANDOM_SEED = 1234567890;

    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _collider;

    private Transform _target;
    private bool _isFallingOut;
    private bool _hasPlayedDestroyVFX;

    private PhotonView _photonView;

    public GameObject VFXPrefab;
    private GunPowder _gunPowder;

    private void Awake()
    {
        _gunPowder = GetComponent<GunPowder>();
    }

    private void OnEnable()
    {
        //Random.InitState(RANDOM_SEED);
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _photonView = GetComponent<PhotonView>();

        if (_rigidbody2D == null || _collider == null)
        {
            Debug.LogError("[GunPowderBezierCurve] Missing required components (Rigidbody2D/BoxCollider2D). Disabling component.");
            enabled = false;
            return;
        }

        if (_rigidbody2D != null) _rigidbody2D.linearVelocity = Vector2.zero;
        gameObject.GetComponentInChildren<GunPowderTrigger>().enabled = false;

        // Resume particle systems that may have been stopped during release phase
        var release = GetComponent<GunPowderRelease>();
        if (release != null)
        {
            release.ResumeParticles();
        }
        else
        {
            var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Play(true);
            }
        }

        // Player 레이어를 ExcludeLayers에서 제거
        int playerLayer = LayerMask.NameToLayer("Player");
        //int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (_collider != null)
        {
            _collider.excludeLayers &= ~(1 << playerLayer);
            //_collider.excludeLayers &= ~(1 << enemyLayer);
        }
        if (_rigidbody2D != null)
        {
            _rigidbody2D.excludeLayers &= ~(1 << playerLayer);
            //_rigidbody2D.excludeLayers &= ~(1 << enemyLayer);
        }

        _start = transform;
        _timerCurrent = 0f;

        // GunPowder에서 타겟과 isFallingOut을 받아옴
        GunPowder gunPowder = GetComponent<GunPowder>();
        if (gunPowder == null)
        {
            Debug.LogError("[GunPowderBezierCurve] Missing GunPowder component on the same GameObject. Disabling.");
            enabled = false;
            return;
        }
        _target = gunPowder.Target;
        // 타겟이 없거나 비활성화되었다면 즉시 제거
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            if(_gunPowder._isDestroying)
            {
                return;
            }

            if(_photonView != null && _photonView.IsMine)
            {
                _gunPowder._isDestroying = true;
                PhotonNetwork.Destroy(gameObject);
                // InstantiateDestroyManager.Instance.RequestDestroy(_photonView.ViewID);
            }
            return;
        }

        // 타겟과 SourceViewId가 같다면 파괴
        if (gunPowder.Target != null)
        {
            PhotonView targetPhotonView = gunPowder.Target.GetComponent<PhotonView>();
            if (targetPhotonView != null && targetPhotonView.gameObject.activeInHierarchy)
            {
                if (gunPowder.SourceViewId == targetPhotonView.ViewID)
                {
                    if(_gunPowder._isDestroying)
                    {
                        return;
                    }

                    if(_photonView != null && _photonView.IsMine)
                    {
                        _gunPowder._isDestroying = true;
                        PhotonNetwork.Destroy(gameObject);
                        // InstantiateDestroyManager.Instance.RequestDestroy(_photonView.ViewID);
                    }
                    return;
                }
            }
        }
        _isFallingOut = gunPowder.IsFallingOut;

        _speed = _gunPowderSpeed;
        Init(_start, _target, _speed, _newPointDistanceFromStart, _newPointDistanceFromEnd);
    }

    public void Init(Transform start, Transform end, float speed, float newPointDistanceFromStart, float newPointDistanceFromEnd)
    {
        _speed = speed;

        // 도착할 시간 랜덤
        _timerMax = Random.Range(RandomTimerMin, RandomTimerMax);

        // 시작점
        _points[0] = start.position;

        // 시작점을 기준으로 랜덤 포인트 지정
        _points[1] = start.position + 
            newPointDistanceFromStart * Random.Range(-1.0f, 1.0f) * start.right +       // X (좌, 우 전체)
            newPointDistanceFromStart * Random.Range(-0.15f, 1.0f) * start.up;          // Y (아래쪽 조금, 위쪽 전체)

        // 끝점을 기준으로 랜덤 포인트 지정
        _points[2] = end.position + 
            newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.right +       // X (좌, 우 전체)
            newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.up;            // Y (아래, 위 전체체)

        // 끝점
        _points[3] = end.position;

        transform.position = start.position;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        _target = gameObject.GetComponent<GunPowder>().Target;
    }

    private void Update()
    {
        if (_gunPowder._isDestroying)
        {
            return;
        }

        // 플레이어가 사라지거나 비활성화된 경우 즉시 제거
        if (_target == null || _target.gameObject == null || !_target.gameObject.activeInHierarchy)
        {
            if(_photonView != null && _photonView.IsMine)
            {
                _gunPowder._isDestroying = true;
                transform.DOKill();
                PhotonNetwork.Destroy(gameObject);
                // InstantiateDestroyManager.Instance.RequestDestroy(_photonView.ViewID);
            }
            return;
        }

        if (!_bezierFinished)
        {
            if (_timerCurrent > _timerMax)
            {   
                _bezierFinished = true;
                return;
            }

            _points[3] = _target.position;
            _timerCurrent += Time.deltaTime * _speed;
            transform.position = BezierCurve.BezierCurve2D(_points[0], _points[3], _points[1], _points[2], _timerCurrent/_timerMax);
        }
        else
        {
            // 베지어 끝난 후, 플레이어를 향해 직선 이동
            float followSpeed = _speed * _followSpeed; // 직선 이동 속도 (베지어보다 약간 빠르게)
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * followSpeed * Time.deltaTime;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(_gunPowder._isDestroying)
        {
            return;
        }

        if(collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy")
          || collision.gameObject.CompareTag("Immune"))
        {
            // 자신이 생성한 건파우더인지 확인
            PhotonView collisionPhotonView = collision.GetComponent<PhotonView>();
            if(collisionPhotonView != null && collisionPhotonView.gameObject.activeInHierarchy)
            {
                if(collisionPhotonView.ViewID == GetComponent<GunPowder>().SourceViewId)
                {
                    return;
                }
            }

            // 플레이어 컴포넌트 확인
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null && player.PlayerStat != null)
            {
                // 건파우더 제거 - 마스터에서만 처리
                // if (PhotonNetwork.IsMasterClient)
                if(_photonView != null && _photonView.IsMine)
                {
                    var targetView = player.GetComponent<PhotonView>();
                    if (targetView != null && targetView.gameObject.activeInHierarchy && targetView.Owner != null)
                    {
                        targetView.RPC(nameof(PlayerStat.RPC_RequestIncreaseGunPowder), targetView.Owner, 1);
                        _gunPowder._isDestroying = true;
                        transform.DOKill();
                        PhotonNetwork.Destroy(gameObject);
                        // if(_photonView != null && _photonView.IsMine)
                        // {
                        //     InstantiateDestroyManager.Instance.RequestDestroy(_photonView.ViewID);
                        // }
                    }
                }
            }
        }
    }

    
    private void OnDisable()
    {
        // 파괴(네트워크 동기화 포함) 시 로컬에서 VFX 재생
        if (_hasPlayedDestroyVFX)
        {
            return;
        }
        _hasPlayedDestroyVFX = true;

        if (VFXPrefab != null)
        {
            FollowVFX vfx = VFXPool.Instance.Get(VFXPrefab.name) as FollowVFX;
            if (vfx != null)
            {
                if (_target != null && _target.gameObject.activeInHierarchy)
                {
                    vfx.PlayAttached(_target);
                }
            }
        }
    }
}
