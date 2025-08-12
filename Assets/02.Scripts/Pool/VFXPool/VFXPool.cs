using System.Collections.Generic;
using UnityEngine;

public class VFXPool : Singleton<VFXPool>
{
    [SerializeField] private List<VFX> _particlePrefabList;
    [SerializeField] private int initialSize = 30;

    private Dictionary<string, VFX> _prefabDict;
    private Dictionary<string, Queue<VFX>> _poolDict;
    private Transform _parent;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        _prefabDict = new Dictionary<string, VFX>();
        _poolDict = new Dictionary<string, Queue<VFX>>();
        _parent = transform;

        foreach (VFX prefab in _particlePrefabList)
        {
            if (prefab == null)
                continue;

            string key = prefab.name;
            if (!_prefabDict.ContainsKey(key))
            {
                _prefabDict[key] = prefab;

                Queue<VFX> queue = new Queue<VFX>();
                for (int i = 0; i < initialSize; i++)
                {
                    VFX vfx = Instantiate(prefab, _parent);
                    vfx.PoolKey = key;
                    vfx.gameObject.SetActive(false);
                    queue.Enqueue(vfx);
                }

                _poolDict[key] = queue;
            }
            else
            {
                Debug.LogWarning($"[{this.name}] 중복된 프리팹 이름: {key}");
            }
        }
    }

    public VFX Get(string vfxName)
    {
        if (!_prefabDict.ContainsKey(vfxName))
        {
            Debug.LogError($"[{this.name}] 프리팹 이름 '{vfxName}'이 존재하지 않습니다.");
            return null;
        }

        if (!_poolDict.ContainsKey(vfxName))
        {
            _poolDict[vfxName] = new Queue<VFX>();
        }

        Queue<VFX> queue = _poolDict[vfxName];

        if (queue.Count > 0)
        {
            VFX vfx = queue.Dequeue();
            vfx.PoolKey = vfxName;
            vfx.gameObject.SetActive(true);
            return vfx;
        }
        else
        {
            VFX prefab = _prefabDict[vfxName];
            VFX vfx = Instantiate(prefab, _parent);
            vfx.PoolKey = vfxName;
            return vfx;
        }
    }

    public VFX RandomGet(string vfxName, int min, int max)
    {
        string prefabName = $"{vfxName}_{Random.Range(min, max + 1)}";
        if (_prefabDict.ContainsKey(prefabName))
        {
            return Get(prefabName);
        }

        Debug.LogError($"[{this.name}] 프리팹 이름 '{prefabName}'이 존재하지 않습니다.");
        return null;
    }

    public void Return(VFX vfx)
    {
        if (vfx == null) return;

        // 풀로 복귀 시 계층 정리: 항상 풀 트랜스폼 하위로 되돌린다
        if (vfx.transform.parent != _parent)
        {
            vfx.transform.SetParent(_parent, true);
        }

        vfx.gameObject.SetActive(false);

        string key = string.IsNullOrEmpty(vfx.PoolKey) ? vfx.name : vfx.PoolKey;
        if (!_poolDict.ContainsKey(key))
        {
            _poolDict[key] = new Queue<VFX>();
        }

        _poolDict[key].Enqueue(vfx);
    }

    public void Play(string vfxName, Vector3 position)
    {
        VFX vfx = Get(vfxName);
        vfx.transform.position = position;
        vfx.Play();
    }
    
    public void RandomPlay(string vfxName, Vector3 position, int min, int max)
    {
        VFX vfx = RandomGet(vfxName, min, max);
        vfx.transform.position = position;
        vfx.Play();
    }
}
