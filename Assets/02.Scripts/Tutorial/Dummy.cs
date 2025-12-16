using UnityEngine;
using DG.Tweening;
using RaycastPro.RaySensors2D;
using Photon.Pun;

public class Dummy : MonoBehaviourPun, IDamagable
{
    protected Animator _animator;
    private Rigidbody2D _rigidbody2D;
    [SerializeField]
    protected BoxRay2D _groundRay2D;

    // 폭발 임펄스 감지는 제거됨

    // 플레이어와 유사한 감쇠(마찰) 변화 파라미터
    private const float MIN_LINEAR_DAMPING = 0.01f;
    private const float MAX_LINEAR_DAMPING = 2.5f;
    private const float DAMPING_LERP_START = 0.5f;
    private const float DUMMY_DAMAGED_TIME = 0.6f; // 더미는 간단히 고정 시간 사용
    private const float UPWARD_FORCE = 10f;

    private float _originalLinearDamping;
    private Tween _dampingTween;
    private DamagePopup _damagePopup;



    [SerializeField] private GameObject _gunpowderPrefab;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _damagePopup = GetComponent<DamagePopup>();
        
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.None; // 보간 비활성화
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



    protected virtual void ApplyTemporaryDampingEffect()
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
        .OnComplete(() =>
        {
            _rigidbody2D.linearDamping = _originalLinearDamping;
        });
    }

    // 폭발을 맞았을 때 실행할 처리 (Explosion 수정 없이 내부 감지로 호출)
    protected virtual void OnExplosionImpact(int attackerViewId = 0)
    {
        // 로직 호출 가능 (예: 사운드, 이펙트, 애니메이션 등)\
        ApplyTemporaryDampingEffect();
        // 사운드
        SoundManager.Instance.PlayLocalRandomSound("PlayerDamage", transform, 1, 7, 0f, false, SoundType.SFX, true, 1f, 50f);
        //SoundManager.Instance.PlayLocalRandomSound("PlayerDamageVoice", transform, 1, 3, 0f, false, SoundType.SFX, true, 1f, 50f);
        // 애니메이션
        _animator.SetTrigger("Damaged");

        // 건파우더 낙출
        ReleaseGunPowder(attackerViewId);

    }

    public void TriggerExplosionEffect(int maxDamage, int damage, int attackerViewId = 0)
    {
        OnExplosionImpact(attackerViewId);
        if (_damagePopup != null && maxDamage > 0)
        {
            _damagePopup.SpawnPopup(damage, maxDamage);
        }
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

    protected virtual void ReleaseGunPowder(int attackerViewId = 0)
    {
        GameObject gunpowderObj = Instantiate(_gunpowderPrefab, transform.position, Quaternion.identity);
        TutorialGunpowder tutorialGunpowder = gunpowderObj.GetComponent<TutorialGunpowder>();
        if (tutorialGunpowder != null && attackerViewId != 0)
        {
            tutorialGunpowder.SetTargetByViewId(attackerViewId);
        }
    }

    protected virtual bool IsGrounded2D()
    {
        if (_groundRay2D == null)
        {
            return false;
        }
        _groundRay2D.Cast();
        return _groundRay2D.Performed;
    }

    public void TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut = false, bool isNormalAttack = false)
    {
        TriggerExplosionEffect(maxDamage, damage, attackerViewId);
        
        // 공격자에게 히트 파티클 생성 요청 (Player와 동일한 로직)
        // 공격자의 Owner 클라이언트에서만 한 번만 실행되도록 보장
        if (attackerViewId != 0)
        {
            PhotonView attackerView = PhotonView.Find(attackerViewId);
            if (attackerView != null && attackerView.gameObject != null && attackerView.gameObject.activeInHierarchy && attackerView.Owner != null)
            {
                // 공격자가 Player인지 확인
                Player attackerPlayer = attackerView.GetComponent<Player>();
                if (attackerPlayer != null && attackerPlayer.DamageController != null)
                {
                    // 공격자의 Owner가 로컬 플레이어일 때만 RPC 호출 (중복 방지)
                    if (attackerView.Owner == PhotonNetwork.LocalPlayer)
                    {
                        bool isCrit = (damage == maxDamage);
                        attackerView.RPC(nameof(attackerPlayer.DamageController.SpawnAttackerHitParticles), attackerView.Owner, transform.position, isCrit, photonView.ViewID);
                    }
                }
            }
        }
    }
}
