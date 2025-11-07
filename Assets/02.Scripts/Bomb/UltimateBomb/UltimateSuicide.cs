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
    private float _jumpTimer;

    protected override void Init()
    {
        base.Init();
        SetStat(ID);
    }

    protected override void Update()
    {
        transform.position = _ownerPhotonview.transform.position;
        if (_owner.PlayerStat.MySpriteREndererList[0].flipX == true)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }

        _jumpTimer += Time.deltaTime;
        if (_jumpTimer >= _jumpResetTime && _owner != null)
        {
            if (_owner.PlayerStat.JumpCount > 1)
            {
                _owner.PlayerStat.JumpCount = 1;
            }
            _jumpTimer = 0f;
        }

        _timer += Time.deltaTime;
        if (_timer >= _duration)
        {
            _timer = 0f;
            UltimateEnd();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (_ownerPhotonview.IsMine)
            {
                PhotonView.RPC(nameof(Explode), RpcTarget.All);
            }
        }
    }

    private void UltimateEnd()
    {
        // TODO
        // _ultimateMaterial OFF

        if (PhotonView.IsMine)
        {
            if (PhotonView != null && PhotonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Bomb] PhotonView is invalid, destroying locally: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }


    [PunRPC]
    public override void Explode()
    {
        Explosion explosion = ExplosionPool.Instance.Get(ExplosionPrefab.name);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
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
