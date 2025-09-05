using Photon.Pun;
using UnityEngine;
using Heathen.UnityPhysics;
using UnityEditor.Search;


public class Mortar : Bomb
{
    public const string ID = "BO0009";

    [Header("References")]
    [SerializeField] private GameObject _mortarShellPrefab;
    [SerializeField] private AudioClip _mortarFireSound;
    [SerializeField] private AudioClip _mortarInstallSound;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _barrel;


    [Header("Settings")]
    [SerializeField] private float _mortarDuration = 8f;
    [SerializeField] private float _minRange = 10f;
    [SerializeField] private float _maxRange = 50f;
    [SerializeField] private float _rangeStep = 5f;
    [SerializeField] private bool _useHighAngle = true; // true=고각, false=저각

    private PhotonView _photonView;
    private BallisticPathLineRender _pathRenderer;
    private BallisticsData _projectileData;

    [Header("Runtime")]
    [SerializeField] private float _targetRange;


    protected override void Init()
    {
        base.Init();
        // SetStat(ID);

        _pathRenderer = GetComponent<BallisticPathLineRender>();

        _projectileData = new BallisticsData
        {
            velocity = _muzzle.right * 10f,
            radius = 1f
        };

        _pathRenderer.projectile = _projectileData;
        _pathRenderer.start = _muzzle.position;

        _pathRenderer.Simulate();
        _pathRenderer.continuousRun = true;
    }

    protected override void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            _targetRange = Mathf.Max(_minRange, _targetRange - _rangeStep * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            _targetRange = Mathf.Min(_maxRange, _targetRange + _rangeStep * Time.deltaTime);
        }
        
        // 거리 → 각도 계산
        float angle = CalculateLaunchAngle(_targetRange, _projectileData.Speed, Mathf.Abs(Physics.gravity.y), _useHighAngle);

        _barrel.localRotation = Quaternion.Euler(0f, 0f, angle);

        _pathRenderer.start = _muzzle.position;
        _projectileData.Rotation = _muzzle.rotation;
        _pathRenderer.projectile = _projectileData;


        if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Z))
        {
            GameObject mortarShellObject = PhotonNetwork.Instantiate(_mortarShellPrefab.name, _muzzle.position, _muzzle.rotation);
            MortarShell mortarShell = mortarShellObject.GetComponent<MortarShell>();
            if (_photonView.IsMine)
            {
                _photonView.RPC(nameof(mortarShell.ThrowBomb), RpcTarget.All, _muzzle.right, _muzzle.up, _muzzle.forward);
            }
        }
    }

    private float CalculateLaunchAngle(float range, float speed, float gravity, bool highAngle)
    {
        float value = (range * gravity) / (speed * speed);
        if (value > 1f || value < -1f)
        {
            return 45f; // 발사 불가능한 경우 기본값 반환
        }


        float asin = Mathf.Asin(value);
        if (float.IsNaN(asin))
        {
            return 45f;
        }

        float angleRad = 0.5f * asin;

        // 두 가지 해 중 선택
        if (highAngle)
        {
            angleRad = 0.5f * (Mathf.PI - Mathf.Asin(value));
        }

        return angleRad * Mathf.Rad2Deg;
    }

    [PunRPC]
    public override void PlaceBomb(Vector3 fireRightDirection, Vector3 fireUpDrection, Vector3 fireFowordDirection)
    {
        if (_mortarFireSound != null)
        {
            SoundManager.Instance.PlayLocalSound(nameof(_mortarInstallSound), transform);
        }

        // TODO
        if (PhotonView.IsMine)
        {
            InputHandler.BlockInput = true;
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
