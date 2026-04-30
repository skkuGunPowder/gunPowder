using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RacingCarSpawner : MonoBehaviour, IGimmick
{
    [SerializeField] private GameObject _racingCarPrefab;
    [SerializeField] private float _leftBound;
    [SerializeField] private float _rightBound;

    [Header("스폰 간격")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 12f;

    private bool _isGimmickActive = false;
    private float _timer;
    private float _nextInterval;

    public GimmickType Type => GimmickType.RacingCar;
    public GimmickGroupType GroupType => GimmickGroupType.During;
    public bool IsActive => _isGimmickActive;

    private void Start()
    {
        if (GimmickManager.Instance != null)
        {
            GimmickManager.Instance.Register(this);
        }
    }

    public void Activate()
    {
        _isGimmickActive = true;
        ResetTimer();
    }

    public void Deactivate()
    {
        _isGimmickActive = false;
    }

    private void Update()
    {
        if (!_isGimmickActive) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.Instance.CurrentGameState != EGameState.Playing) return;

        _timer += Time.deltaTime;
        if (_timer >= _nextInterval)
        {
            SpawnRacingCar();
            ResetTimer();
        }
    }

    private void SpawnRacingCar()
    {
        // 살아있는 플레이어 중 랜덤 1명 선택
        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        List<Player> alivePlayers = new List<Player>();
        foreach (Player p in allPlayers)
        {
            bool isDead = p.photonView.Owner.GetCustomProperty<bool>(EProperties.IsDead.ToString(), false);
            if (!isDead)
            {
                alivePlayers.Add(p);
            }
        }

        if (alivePlayers.Count == 0)
        {
            return;
        }

        Player target = alivePlayers[Random.Range(0, alivePlayers.Count)];
        float targetY = target.transform.position.y;

        // 방향 랜덤: 1 = 좌측에서 출발 → 우측으로, -1 = 우측에서 출발 → 좌측으로
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;
        float spawnX = direction == 1 ? _leftBound + 3f : _rightBound - 3f;
        Vector3 spawnPos = new Vector3(spawnX, targetY, 0f);

        object[] data = new object[] { direction, _leftBound, _rightBound };
        PhotonNetwork.Instantiate(_racingCarPrefab.name, spawnPos, Quaternion.identity, 0, data);
    }

    private void ResetTimer()
    {
        _timer = 0f;
        _nextInterval = Random.Range(_minInterval, _maxInterval);
    }

    private void OnDestroy()
    {
        if (GimmickManager.Instance != null)
        {
            GimmickManager.Instance.Unregister(this);
        }
    }
}
