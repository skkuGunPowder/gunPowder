using Photon.Pun;
using UnityEngine;

public class UltimateSuicide : Bomb
{
    private const string ID = "BO0019";

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Settings")]
    [SerializeField] private float _duration = 12f;
    [SerializeField] private float  _jumpResetTime = 1f;

    private Player _owner;
    private float _timer;

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
    }

    protected override void Update()
    {
        if(_isDestroying)
        {
            return;
        }

        if (_owner == null)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= _duration)
        {
            _timer = 0f;
            UltimateEnd();
        }

        transform.position = _owner.transform.position;

        if (_owner.PlayerStat.MySpriteREndererList != null && _owner.PlayerStat.MySpriteREndererList.Count > 0)
        {
            _spriteRenderer.flipX = _owner.PlayerStat.MySpriteREndererList[0].flipX;
        }

        if (_owner.PlayerStat.JumpCount > 1)
        {
            _owner.PlayerStat.JumpCount = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isDestroying)
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            if (PhotonView.IsMine)
            {
                PhotonView.RPC(nameof(Explode), RpcTarget.All);
            }
        }
    }

    private void UltimateEnd()
    {
        if(_isDestroying)
        {
            return;
        }
        _isDestroying = true;

        // TODO
        // _ultimateMaterial OFF

        DestroyCollector.Instance.PhotonLazyDestory(gameObject, PhotonView);
        // if (photonView.IsMine)
        // {
        //     PhotonNetwork.Destroy(gameObject);
        // }
    }


    [PunRPC]
    public override void Explode()
    {
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;

        if (_ownerPhotonview != null)
        {
            explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
        }
        else
        {
            Debug.LogError($"[UltimateSuicide] Owner PhotonView가 없습니다!");
        }

        if (_vfx != null)
        {
            _vfx.transform.position = transform.position;
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if (_ownerPhotonview != null)
        {
            _owner = _ownerPhotonview.GetComponent<Player>();
        }

        // TODO
        // _ultimateMaterial ON
    }
}
