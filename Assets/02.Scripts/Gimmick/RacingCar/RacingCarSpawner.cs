using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RacingCarSpawner : MonoBehaviour, IGimmick
{
    [SerializeField] private GameObject _racingCarPrefab;
    [SerializeField] private float _leftBound;
    [SerializeField] private float _rightBound;
    [SerializeField] private float _spawnOffsetFromEdge = 3f;

    [Header("스폰 간격")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 12f;

    private bool _isGimmickActive = false;
    private float _timer;
    private float _nextInterval;
    private bool _isGameOverHandled;

    public GimmickType Type => GimmickType.RacingCar;
    public GimmickGroupType GroupType => GimmickGroupType.HurryUp;
    public bool IsActive => _isGimmickActive;

    private void Start()
    {
        if (GimmickManager.Instance != null)
        {
            GimmickManager.Instance.Register(this);
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= HandleGameOver;
            EventManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    public void Activate()
    {
        if (_isGameOverHandled) return;

        // Playing 상태에서만 활성화 허용 (Waiting 중 도달한 HurryUp RPC는 무효)
        if (GameManager.Instance == null
            || GameManager.Instance.CurrentGameState != EGameState.Playing) return;

        _isGimmickActive = true;
        ResetTimer();
    }

    public void Deactivate()
    {
        _isGimmickActive = false;
    }

    private void HandleGameOver()
    {
        _isGameOverHandled = true;
        Deactivate();
    }

    private void Update()
    {
        EGameState state = GameManager.Instance != null
            ? GameManager.Instance.CurrentGameState
            : EGameState.Waiting;

        // Waiting(=Playing이 아닌 모든 상태) → 매 프레임 강제 비활성 + 초기화
        // _isGimmickActive를 true로 만들 수 있는 유일한 경로는 Activate() (HurryUp 선언)뿐임
        if (state != EGameState.Playing)
        {
            _isGimmickActive = false;
            _timer = 0f;
            _isGameOverHandled = false;
            return;
        }

        // Playing 상태이지만 HurryUp이 아직 선언되지 않았으면 spawn 금지
        if (!_isGimmickActive) return;
        if (!PhotonNetwork.IsMasterClient) return;

        _timer += Time.deltaTime;
        if (_timer >= _nextInterval)
        {
            SpawnRacingCar();
            ResetTimer();
        }
    }

    private void SpawnRacingCar()
    {
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

        // direction 1 = 좌측 바깥 출발 → 우측 바깥으로 (X 증가, Rotation Y = 180)
        // direction -1 = 우측 바깥 출발 → 좌측 바깥으로 (X 감소, Rotation Y = 0)
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;
        float spawnX = direction == 1
            ? _leftBound - _spawnOffsetFromEdge
            : _rightBound + _spawnOffsetFromEdge;
        Vector3 spawnPos = new Vector3(spawnX, targetY, 0f);
        Quaternion spawnRot = direction == 1
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.Euler(0f, 0f, 0f);

        object[] data = new object[] { direction, _leftBound, _rightBound, _spawnOffsetFromEdge };
        PhotonNetwork.Instantiate(_racingCarPrefab.name, spawnPos, spawnRot, 0, data);
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

        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameOver -= HandleGameOver;
        }
    }
}
