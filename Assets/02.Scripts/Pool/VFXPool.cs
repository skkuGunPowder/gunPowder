using System.Collections.Generic;
using UnityEngine;

public class VFXPool : Singleton<VFXPool>
{
    [SerializeField] private List<ParticleSystem> _particlePrefabList;
    [SerializeField] private int initialSize = 100;

    private Dictionary<string, ParticleSystem> _prefabDict;
    private Dictionary<string, Queue<ParticleSystem>> _poolDict;
    private Transform _parent;
 
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        _prefabDict = new Dictionary<string, ParticleSystem>();
        _poolDict = new Dictionary<string, Queue<ParticleSystem>>();
        _parent = transform;

        foreach (ParticleSystem prefab in _particlePrefabList)
        {
            if (prefab == null)
                continue;

            string key = prefab.name;
            if (!_prefabDict.ContainsKey(key))
            {
                _prefabDict[key] = prefab;

                Queue<ParticleSystem> queue = new Queue<ParticleSystem>();
                for (int i = 0; i < initialSize; i++)
                {
                    ParticleSystem vfx = Instantiate(prefab, _parent);
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

    public ParticleSystem Get(string vfxName)
    {
        if (!_prefabDict.ContainsKey(vfxName))
        {
            Debug.LogError($"[{this.name}] 프리팹 이름 '{vfxName}'이 존재하지 않습니다.");
            return null;
        }

        if (!_poolDict.ContainsKey(vfxName))
        {
            _poolDict[vfxName] = new Queue<ParticleSystem>();
        }

        Queue<ParticleSystem> queue = _poolDict[vfxName];

        if (queue.Count > 0)
        {
            Debug.Log("정상 호출");
            ParticleSystem vfx = queue.Dequeue();
            vfx.gameObject.SetActive(true);
            return vfx;
        }
        else
        {
            ParticleSystem prefab = _prefabDict[vfxName];
            ParticleSystem vfx = Instantiate(prefab, _parent);
            return vfx;
        }
    }

    public void Return(string vfxName, ParticleSystem vfx)
    {
        vfx.gameObject.SetActive(false);

        if (!_poolDict.ContainsKey(vfxName))
        {
            _poolDict[vfxName] = new Queue<ParticleSystem>();
        }

        _poolDict[vfxName].Enqueue(vfx);
    }
}
