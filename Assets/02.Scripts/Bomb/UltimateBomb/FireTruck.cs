using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FireTruck : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D _damageCollider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _vfx;


    [Header("Settings")]
    [SerializeField] private float _fadeTime = 0.5f;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _damageInterval = 0.3f;
    [SerializeField] private int _damageAmount = 3;
    [SerializeField] private Vector2 _attackRange = new Vector2(15f, 8f);
    [SerializeField] private float _startOffsetDistance = 5f;
    [SerializeField] private float _rayDistance = 10f;
    [SerializeField] private LayerMask _groundLayer;

    private Player _player;
    private Vector3 _spawnPosition;
    private Animator _animator;
    private List<IDamagable> targetsInRange = new List<IDamagable>();

    private Coroutine damageCoroutine;


    private void Awake()
    {
        if (_damageCollider is BoxCollider2D box)
        {
            box.size = _attackRange;
            box.isTrigger = true;
        }

        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer.color = new Color(1, 1, 1, 0);
        _damageCollider.enabled = false;
    }

    public void Init(Player player, bool isFacingRight)
    {
        _player = player;
        if (isFacingRight)
        {
            _startOffsetDistance *= -1;
        }
    }

    public void Summon()
    {
        RaycastHit2D hit = Physics2D.Raycast(_player.transform.position, Vector2.down, _rayDistance, _groundLayer);
        if (hit.collider != null)
        {
            Debug.LogError("땅 체크됨");
            _spawnPosition = hit.point;
        }
        else
        {
            Debug.LogError("땅 체크 안됨");
            _spawnPosition = _player.transform.position;
        }

        transform.position = _spawnPosition + new Vector3(_startOffsetDistance, 0f, 0f);
        
        Sequence seq = DOTween.Sequence();

        seq.Append(_spriteRenderer.DOFade(1f, _fadeTime));
        seq.Join(transform.DOMove(_spawnPosition, _fadeTime).SetEase(Ease.OutQuad));

        seq.AppendCallback(() =>
        {
            StartCoroutine(StartAttackCoroutine());
        });
    }

    private void StopFireTruck()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }

        _vfx.Stop();
        _damageCollider.enabled = false;
        _animator.SetBool("IsAttack", false);

        Sequence seq = DOTween.Sequence();
        seq.Append(_spriteRenderer.DOFade(0f, 0.5f));
        seq.Join(transform.DOMove(transform.position + new Vector3(_startOffsetDistance, 0, 0), _fadeTime).SetEase(Ease.OutQuad)).OnComplete(() =>
        {
            Destroy(gameObject);
        });

        targetsInRange.Clear();
    }
    
    private IEnumerator StartAttackCoroutine()
    {
        while (_animator.GetCurrentAnimatorStateInfo(0).IsName("FireTruckLanding") && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        _vfx.Play();
        _damageCollider.enabled = true;
        _animator.SetBool("IsAttack", true);
        damageCoroutine = StartCoroutine(DamageOverTime());
        DOVirtual.DelayedCall(_duration, StopFireTruck);
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            foreach (var target in targetsInRange)
            {
                target.TakeDamage(_damageAmount, transform.position, _player.PhotonView.ViewID, _player.PhotonView.OwnerActorNr);
            }
            yield return new WaitForSeconds(_damageInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable dmg))
        {
            if (!targetsInRange.Contains(dmg))
            {
                targetsInRange.Add(dmg);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamagable dmg))
        {
            if (targetsInRange.Contains(dmg))
            {
                targetsInRange.Remove(dmg);
            }
        }
    }
} 
