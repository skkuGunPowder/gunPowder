using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrabSpawner : MonoBehaviour, IGimmick
{
    [Header("References")]
    [SerializeField] private GameObject _crabPrefab;
    [SerializeField] private List<Transform> _cranbSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float _resetTime = 30f;

    private List<GameObject> _spawnedCrabs;
    private float _timer;

    private bool _isGimmickActive = false;

    public GimmickType Type => GimmickType.CrabSpawner;
    public GimmickGroupType GroupType => GimmickGroupType.During;
    public bool IsActive => _isGimmickActive;

    public void Activate()
    {
        _isGimmickActive = true;
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnCrabs();
        }
    }

    public void Deactivate()
    {
        _isGimmickActive = false;
        if (PhotonNetwork.IsMasterClient)
        {
            DestroyAllCrabs();
        }
    }

    private void DestroyAllCrabs()
    {
        foreach (GameObject crab in _spawnedCrabs)
        {
            if (crab != null)
            {
                PhotonNetwork.Destroy(crab);
            }
        }
        _spawnedCrabs.Clear();
    }

    private void Awake()
    {
        _spawnedCrabs = new List<GameObject>();
    }

    private void Start()
    {
        if (GimmickManager.Instance != null)
            GimmickManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (GimmickManager.Instance != null)
            GimmickManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if (!_isGimmickActive) return;

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        _timer += Time.deltaTime;
        if(_timer >= _resetTime)
        {
            ResetCrabs();
            _timer = 0f;
        }
    }

    private void SpawnCrabs()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        foreach (Transform point in _cranbSpawnPoint)
        {
            GameObject crab = PhotonNetwork.Instantiate(_crabPrefab.name, point.position, Quaternion.identity);
            _spawnedCrabs.Add(crab);
        }
    }

    private void ResetCrabs()
    {
        for(int i=0; i<_cranbSpawnPoint.Count; i++)
        {
            if (_spawnedCrabs[i] == null && PhotonNetwork.IsMasterClient)
            {
                _spawnedCrabs[i] = PhotonNetwork.Instantiate(_crabPrefab.name, _cranbSpawnPoint[i].position, Quaternion.identity);
                continue;
            }
            _spawnedCrabs[i].transform.position = _cranbSpawnPoint[i].position;
        }
    }
}
