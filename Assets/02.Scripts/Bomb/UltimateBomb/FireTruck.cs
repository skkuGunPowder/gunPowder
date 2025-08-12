using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Photon.Pun;

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
    [SerializeField] private float _rayDistance = 20f;
    [SerializeField] private LayerMask _groundLayer;

    private PhotonView _photonView;
    public PhotonView PhotonView => _photonView;
    private Player _owner;
    private CameraController _cameraController;
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

        _photonView = GetComponent<PhotonView>();
        _animator = GetComponentInChildren<Animator>();
        _cameraController = Camera.main.GetComponent<CameraController>();
        _spriteRenderer.color = new Color(1, 1, 1, 0);
        _damageCollider.enabled = false;
    }

    [PunRPC]
    public void Launch(int ownerPhotonViewID, bool isFacingRight)
    {
        _owner = PhotonView.Find(ownerPhotonViewID).GetComponent<Player>();
        if (isFacingRight)
        {
            _startOffsetDistance *= -1;
        }

        Summon();
    }

    public void Summon()
    {
        RaycastHit2D hit = Physics2D.Raycast(_owner.transform.position, Vector2.down, _rayDistance, _groundLayer);
        if (hit.collider != null)
        {
            _spawnPosition = hit.point;
        }
        else
        {
            _spawnPosition = _owner.transform.position;
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

        Destroy(_vfx.gameObject);
        _damageCollider.enabled = false;
        _animator.SetBool("IsAttack", false);

        Sequence seq = DOTween.Sequence();
        seq.Append(DOVirtual.DelayedCall(0.5f, () => _spriteRenderer.DOFade(0f, _fadeTime)));
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
            _cameraController.SmallShakeAt(transform, 4f);

            foreach (var target in targetsInRange)
            {
                target.TakeDamage(_damageAmount, transform.position, _owner.PhotonView.ViewID, _owner.PhotonView.OwnerActorNr);
            }
            yield return new WaitForSeconds(_damageInterval);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == _owner.gameObject)
        {
            return;    
        }
        
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
        if (collision.gameObject == _owner.gameObject)
        {
            return;    
        }

        if (collision.TryGetComponent(out IDamagable dmg))
        {
            if (targetsInRange.Contains(dmg))
            {
                targetsInRange.Remove(dmg);
            }
        }
    }
} 
