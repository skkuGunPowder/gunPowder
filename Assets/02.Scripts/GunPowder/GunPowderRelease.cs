using Photon.Pun.UtilityScripts;
using RaycastPro.RaySensors;
using RaycastPro.RaySensors2D;
using UnityEngine;

public class GunPowderRelease : MonoBehaviour
{
    public int maxBounce = 3;      // 최대 튕김 횟수
    public float xzForce = 3f;     // XZ 평면 힘 (멀리 튀게)
    public float yForce = 5f;      // Y축 힘 (높이 튕게)
    public float gravity = 9.8f;   // 중력

    private Vector3 direction;     // 이동 방향
    private int currentBounce = 0;
    private bool isGrounded = true;

    private float maxHeight;
    private float currentHeight;

    private BoxCollider2D _collider;

    public float GroundCheckDistance; // 바닥 체크용 Raycast 거리

    public float GroundCheckOffset;
    public float YForceOffset;

    private BoxRay2D _groundRay2D;

    private Rigidbody2D _rigidbody2D;

    private int _randomSeed;

    private ParticleSystem[] _childParticleSystems;

    void OnEnable()
    {
        _collider = GetComponent<BoxCollider2D>();
        _groundRay2D = GetComponent<BoxRay2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        if (_collider == null || _groundRay2D == null || _rigidbody2D == null)
        {
            Debug.LogError("[GunPowderRelease] Missing required components (BoxCollider2D/BoxRay2D/Rigidbody2D). Disabling component.");
            enabled = false;
            return;
        }
        
        // 랜덤 시드가 설정되어 있으면 사용
        if (_randomSeed != 0)
        {
            Random.InitState(_randomSeed);
        }
        
        // XZ 평면 랜덤 방향
        Vector2 randXZ = Random.insideUnitCircle.normalized * xzForce;
        direction = new Vector3(randXZ.x, 0, 0);
        GroundCheckDistance = _collider.size.y * 0.5f * transform.localScale.y + 0.05f;

        float yRandom = Random.Range(yForce - YForceOffset, yForce + YForceOffset);
        maxHeight = yRandom;
        Initialize(direction, yRandom);
    }

    void Update()
    {
        if (!isGrounded)
        {
            CheckGroundHit();
        }
    }

    void Initialize(Vector3 _direction, float yForceValue)
    {
        isGrounded = false;
        direction = _direction;
        currentBounce++;
        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.AddForce(new Vector2(direction.x, yForceValue), ForceMode2D.Impulse);
        }
    }

    void Initialize(Vector3 _direction)
    {
        // 랜덤 시드가 설정되어 있으면 사용
        if (_randomSeed != 0)
        {
            Random.InitState(_randomSeed);
        }
        
        float yRandom = Random.Range(yForce - YForceOffset, yForce + YForceOffset);
        Initialize(_direction, yRandom);
    }

    void CheckGroundHit()
    {
        if (_groundRay2D == null) return;

        _groundRay2D.Cast();

        if (_groundRay2D.Performed)
        {
            var hit = _groundRay2D.Hit;
            // Rigidbody2D의 y속도가 0 이하(하강 중)일 때만 튕김 처리
            if (transform.position.y - hit.point.y < transform.localScale.y + GroundCheckOffset && _rigidbody2D != null && _rigidbody2D.linearVelocity.y <= 0)
            {
                float halfHeight = _collider != null ? GroundCheckDistance : 0.5f;

                Vector3 pos = transform.position;
                pos.y = hit.point.y + halfHeight;
                transform.position = pos;

                if (currentBounce < maxBounce)
                {
                    Initialize(direction / 1.1f);
                }
                else
                {
                    isGrounded = true;
                    if (_rigidbody2D != null) 
                    {
                        _rigidbody2D.linearVelocity = Vector2.zero;
                        _rigidbody2D.gravityScale = 0f; // 중력 비활성화
                        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll; // 모든 이동 고정
                    }
                    if (_collider != null)
                    {
                        _collider.isTrigger = true; // Collider를 Trigger로 변경
                    }
                    var trigger = gameObject.GetComponentInChildren<GunPowderTrigger>(true);
                    if (trigger != null)
                    {
                        trigger.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning("[GunPowderRelease] Missing GunPowderTrigger in children when trying to enable.");
                    }

                    // 착지 시 파티클 정지 (재생 가능하게 StopEmitting)
                    StopParticles();
                    
                    // 땅에 닿으면 Player 레이어를 ExcludeLayers에서 제거
                    int playerLayer = LayerMask.NameToLayer("Player");
                    int enemyLayer = LayerMask.NameToLayer("Enemy");
                    if (_collider != null)
                        _collider.excludeLayers &= ~(1 << playerLayer);
                    if (_rigidbody2D != null)
                        _rigidbody2D.excludeLayers &= ~(1 << playerLayer);
                }
            }
        }
    }

    
    public void SetRandomSeed(int randomSeed)
    {
        _randomSeed = randomSeed;
        Random.InitState(_randomSeed);
    }

    public void StopParticles()
    {
        if (_childParticleSystems == null || _childParticleSystems.Length == 0)
        {
            _childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
        foreach (var ps in _childParticleSystems)
        {
            if (ps == null) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public void ResumeParticles()
    {
        if (_childParticleSystems == null || _childParticleSystems.Length == 0)
        {
            _childParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
        foreach (var ps in _childParticleSystems)
        {
            if (ps == null) continue;
            ps.Play(true);
        }
    }
}
