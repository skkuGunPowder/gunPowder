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

    private BallisticPathLineRender _pathRenderer;
    private BallisticsData _projectileData;
    private Player _owner;
    private int _currentAmmo;
    private float _currentAngle = 90f;
    private float _timer;
    private float _delayTimer;


    protected override void Init()
    {
        base.Init();
        SetStat(ID);

        _owner = _ownerPhotonview.GetComponent<Player>();

        _muzzle.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
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

            _muzzle.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

            _projectileData.velocity = _muzzle.right * _stat.Speed;
            _pathRenderer.projectile = _projectileData;
            _pathRenderer.start = _muzzle.position;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            _currentAngle = Mathf.Max(_minAngle, _currentAngle - _rangeStep * Time.deltaTime);

            _muzzle.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

            _projectileData.velocity = _muzzle.right * _stat.Speed;
            _pathRenderer.projectile = _projectileData;
            _pathRenderer.start = _muzzle.position;
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

    private void UltimateEnd()
    {
        if (PhotonView.IsMine)
        {
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
    

}
