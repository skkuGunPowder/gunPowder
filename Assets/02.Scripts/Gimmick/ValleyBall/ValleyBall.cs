using System;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class ValleyBall : Bomb, IDamagable
{
    public Sprite NormalSprite;
    public Sprite BurningSprite;

    public int MaxHitCount = 10;

    public Action OnRedTouched;
    public Action OnBlueTouched;

    private float _wobbleAmount = 0.6f;     // 출렁이는 크기 변화 비율
    private float _wobbleDuration = 0.1f;   // 출렁이는 애니메이션 시간
    private Vector3 _originalScale;
    private Tween _wobbleTween;

    private int _hitCount;
    private SpriteRenderer _renderer;

    protected override void Init()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = NormalSprite;
        _hitCount = 0;
        _originalScale = transform.localScale;
    }

    protected override void Update()
    {
        // Update 처리 없음   
    }

    public void TakeDamage(int damage, int maxDamage, int HealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut =false, bool isNormalAttack = false)
    {
        _hitCount++;
        if(_hitCount >= MaxHitCount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public override void Explode()
    {
         if (_isDestroying)
        {
            return;
        }
        _isDestroying = true;
        transform.DOKill();

        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_wobbleTween != null && _wobbleTween.IsActive())
        {
            _wobbleTween.Kill();
        }

        if(collision.gameObject.CompareTag("Bomb"))
        {
            return;
        }

        if(collision.gameObject.CompareTag("RedTouch"))
        {
            OnRedTouched?.Invoke();
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
            }
            _isDestroying = true;
            DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
            return;
        }

        if(collision.gameObject.CompareTag("BlueTouch"))
        {
            OnBlueTouched?.Invoke();
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
            }
            _isDestroying = true;
            DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
            return;
        }

        ContactPoint2D contact = collision.contacts[0];
        Vector2 normal = transform.InverseTransformDirection(contact.normal.normalized);

        float xSquash = 1f - Mathf.Abs(normal.x) * _wobbleAmount;
        float ySquash = 1f - Mathf.Abs(normal.y) * _wobbleAmount;

        float xStretch = 1f + Mathf.Abs(normal.y) * _wobbleAmount * 0.5f;
        float yStretch = 1f + Mathf.Abs(normal.x) * _wobbleAmount * 0.5f;

        Vector3 squashedScale = new Vector3(
            _originalScale.x * xSquash * xStretch,
            _originalScale.y * ySquash * yStretch,
            _originalScale.z
        );

        _wobbleTween = transform.DOScale(squashedScale, _wobbleDuration / 2f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            transform.DOScale(_originalScale, _wobbleDuration / 2f)
                .SetEase(Ease.InQuad);
        });
    }
}
