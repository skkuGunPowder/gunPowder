
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


    protected override void Init()
    {
        base.Init();
        SetStat(ID);

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();

        transform.position = _ownerPhotonview.transform.position;
        if (_owner.PlayerStat.MySpriteREndererList[0].flipX == true)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }
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

        Explode();
    }

    private void Beep(int beatCount)
    {
        // 사운드 피치 변경
        if (_suicideBombSound != null)
        {
            Sound sound = SoundManager.Instance.PlayLocalSound(_suicideBombSound.name, transform);
            AudioSource audioSource = sound.GetAudioSource();
            audioSource.pitch = 1f + (beatCount * 0.1f);
        }

        // 스케일 변경
        if (beatCount <= 4)
        {
            transform.DOScale(Vector3.one * 1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            transform.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutBack);
        }

        // 스프라이트 변경
        _spriteRenderer.sprite = _redSprite;
        DOVirtual.DelayedCall(0.2f, () => _spriteRenderer.sprite = _defaultSprite);
    }


    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _owner = _ownerPhotonview.GetComponent<Player>();
        StartCoroutine(BombRoutine());
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
}
