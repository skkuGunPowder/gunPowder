using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Hierarchy References")]
    [Tooltip("Parent transform that contains the ghost instances in order.")]
    [SerializeField] private Transform ghostsRoot;

    [Header("Timing")]
    [Tooltip("How often to record the player's transform snapshot (seconds).")]
    [SerializeField] private float recordIntervalSeconds = 1f / 60f;

    [Tooltip("Time delay between each ghost (seconds). 0.03 = 30ms spacing.")]
    [SerializeField] private float delayPerGhostSeconds = 0.03f;

    [Header("Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float firstGhostAlpha = 0.65f;

    [Range(0f, 1f)]
    [SerializeField] private float lastGhostAlpha = 0.1f;

    [Tooltip("Copy SpriteRenderer.flipX state from the player.")]
    [SerializeField] private bool copyFlipX = true;

    [Tooltip("Copy Z rotation from the player.")]
    [SerializeField] private bool copyRotationZ = false;

    [Header("Activation")]
    [Tooltip("Automatically set each GhostSprites group active when this component is enabled, and inactive when disabled.")]
    [SerializeField] private bool toggleGhostObjectsActive = true;

    [Tooltip("If false, the component disables itself on Start so it begins OFF.")]
    [SerializeField] private bool enabledOnStart = false;

    private readonly List<Transform> _ghostTransforms = new List<Transform>();
    private readonly List<SpriteRenderer[]> _ghostSpriteRendererGroups = new List<SpriteRenderer[]>();

    private struct Snapshot
    {
        public Vector3 position;
        public float rotationZ;
        public bool flipX;
    }

    // Newest snapshot at index 0 for cheap indexing per ghost
    private readonly List<Snapshot> _snapshotHistory = new List<Snapshot>();

    private float _recordTimer;
    private int _stepsBetweenGhosts = 2; // computed from timings
    private int _historyCapacity = 32;   // computed from ghost count

    private PlayerStat _playerStat; // used to read SpriteRenderer list for flipX

    private void Awake()
    {
        _playerStat = GetComponent<PlayerStat>();

        if (ghostsRoot == null)
        {
            // Try to locate a child named "GhostSprites" automatically to reduce setup
            Transform found = transform.Find("GhostSprites");
            if (found == null)
            {
                // Also try a parent container named "GhostSprite" (without trailing 's')
                found = transform.Find("GhostSprite");
            }
            if (found != null)
            {
                ghostsRoot = found;
            }
        }

        BuildGhostLists();
        RecomputeTimingsAndCapacity();
        // Default to invisible/disabled visuals until explicitly enabled later
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
            enabled = false; // start OFF until toggled (e.g., Jump Dash)
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying == false)
        {
            // Keep alpha previewable in editor
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

        // Drive each ghost from history
        for (int i = 0; i < _ghostTransforms.Count; i++)
        {
            int historyIndex = (i + 1) * _stepsBetweenGhosts; // ghost[0] is the most recent after the player
            if (historyIndex < _snapshotHistory.Count)
            {
                Snapshot snap = _snapshotHistory[historyIndex];
                Transform ghost = _ghostTransforms[i];
                Vector3 pos = snap.position;
                pos.z = ghost.position.z; // keep ghost's original Z order
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
        SetZeroAlpha();
        if (toggleGhostObjectsActive)
        {
            SetGhostGroupsActive(false);
        }
        ClearHistory();
    }

    private void RecordSnapshot()
    {
        bool flipXNow = false;
        if (copyFlipX && _playerStat != null && _playerStat.MySpriteREndererList != null && _playerStat.MySpriteREndererList.Count > 0)
        {
            // Use the first sprite renderer as the canonical flip source
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

        // Strategy 1: If there are any transforms named like "GhostSprites*" under the root,
        // treat each of those as one ghost group (including the root itself if it matches).
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
            // Strategy 2 (fallback): assume each direct child is a ghost instance
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
    /// Clears stored history so ghosts snap back to the player immediately.
    /// Call this when teleporting.
    /// </summary>
    public void ClearHistory()
    {
        _snapshotHistory.Clear();
    }
}


