using System.Collections.Generic;
using System.Collections;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GhostTrail : MonoBehaviour
{
    [Header("계층 참조")]
    [Tooltip("고스트 인스턴스를 순서대로 포함하는 부모 트랜스폼.")]
    [SerializeField] private Transform ghostsRoot;

    [Header("타이밍")]
    [Tooltip("플레이어의 트랜스폼 스냅샷을 기록하는 주기(초).")]
    [SerializeField] private float recordIntervalSeconds = 1f / 60f;

    [Tooltip("각 고스트 사이의 시간 지연(초). 0.03 = 30ms 간격.")]
    [SerializeField] private float delayPerGhostSeconds = 0.03f;

    [Header("시각 설정")]
    [Range(0f, 1f)]
    [SerializeField] private float firstGhostAlpha = 0.65f;

    [Range(0f, 1f)]
    [SerializeField] private float lastGhostAlpha = 0.1f;

    [Tooltip("플레이어의 SpriteRenderer.flipX 상태를 복사합니다.")]
    [SerializeField] private bool copyFlipX = true;

    [Tooltip("플레이어의 Z 회전값을 복사합니다.")]
    [SerializeField] private bool copyRotationZ = false;

    [Header("활성화")]
    [Tooltip("이 컴포넌트가 활성화되면 각 GhostSprites 그룹을 자동으로 활성화하고, 비활성화되면 비활성화합니다.")]
    [SerializeField] private bool toggleGhostObjectsActive = true;

    [Tooltip("false이면 Start에서 이 컴포넌트를 비활성화하여 꺼진 상태로 시작합니다.")]
    [SerializeField] private bool enabledOnStart = false;

    [Header("비활성화")]
    [Tooltip("컴포넌트가 꺼질 때, 앞쪽부터 마지막까지 고스트를 하나씩 숨깁니다.")]
    [SerializeField] private bool turnOffSequentially = true;

    [Tooltip("꺼짐 시퀀스 동안 각 고스트를 비활성화할 때의 지연(초).")]
    [SerializeField] private float turnOffDelayPerGhostSeconds = 1f;

    private readonly List<Transform> _ghostTransforms = new List<Transform>();
    private readonly List<SpriteRenderer[]> _ghostSpriteRendererGroups = new List<SpriteRenderer[]>();

    private struct Snapshot
    {
        public Vector3 position;
        public float rotationZ;
        public bool flipX;
    }

    // 가장 최신 스냅샷은 인덱스 0(고스트별 빠른 인덱싱용)
    private readonly List<Snapshot> _snapshotHistory = new List<Snapshot>();

    private float _recordTimer;
    private int _stepsBetweenGhosts = 2; // 타이밍으로부터 계산됨
    private int _historyCapacity = 32;   // 고스트 수로부터 계산됨

    private PlayerStat _playerStat; // flipX를 위해 SpriteRenderer 목록을 읽는 데 사용
    private CancellationTokenSource _turnOffCancellationTokenSource;

    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();

        if (ghostsRoot == null)
        {
            // 설정 작업을 줄이기 위해 자동으로 "GhostSprites"라는 자식을 찾습니다
            Transform found = transform.Find("GhostSprites");
            if (found == null)
            {
                // 또한 "GhostSprite"(마지막 's' 없음)라는 부모 컨테이너도 시도합니다
                found = transform.Find("GhostSprite");
            }
            if (found != null)
            {
                ghostsRoot = found;
            }
        }

        BuildGhostLists();
        RecomputeTimingsAndCapacity();
        // 나중에 명시적으로 활성화될 때까지 기본적으로 보이지 않도록/비활성 상태로 설정
        SetZeroAlpha();
        if (toggleGhostObjectsActive)
        {
            SetGhostGroupsActive(false);
        }
    }

    private void Start()
    {
        if (!enabledOnStart)
        {
            enabled = false; // 토글될 때까지 꺼진 상태로 시작(예: 점프 대시)
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying == false)
        {
            // 에디터에서 알파 미리보기가 가능하도록 유지
            BuildGhostLists();
            ApplyInitialAlphas();
        }
    }

    private void Update()
    {
        if (ghostsRoot == null || _ghostTransforms.Count == 0)
        {
            return;
        }

        _recordTimer += Time.deltaTime;
        while (_recordTimer >= recordIntervalSeconds)
        {
            _recordTimer -= recordIntervalSeconds;
            RecordSnapshot();
        }

        // 히스토리를 기반으로 각 고스트를 구동
        for (int i = 0; i < _ghostTransforms.Count; i++)
        {
            int historyIndex = (i + 1) * _stepsBetweenGhosts; // ghost[0]은 플레이어 바로 다음으로 최신
            if (historyIndex < _snapshotHistory.Count)
            {
                Snapshot snap = _snapshotHistory[historyIndex];
                Transform ghost = _ghostTransforms[i];
                Vector3 pos = snap.position;
                pos.z = ghost.position.z; // 고스트의 원래 Z 순서를 유지
                ghost.position = pos;

                if (copyRotationZ)
                {
                    Vector3 eul = ghost.eulerAngles;
                    eul.z = snap.rotationZ;
                    ghost.eulerAngles = eul;
                }

                if (copyFlipX)
                {
                    SpriteRenderer[] renderers = _ghostSpriteRendererGroups[i];
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        if (renderers[r] != null)
                        {
                            renderers[r].flipX = snap.flipX;
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        if (_turnOffCancellationTokenSource != null)
        {
            _turnOffCancellationTokenSource.Cancel();
            _turnOffCancellationTokenSource.Dispose();
            _turnOffCancellationTokenSource = null;
        }
        BuildGhostLists();
        RecomputeTimingsAndCapacity();
        PrewarmHistory();
        SnapGhostsToPlayerNow();
        ApplyInitialAlphas();
        if (toggleGhostObjectsActive)
        {
            SetGhostGroupsActive(true);
        }
    }

    private void OnDisable()
    {
        if (_turnOffCancellationTokenSource != null)
        {
            _turnOffCancellationTokenSource.Cancel();
            _turnOffCancellationTokenSource.Dispose();
            _turnOffCancellationTokenSource = null;
        }
        SetZeroAlpha();
        if (toggleGhostObjectsActive)
        {
            SetGhostGroupsActive(false);
        }
        ClearHistory();
    }

    public void TurnOffSequentiallyThenDisable()
    {
        if (!isActiveAndEnabled)
        {
            // 이미 비활성 또는 활성 아님: 꺼진 상태를 보장
            SetZeroAlpha();
            if (toggleGhostObjectsActive)
            {
                SetGhostGroupsActive(false);
            }
            ClearHistory();
            return;
        }
        if (_turnOffCancellationTokenSource != null)
        {
            _turnOffCancellationTokenSource.Cancel();
            _turnOffCancellationTokenSource.Dispose();
        }
        _turnOffCancellationTokenSource = new CancellationTokenSource();
        TurnOffSequenceThenDisable(_turnOffCancellationTokenSource.Token).Forget();
    }

    private async UniTask TurnOffSequenceThenDisable(CancellationToken cancellationToken)
    {
        if (_ghostSpriteRendererGroups.Count == 0)
        {
            if (toggleGhostObjectsActive)
            {
                SetGhostGroupsActive(false);
            }
            ClearHistory();
            if (_turnOffCancellationTokenSource != null)
            {
                _turnOffCancellationTokenSource.Dispose();
                _turnOffCancellationTokenSource = null;
            }
            enabled = false;
            return;
        }

        for (int i = _ghostSpriteRendererGroups.Count - 1; i >= 0; i--)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            SpriteRenderer[] renderers = _ghostSpriteRendererGroups[i];
            for (int r = 0; r < renderers.Length; r++)
            {
                if (renderers[r] == null) continue;
                Color c = renderers[r].color;
                c.a = 0f;
                renderers[r].color = c;
            }

            if (toggleGhostObjectsActive && i < _ghostTransforms.Count)
            {
                Transform t = _ghostTransforms[i];
                if (t != null && t.gameObject.activeSelf)
                {
                    t.gameObject.SetActive(false);
                }
            }

            if (turnOffDelayPerGhostSeconds > 0f)
            {
                await UniTask.WaitForSeconds(turnOffDelayPerGhostSeconds, cancellationToken: cancellationToken);
            }
            else
            {
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            ClearHistory();
            enabled = false;
        }

        if (_turnOffCancellationTokenSource != null)
        {
            _turnOffCancellationTokenSource.Dispose();
            _turnOffCancellationTokenSource = null;
        }
    }

    private void RecordSnapshot()
    {
        bool flipXNow = false;
        if (copyFlipX && _playerStat != null && _playerStat.MySpriteREndererList != null && _playerStat.MySpriteREndererList.Count > 0)
        {
            // 첫 번째 스프라이트 렌더러를 기준 flip 소스로 사용
            SpriteRenderer sr = _playerStat.MySpriteREndererList[0];
            if (sr != null)
            {
                flipXNow = sr.flipX;
            }
        }

        Snapshot snapshot = new Snapshot
        {
            position = transform.position,
            rotationZ = transform.eulerAngles.z,
            flipX = flipXNow
        };

        _snapshotHistory.Insert(0, snapshot);
        if (_snapshotHistory.Count > _historyCapacity)
        {
            _snapshotHistory.RemoveAt(_snapshotHistory.Count - 1);
        }
    }

    private void BuildGhostLists()
    {
        _ghostTransforms.Clear();
        _ghostSpriteRendererGroups.Clear();

        if (ghostsRoot == null)
        {
            return;
        }

        // 전략 1: 루트 하위에 이름이 "GhostSprites*"인 트랜스폼이 있으면
        // 각 트랜스폼을 하나의 고스트 그룹으로 취급합니다(루트가 일치하는 경우 루트 포함).
        List<Transform> namedGroups = new List<Transform>();
        Transform[] allInSubtree = ghostsRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allInSubtree.Length; i++)
        {
            Transform t = allInSubtree[i];
            if (t.name.StartsWith("GhostSprites"))
            {
                namedGroups.Add(t);
            }
        }

        if (namedGroups.Count > 0)
        {
            foreach (Transform group in namedGroups)
            {
                SpriteRenderer[] renderers = group.GetComponentsInChildren<SpriteRenderer>(true);
                if (renderers != null && renderers.Length > 0)
                {
                    _ghostTransforms.Add(group);
                    _ghostSpriteRendererGroups.Add(renderers);
                }
            }
        }
        else
        {
            // 전략 2(대체): 각 직속 자식을 고스트 인스턴스로 간주
            for (int i = 0; i < ghostsRoot.childCount; i++)
            {
                Transform child = ghostsRoot.GetChild(i);
                SpriteRenderer[] renderers = child.GetComponentsInChildren<SpriteRenderer>(true);
                if (renderers != null && renderers.Length > 0)
                {
                    _ghostTransforms.Add(child);
                    _ghostSpriteRendererGroups.Add(renderers);
                }
            }
        }
    }

    private void RecomputeTimingsAndCapacity()
    {
        recordIntervalSeconds = Mathf.Max(0.001f, recordIntervalSeconds);
        delayPerGhostSeconds = Mathf.Max(0.001f, delayPerGhostSeconds);

        _stepsBetweenGhosts = Mathf.Max(1, Mathf.RoundToInt(delayPerGhostSeconds / recordIntervalSeconds));
        _historyCapacity = Mathf.Max(8, (_ghostTransforms.Count + 1) * _stepsBetweenGhosts + 2);

        if (_snapshotHistory.Capacity < _historyCapacity)
        {
            _snapshotHistory.Capacity = _historyCapacity;
        }
    }

    private void ApplyInitialAlphas()
    {
        if (_ghostTransforms.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _ghostSpriteRendererGroups.Count; i++)
        {
            float t = _ghostSpriteRendererGroups.Count > 1 ? (float)i / (float)(_ghostSpriteRendererGroups.Count - 1) : 1f;
            float alpha = Mathf.Lerp(firstGhostAlpha, lastGhostAlpha, t);
            SpriteRenderer[] renderers = _ghostSpriteRendererGroups[i];
            for (int r = 0; r < renderers.Length; r++)
            {
                if (renderers[r] == null) continue;
                Color c = renderers[r].color;
                c.a = alpha;
                renderers[r].color = c;
            }
        }
    }

    private void SetZeroAlpha()
    {
        for (int i = 0; i < _ghostSpriteRendererGroups.Count; i++)
        {
            SpriteRenderer[] renderers = _ghostSpriteRendererGroups[i];
            for (int r = 0; r < renderers.Length; r++)
            {
                if (renderers[r] == null) continue;
                Color c = renderers[r].color;
                c.a = 0f;
                renderers[r].color = c;
            }
        }
    }

    private void SetGhostGroupsActive(bool active)
    {
        for (int i = 0; i < _ghostTransforms.Count; i++)
        {
            Transform t = _ghostTransforms[i];
            if (t != null && t.gameObject.activeSelf != active)
            {
                t.gameObject.SetActive(active);
            }
        }
    }

    private Snapshot CreateCurrentSnapshot()
    {
        bool flipXNow = false;
        if (copyFlipX && _playerStat != null && _playerStat.MySpriteREndererList != null && _playerStat.MySpriteREndererList.Count > 0)
        {
            SpriteRenderer sr = _playerStat.MySpriteREndererList[0];
            if (sr != null)
            {
                flipXNow = sr.flipX;
            }
        }

        return new Snapshot
        {
            position = transform.position,
            rotationZ = transform.eulerAngles.z,
            flipX = flipXNow
        };
    }

    private void PrewarmHistory()
    {
        ClearHistory();
        Snapshot now = CreateCurrentSnapshot();
        int warmCount = _historyCapacity;
        for (int i = 0; i < warmCount; i++)
        {
            _snapshotHistory.Insert(0, now);
        }
    }

    private void SnapGhostsToPlayerNow()
    {
        Snapshot now = CreateCurrentSnapshot();
        for (int i = 0; i < _ghostTransforms.Count; i++)
        {
            Transform ghost = _ghostTransforms[i];
            if (ghost == null) continue;
            Vector3 p = now.position;
            p.z = ghost.position.z;
            ghost.position = p;

            if (copyRotationZ)
            {
                Vector3 e = ghost.eulerAngles;
                e.z = now.rotationZ;
                ghost.eulerAngles = e;
            }
            if (copyFlipX)
            {
                SpriteRenderer[] renderers = _ghostSpriteRendererGroups[i];
                for (int r = 0; r < renderers.Length; r++)
                {
                    if (renderers[r] != null)
                    {
                        renderers[r].flipX = now.flipX;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 저장된 히스토리를 지워 고스트가 즉시 플레이어 위치로 붙도록 합니다.
    /// </summary>
    public void ClearHistory()
    {
        _snapshotHistory.Clear();
    }
}


