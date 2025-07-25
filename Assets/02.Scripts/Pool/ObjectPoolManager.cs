using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;


[System.Serializable]
public class Pool
{
    public GameObject Prefab;
    public int MinSize;
    public int MaxSize;
}

[RequireComponent(typeof(PhotonView))]
public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public List<Pool> Pools;
    private Dictionary<string, ObjectPool<GameObject>> _poolDict;

    protected override void Awake()
    {
        base.Awake();
        _poolDict = new Dictionary<string, ObjectPool<GameObject>>();
        foreach(var pool in Pools)
        {
            ObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject newObject = PhotonNetwork.Instantiate(pool.Prefab.name, 
                        Vector3.zero, Quaternion.identity);
                    
                    // gameObject하위에 추가
                    newObject.transform.SetParent(transform);
                    newObject.SetActive(false);
                    return newObject;
                },
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => PhotonNetwork.Destroy(obj),
                collectionCheck: false,
                defaultCapacity: pool.MinSize,
                maxSize: pool.MaxSize
            );
            _poolDict.Add(pool.Prefab.name, objectPool);
        }
    }

    public GameObject GetObject(string prefabName)
    {
        if( _poolDict.ContainsKey(prefabName))
        {
            return _poolDict[prefabName].Get();
        }
        Debug.Log("No pool with prefab name: " + prefabName);
        return null;
    }

    public void ReleaseObject(string prefabName, GameObject obj)
    {
        if (_poolDict.ContainsKey(prefabName))
        {
            _poolDict[prefabName].Release(obj);
        }
        else
        {
            Debug.LogWarning("No Pool with prefab name : " + prefabName);
        }
    }
}
