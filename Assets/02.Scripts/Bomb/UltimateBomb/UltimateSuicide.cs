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
        // Owner와 PhotonView null 체크
        if (_ownerPhotonview != null && _owner != null)
        {
            // Transform 직접 수정 대신 부모로 설정 (PlaceBomb에서 이미 설정함)
            // 매 프레임 위치 업데이트는 부모 관계로 자동 처리됨

            if (_owner.PlayerStat.MySpriteREndererList != null && _owner.PlayerStat.MySpriteREndererList.Count > 0)
            {
                _spriteRenderer.flipX = _owner.PlayerStat.MySpriteREndererList[0].flipX;
            }

            _jumpTimer += Time.deltaTime;
            if (_jumpTimer >= _jumpResetTime)
            {
                if (_owner.PlayerStat.JumpCount > 1)
                {
                    _owner.PlayerStat.JumpCount = 1;
                }
                _jumpTimer = 0f;
            }
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
            // 소유자만 폭발 RPC 호출
            if (PhotonView.IsMine)
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

        // Owner null 체크
        if (_ownerPhotonview != null)
        {
            explosion.Explode(_stat.IsFallingOut, _ownerPhotonview);
        }
        else
        {
            Debug.LogWarning($"[UltimateSuicide] Owner PhotonView is null");
            explosion.Explode(_stat.IsFallingOut, null);
        }

        if (_vfx != null)
        {
            _vfx.transform.SetParent(transform);
        }

        // 🔴 CRITICAL FIX: 폭발 후 오브젝트 파괴 추가
        if (PhotonView.IsMine)
        {
            if (PhotonView != null && PhotonView.ViewID != 0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[UltimateSuicide] PhotonView is invalid, destroying locally: {gameObject.name}");
                Destroy(gameObject);
            }
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
