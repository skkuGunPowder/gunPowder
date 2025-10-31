using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CrabSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _crabPrefab;
    [SerializeField] private List<Transform> _cranbSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float _resetTime = 30f;

    private List<GameObject> _spawnedCrabs;
    private float _timer;

    private void Awake()
    {
        _spawnedCrabs = new List<GameObject>();

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        
        SpawnCrabs();
    }

    private void Update()
    {
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
