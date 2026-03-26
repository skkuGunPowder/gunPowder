using Photon.Pun;
using UnityEngine;

public class AirDropSpawner : MonoBehaviour, IGimmick
{
    [Header("스포닝 설정")]
    [SerializeField] private float _spawnInterval = 30f;
    [SerializeField] private float _spawnChance = 0.1f;
    [SerializeField] private GameObject _airDropJetPrefab;

    private float _timer;
    private bool _isGimmickActive = false;

    public GimmickType Type => GimmickType.AirDropSpawner;
    public GimmickGroupType GroupType => GimmickGroupType.During;
    public bool IsActive => _isGimmickActive;

    public void Activate()
    {
        _isGimmickActive = true;
        _timer = 0f;
    }

    public void Deactivate()
    {
        _isGimmickActive = false;
    }

    private void Start()
    {
        if (GimmickManager.Instance != null)
            GimmickManager.Instance.Register(this);
    }

    private void Update()
    {
        if (!_isGimmickActive) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.Instance.CurrentGameState != EGameState.Playing) return;

        _timer += Time.deltaTime;
        if (_timer > _spawnInterval)
        {
            _timer = 0f;
            if (Random.Range(0f, 1.0f) <= _spawnChance)
            {
                PhotonNetwork.Instantiate(_airDropJetPrefab.name, transform.position, Quaternion.identity);
            }
        }
    }

    private void OnDestroy()
    {
        if (GimmickManager.Instance != null)
            GimmickManager.Instance.Unregister(this);
    }
}
