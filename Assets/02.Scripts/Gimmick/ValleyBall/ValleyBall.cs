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
    private bool _isBurning = false;
    private bool _isTourched = false;

    protected override void Init()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = NormalSprite;
        _hitCount = 0;
        _isBurning = false;
        _originalScale = transform.localScale;
        photonView.RPC(nameof(SetOwner), RpcTarget.All, photonView.ViewID);
    }

    protected override void Update()
    {
        // Update 처리 없음   
    }

    [PunRPC]
    public void TakeDamage(int damage, int maxDamage, int StealPercent, Vector3 attackerBomb, int attackerViewId, int attackerActorNumber, bool isFallingOut =false, bool isNormalAttack = false)
    {
        if(PhotonNetwork.GetPhotonView(attackerViewId).IsMine)
        {
            photonView.RPC(nameof(SetOwner), RpcTarget.All, attackerViewId);
        }

        if (_isBurning)
        {
            return;
        }

        _hitCount++;
        float ratio = (float)_hitCount / MaxHitCount;
        _renderer.color = Color.Lerp(Color.white, Color.red, ratio);
        if(_hitCount >= MaxHitCount)
        {
            _isBurning = true;
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
        _renderer.color = Color.white;
        
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(false, _ownerPhotonview);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(_isTourched)
        {
            return;
        }

        if (_wobbleTween != null && _wobbleTween.IsActive())
        {
            _wobbleTween.Kill();
        }

        if(collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            if(!_isBurning)
            {
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(Explode), RpcTarget.All);
            }
            _hitCount = 0;
        }

        if(collision.gameObject.CompareTag("RedTouch"))
        {
            _isTourched = true;
            EventManager.Instance.ScoreGoal(EInGameTeam.Red);

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
            _isTourched = true;
            EventManager.Instance.ScoreGoal(EInGameTeam.Blue);

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
            transform.DOScale(_originalScale, _wobbleDuration / 2f).SetEase(Ease.InQuad);
        });
    }
}
