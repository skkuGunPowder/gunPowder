
using System.Collections;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class SuicideBomb : Bomb
{
    public const string ID = "BO0018";

    [Header("References")]
    [SerializeField] private AudioClip _suicideBombSound;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _redSprite;

    [Header("Settings")]
    [SerializeField] private int _totalBeats = 5;

    private SpriteRenderer _spriteRenderer;
    private Player _owner;

    private float _remainingTime;
    private int _beatCount;
    private float _timer;
    private float _interval;
    private bool _isBombActive = false;

    private IEnumerator _bombCoroutine;


    protected override void Init()
    {
        base.Init();
        SetStat(ID);

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _bombCoroutine = BombRoutine();
        
        // 기존 트윈 정리
        CleanupTweens();
    }

    protected override void Update()
    {
        base.Update();

        CheckDirection();
    }

    public override void PauseBomb()
    {
        base.PauseBomb();
        StopCoroutine(_bombCoroutine);
    }

    public override void ResumeBomb()
    {
        base.ResumeBomb();
        StartCoroutine(_bombCoroutine);
    }

    private void CheckDirection()
    {
        if (_owner.PlayerStat.MySpriteREndererList[0].flipX == true)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }
    }

    private void Beep(int beatCount)
    {
        if (_suicideBombSound != null)
        {
            Sound sound = SoundManager.Instance.PlayLocalSound(_suicideBombSound.name, transform);
            AudioSource audioSource = sound.GetAudioSource();
            audioSource.pitch = 1f + (beatCount * 0.1f);
        }

        if (beatCount <= 4)
        {
            transform.DOScale(Vector3.one * 1.2f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetTarget(transform);
        }
        else
        {
            transform.DOScale(Vector3.one * 1.2f, 0.1f)
                .SetEase(Ease.OutBack)
                .SetTarget(transform);
        }

        _spriteRenderer.sprite = _redSprite;
        DOVirtual.DelayedCall(0.2f, () => 
        {
            if (_spriteRenderer != null && gameObject != null)
            {
                _spriteRenderer.sprite = _defaultSprite;
            }
        }).SetTarget(transform);
    }

    private IEnumerator BombRoutine()
    {
        float remainingTime = _stat.FuzeTime;
        int beatCount = 0;

        while (remainingTime > 0f)
        {
            beatCount++;

            Beep(beatCount);

            float interval = remainingTime / Mathf.Max(1, _totalBeats - beatCount + 1);

            yield return new WaitForSeconds(interval);
            remainingTime -= interval;
        }

        if (photonView.IsMine)
        {
            photonView.RPC(nameof(Explode), RpcTarget.All);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _owner = _ownerPhotonview.GetComponent<Player>();
        CheckDirection();
        transform.position = _ownerPhotonview.transform.position;
        transform.parent = _owner.transform;
        StartCoroutine(_bombCoroutine);
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        PlaceBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void Explode()
    {
        CleanupTweens();
        base.Explode();
    }

    private void OnDisable()
    {
        CleanupTweens();
    }

    private void OnDestroy()
    {
        CleanupTweens();
    }

    private void CleanupTweens()
    {
        // transform에 연결된 모든 DOTween 정리
        if (transform != null)
        {
            transform.DOKill();
        }
        // 코루틴 정리
        if (_bombCoroutine != null)
        {
            StopCoroutine(_bombCoroutine);
        }
    }
}
