using Photon.Pun;
using UnityEngine;
using Heathen.UnityPhysics;


public class Mortar : Bomb
{
    public const string ID = "BO0009";

    [Header("References")]
    [SerializeField] private GameObject _mortarShellPrefab;
    [SerializeField] private AudioClip _mortarFireSound;
    [SerializeField] private AudioClip _mortarDeploySound;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _barrel;

    [Header("Settings")]
    [SerializeField] private float _mortarDuration = 8f;
    [SerializeField] private int _maxAmmo = 5;
    [SerializeField] private float _firedelay = 0.5f;
    [SerializeField] private float _minAngle = 45f;
    [SerializeField] private float _maxAngle = 85f;
    [SerializeField] private float _rangeStep = 20f;

    private Player _owner;
    private Animator _animator;
    private BallisticPathLineRender _pathRenderer;
    private BallisticsData _projectileData;
    private int _currentAmmo;
    private float _currentAngle = 60f;
    private bool _isFacingRight = false;
    private float _timer;
    private float _delayTimer;
    private SuperAmorBuff _superArmorBuff;



    protected override void Init()
    {
        base.Init();
        SetStat(ID);

        _barrel.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        _currentAmmo = _maxAmmo;

        _animator = GetComponent<Animator>();
        _animator.SetFloat("Angle", _currentAngle);

        _pathRenderer = GetComponent<BallisticPathLineRender>();

        _projectileData = new BallisticsData
        {
            velocity = _muzzle.right * _stat.Speed,
            radius = 0.5f
        };


        if (PhotonView.IsMine)
        {
            _pathRenderer.enabled = true;
            _pathRenderer.projectile = _projectileData;
            _pathRenderer.start = _muzzle.position;
            _pathRenderer.continuousRun = true;
        }
        else
        {
            _pathRenderer.enabled = false;
        }

        if (transform.eulerAngles.y != 0f)
        {
            _isFacingRight = false;
        }
        else
        {
            _isFacingRight = true;
        }
    }

    protected override void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _mortarDuration)
        {
            _timer = 0f;
            RemoveMortar();
        }

        _delayTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_owner.PhotonView.IsMine)
            {
                _owner.PhotonView.RPC(nameof(_owner.RPC_ChangeState), RpcTarget.All, nameof(PlayerJumpState));
            }

            RemoveMortar();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_isFacingRight)
            {
                _currentAngle = Mathf.Min(_maxAngle, _currentAngle + _rangeStep * Time.deltaTime);
            }
            else
            {
                _currentAngle = Mathf.Max(_minAngle, _currentAngle - _rangeStep * Time.deltaTime);
            }

            _barrel.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

            _projectileData.velocity = _muzzle.right * _stat.Speed;
            _pathRenderer.projectile = _projectileData;
            _pathRenderer.start = _muzzle.position;
            _animator.SetFloat("Angle", _currentAngle);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_isFacingRight)
            {
                _currentAngle = Mathf.Max(_minAngle, _currentAngle - _rangeStep * Time.deltaTime);
            }
            else
            {
                _currentAngle = Mathf.Min(_maxAngle, _currentAngle + _rangeStep * Time.deltaTime);
            }

            _barrel.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);

            _projectileData.velocity = _muzzle.right * _stat.Speed;
            _pathRenderer.projectile = _projectileData;
            _pathRenderer.start = _muzzle.position;
            _animator.SetFloat("Angle", _currentAngle);
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z))
        {
            if(_delayTimer < _firedelay)
            {
                return;
            }
            _delayTimer = 0f;

            if (_mortarFireSound != null)
            {
                SoundManager.Instance.PlayLocalSound(_mortarFireSound.name, transform);
            }


            if (PhotonView.IsMine)
            {
                GameObject mortarShellObject = PhotonNetwork.Instantiate(_mortarShellPrefab.name, _muzzle.position, _muzzle.rotation);
                MortarShell mortarShell = mortarShellObject.GetComponent<MortarShell>();
                if (mortarShell.PhotonView.IsMine)
                {
                    mortarShell.PhotonView.RPC(nameof(mortarShell.ThrowBomb), RpcTarget.All, _muzzle.right, _muzzle.up, _muzzle.forward);
                    mortarShell.PhotonView.RPC(nameof(mortarShell.SetOwner), RpcTarget.All, _owner.PhotonView.ViewID);
                }
            }

            _owner.ResetGunPowderDecreaseWithoutAttackTimer();
        
            _currentAmmo--;
            if(_currentAmmo <= 0)
            {
                RemoveMortar();
            }
        }
    }

    private void RemoveMortar()
    {
        if (PhotonView.IsMine)
        {
            InputHandler.BlockInput = false;
            _superArmorBuff.EndBuff();

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
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if (_ownerPhotonview != null)
        {
            _owner = _ownerPhotonview.GetComponent<Player>();
        }

        if(_owner.PlayerFSM.IsCurrentState<PlayerJumpState>() || _owner.PlayerFSM.IsCurrentState<PlayerFallState>() || _owner.PlayerFSM.IsCurrentState<PlayerJumpDashState>())
        {
            RemoveMortar();
            return;
        }

        if (_mortarDeploySound != null)
        {
            SoundManager.Instance.PlayLocalSound(_mortarDeploySound.name, transform);
        }

        if (PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
            _superArmorBuff = BuffManager.Instance.GetBuff("BF0003", _owner) as SuperAmorBuff;
            _owner.PlayerBuffHandler.AddBuff(_superArmorBuff);
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
