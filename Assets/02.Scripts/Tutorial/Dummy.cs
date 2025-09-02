using UnityEngine;
using DG.Tweening;

public class Dummy : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;

    // 폭발 임펄스 감지 파라미터
    private const float EXPLOSION_IMPULSE_THRESHOLD = 4.5f; // 한 프레임에서 이 이상 속도 변화 시 폭발로 간주
    private const float EXPLOSION_COOLDOWN = 0.25f;          // 연속 감지 방지 쿨다운 (초)

    // 플레이어와 유사한 감쇠(마찰) 변화 파라미터
    private const float MIN_LINEAR_DAMPING = 0.01f;
    private const float MAX_LINEAR_DAMPING = 2.5f;
    private const float DAMPING_LERP_START = 0.5f;
    private const float DUMMY_DAMAGED_TIME = 0.6f; // 더미는 간단히 고정 시간 사용
    private const float UPWARD_FORCE = 10f;

    private float _originalLinearDamping;
    private Tween _dampingTween;

    private Vector2 _previousVelocity;
    private float _lastImpulseTime;

    [SerializeField] private GameObject _gunpowderPrefab;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _lastImpulseTime = -999f;
    }

    private void OnEnable()
    {
        if (_rigidbody2D != null)
        {
            _previousVelocity = _rigidbody2D.linearVelocity;
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.z != 0f)
        {
            pos.z = 0f;
            transform.position = pos;
        }
    }

    void FixedUpdate()
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        Vector2 currentVelocity = _rigidbody2D.linearVelocity;
        Vector2 deltaV = currentVelocity - _previousVelocity;

        // 폭발로 인한 임펄스(Impulse) 추정: 한 물리프레임 내 큰 속도 변화 발생 시 처리
        if (deltaV.magnitude >= EXPLOSION_IMPULSE_THRESHOLD &&
            (Time.time - _lastImpulseTime) >= DUMMY_DAMAGED_TIME)
        {
            _lastImpulseTime = Time.time;
            OnExplosionImpact();
        }

        _previousVelocity = currentVelocity;
    }

    private void ApplyTemporaryDampingEffect()
    {
        if (_rigidbody2D == null) return;

        _rigidbody2D.AddForce(Vector2.up * UPWARD_FORCE, ForceMode2D.Impulse);

        _dampingTween?.Kill();
        _originalLinearDamping = _rigidbody2D.linearDamping;

        // 시작 감쇠값 설정 후 일정 시간 동안 MAX까지 보간
        _rigidbody2D.linearDamping = DAMPING_LERP_START;
        _dampingTween = DOTween.To(
            () => _rigidbody2D.linearDamping,
            x => _rigidbody2D.linearDamping = x,
            MAX_LINEAR_DAMPING,
            DUMMY_DAMAGED_TIME
        ).SetEase(Ease.InOutBack)
        .OnComplete(() => {
            _rigidbody2D.linearDamping = _originalLinearDamping;
        });
    }

    // 폭발을 맞았을 때 실행할 처리 (Explosion 수정 없이 내부 감지로 호출)
    private void OnExplosionImpact()
    {
        // 로직 호출 가능 (예: 사운드, 이펙트, 애니메이션 등)\
        ApplyTemporaryDampingEffect();
        // 사운드
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 3, 0f, false, SoundType.SFX, true, 1f, 50f);
        // 애니메이션
        //_animator.SetTrigger("Damaged");

        // 건파우더 낙출
        ReleaseGunPowder();

    }

    // 외부에서 수동 트리거를 원할 때 호출할 수 있는 공개 메서드
    public void TriggerExplosionEffect()
    {
        OnExplosionImpact();
    }

    private void OnDisable()
    {
        if (_dampingTween != null && _dampingTween.IsActive())
        {
            _dampingTween.Kill();
        }
        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearDamping = _originalLinearDamping;
        }
    }

    private void ReleaseGunPowder()
    {
        Instantiate(_gunpowderPrefab, transform.position, Quaternion.identity);
    }
}
