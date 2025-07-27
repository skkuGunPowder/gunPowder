using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ExplosionPoolManager : Singleton<ExplosionPoolManager>
{
    [SerializeField] private List<GameObject> explosionPrefabs;
    [SerializeField] private int minSize = 10;
    [SerializeField] private int maxSize = 40;

    private Dictionary<GameObject, ObjectPool<GameObject>> _poolDictionary;

    protected override void Awake()
    {
        base.Awake();

        _poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach (GameObject prefab in explosionPrefabs)
        {
            if (prefab == null) continue;

            var pool = new ObjectPool<GameObject>(
                () => CreateObject(prefab),
                OnGetPool,
                OnReleasePool,
                OnDestroyPool,
                false,
                minSize,
                maxSize
            );

            _poolDictionary.Add(prefab, pool);
        }
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        return obj;
    }

    private void OnGetPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnReleasePool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyPool(GameObject obj)
    {
        Destroy(obj);
    }

    public GameObject GetExplosion(GameObject prefab)
    {
        if (_poolDictionary.TryGetValue(prefab, out var pool))
        {
            return pool.Get();
        }

        Debug.LogWarning($"Explosion prefab not registered: {prefab.name}");
        return null;
    }

    public void ReleaseExplosion(GameObject prefab, GameObject obj)
    {
        if (_poolDictionary.TryGetValue(prefab, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogWarning($"Explosion prefab not registered: {prefab.name}");
            Destroy(obj);
        }
    }
}
