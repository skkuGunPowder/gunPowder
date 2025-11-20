using UnityEngine;
using DG.Tweening;
using Photon.Pun;


public class BasicBomb : Bomb
{
    public const string ID = "BO0001";

    [SerializeField] private float pulseScale = 2f; // 펄스 크기
    [SerializeField] private float pulseDuration = 0.1f; // 펄스 지속 시간

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
        SoundManager.Instance.PlayLocalSound(this.GetType().Name, transform, 0, true);
        Vector3 originalScale = transform.localScale;
        transform.DOScale(originalScale * pulseScale, pulseDuration / 2f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_isDestroying)
        {
            return;
        }

        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Immune")
        {
            return;
        }

        if (CheckPriority(other))
        {
            return;
        }
        
        if(photonView.IsMine)
        {
            photonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        ThrowBomb(fireRightDirection, fireUpDrection, fireFowordDirection);
    }

    [PunRPC]
    public override void ThrowBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    public override void ThrowBombStraight(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed * 2f;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }

    [PunRPC]
    public override void BoostBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if(_isDestroying)
        {
            return;
        }

        _fireDirection = fireRightDirection;

        if (photonView.IsMine)
        {
            photonView.RPC(nameof(Explode), RpcTarget.All);
        }
    }

    [PunRPC]
    public override void SmashBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        _fireDirection = fireRightDirection;
        _currentSpeed = _stat.Speed;
        _rigidBody.AddForce(_fireDirection * _currentSpeed, ForceMode2D.Impulse);
    }
}