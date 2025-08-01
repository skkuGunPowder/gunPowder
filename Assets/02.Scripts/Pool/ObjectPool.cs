using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Behaviour
{
    private Dictionary<string, T> _prefabDict;
    private Dictionary<string, Queue<T>> _poolDict;
    private Transform _parent;

    public ObjectPool(List<T> prefabList, int initialSize, Transform parent = null)
    {
        _prefabDict = new Dictionary<string, T>();
        _poolDict = new Dictionary<string, Queue<T>>();
        _parent = parent;

        // 프리팹 리스트를 이름 기반 딕셔너리로 변환
        foreach (var prefab in prefabList)
        {
            if (prefab == null)
                continue;

            string key = prefab.name;
            if (!_prefabDict.ContainsKey(key))
            {
                _prefabDict[key] = prefab;

                Queue<T> queue = new Queue<T>();
                for (int i = 0; i < initialSize; i++)
                {
                    T obj = Object.Instantiate(prefab, parent);
                    obj.gameObject.SetActive(false);
                    queue.Enqueue(obj);
                }

                _poolDict[key] = queue;
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] 중복된 프리팹 이름: {key}");
            }
        }
    }

    public T Get(string name)
    {
        if (!_prefabDict.ContainsKey(name))
        {
            Debug.LogError($"[ObjectPool] 프리팹 이름 '{name}'이 존재하지 않습니다.");
            return null;
        }

        if (!_poolDict.ContainsKey(name))
        {
            _poolDict[name] = new Queue<T>();
        }

        Queue<T> queue = _poolDict[name];

        if (queue.Count > 0)
        {
            T obj = queue.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T prefab = _prefabDict[name];
            T obj = Object.Instantiate(prefab, _parent);
            return obj;
        }
    }

    public void Return(string name, T obj)
    {
        obj.gameObject.SetActive(false);

        if (!_poolDict.ContainsKey(name))
        {
            _poolDict[name] = new Queue<T>();
        }

        _poolDict[name].Enqueue(obj);
    }
}
