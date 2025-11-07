using Com.LuisPedroFonseca.ProCamera2D;
using Heathen.UnityPhysics;
using Photon.Pun;
using UnityEngine;

public class UltimateMortar : Bomb
{
    private const string ID = "BO0010";

    [Header("References")]
    [SerializeField] private GameObject _headBombPrefab;
    [SerializeField] private AudioClip _fireSound;
    [SerializeField] private AudioClip _deploySound;
    [SerializeField] private Transform _muzzle;

    [Header("Settings")]
    [SerializeField] private float _duration = 8f;
    [SerializeField] private int _maxAmmo = 5;
    [SerializeField] private float _firedelay = 0.5f;
    [SerializeField] private float _minAngle = 0f;
    [SerializeField] private float _maxAngle = 180f;
    [SerializeField] private float _rangeStep = 20f;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float _maxZoomValue = 30f;

    private BallisticPathLineRender _pathRenderer;
    private BallisticsData _projectileData;
    private Player _owner;
    private int _currentAmmo;
    private float _currentAngle = 90f;
    private float _timer;
    private float _delayTimer;
    private SuperAmorBuff _superArmorBuff;

    private ProCamera2D _proCamera;
    private float _defaultZoom;
    private float _targetZoom;


    protected override void Init()
    {
        base.Init();
        SetStat(ID);

        _muzzle.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        float normalizedAngle = Mathf.InverseLerp(_minAngle, _maxAngle, Mathf.Abs(_currentAngle));

        _proCamera = Camera.main.GetComponent<ProCamera2D>();
        _defaultZoom = Camera.main.orthographicSize;
        _targetZoom = Mathf.Lerp(_maxZoomValue, _defaultZoom, normalizedAngle);

        _currentAmmo = _maxAmmo;

        _pathRenderer = GetComponent<BallisticPathLineRender>();

        _projectileData = new BallisticsData
        {
            velocity = _muzzle.right * _stat.Speed,
            radius = 0.5f
        };

        _pathRenderer.projectile = _projectileData;
        _pathRenderer.start = _muzzle.position;
        _pathRenderer.continuousRun = true;
    }

    protected override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration)
        {
            _timer = 0f;
            UltimateEnd();
        }

        _delayTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_owner.PhotonView.IsMine)
            {
                _owner.PhotonView.RPC(nameof(_owner.RPC_ChangeState), RpcTarget.All, nameof(PlayerJumpState));
            }

            UltimateEnd();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _currentAngle = Mathf.Min(_maxAngle, _currentAngle + _rangeStep * Time.deltaTime);

            float normalizedAngle = Mathf.InverseLerp(_minAngle, _maxAngle, Mathf.Abs(_currentAngle));
            _targetZoom = Mathf.Lerp(_maxZoomValue, _defaultZoom, normalizedAngle);

            UpdateMortar();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            _currentAngle = Mathf.Max(_minAngle, _currentAngle - _rangeStep * Time.deltaTime);

            float normalizedAngle = Mathf.InverseLerp(_minAngle, _maxAngle, Mathf.Abs(_currentAngle));
            _targetZoom = Mathf.Lerp(_maxZoomValue, _defaultZoom, normalizedAngle);

            UpdateMortar();
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z))
        {
            if (_delayTimer < _firedelay)
            {
                return;
            }
            _delayTimer = 0f;

            if (_fireSound != null)
            {
                SoundManager.Instance.PlayLocalSound(nameof(_fireSound), transform);
            }

            GameObject mortarShellObject = PhotonNetwork.Instantiate(_headBombPrefab.name, _muzzle.position, _muzzle.rotation);
            MortarShell mortarShell = mortarShellObject.GetComponent<MortarShell>();
            if (mortarShell.PhotonView.IsMine)
            {
                mortarShell.PhotonView.RPC(nameof(mortarShell.ThrowBomb), RpcTarget.All, _muzzle.right, _muzzle.up, _muzzle.forward);
            }

            _currentAmmo--;
            if (_currentAmmo <= 0)
            {
                UltimateEnd();
            }
        }
    }

    private void UpdateMortar()
    {
        _muzzle.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

        _projectileData.velocity = _muzzle.right * _stat.Speed;
        _pathRenderer.projectile = _projectileData;
        _pathRenderer.start = _muzzle.position;

        if(PhotonView.IsMine)
        {
            _proCamera.UpdateScreenSize(_targetZoom);
        }
    }

    private void UltimateEnd()
    {
        if (PhotonView.IsMine)
        {
            _proCamera.UpdateScreenSize(_defaultZoom);
            InputHandler.BlockInput = false;
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

    private void OnDestroy()
    {
        if (PhotonView.IsMine)
        {
            _proCamera.UpdateScreenSize(_defaultZoom);
        }
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if (_ownerPhotonview != null)
        {
            _owner = _ownerPhotonview.GetComponent<Player>();
        }

        if (_deploySound != null)
        {
            SoundManager.Instance.PlayLocalSound(nameof(_deploySound), transform);
        }

        if (PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
            _superArmorBuff = BuffManager.Instance.GetBuff("BF0003", _owner) as SuperAmorBuff;
            _owner.PlayerBuffHandler.AddBuff(_superArmorBuff);
            _owner.SetPausedNoAttack();
        }
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
