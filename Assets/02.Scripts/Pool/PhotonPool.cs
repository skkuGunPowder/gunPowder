using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

// 커스텀 Prefab Pool
public class PhotonPool : MonoBehaviour, IPunPrefabPool
{
    private readonly Dictionary<string, Queue<GameObject>> poolDict = new();
    private readonly Dictionary<string, int> activeCountDict = new(); // 활성화된 오브젝트 수

    [System.Serializable]
    public class PrewarmInfo
    {
        public string prefabId;
        public int initialCount = 10;
        public int maxCount = 50;
    }

    [Header("미리 생성할 프리팹 설정")]
    public List<PrewarmInfo> prewarmSettings = new();

    private void Awake()
    {
        PhotonNetwork.PrefabPool = this;
        foreach (var info in prewarmSettings)
        {
            Prewarm(info.prefabId, info.initialCount);
        }
    }

    // 오브젝트 미리 생성
    public void Prewarm(string prefabId, int count)
    {
        if (!poolDict.ContainsKey(prefabId))
            poolDict[prefabId] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"[PhotonPool] '{prefabId}' 프리팹이 Resources 폴더에 없습니다.");
                return;
            }

            GameObject obj = Instantiate(prefab);
            obj.name = prefabId;
            obj.SetActive(false);
            poolDict[prefabId].Enqueue(obj);
        }

        Debug.Log($"[PhotonPool] '{prefabId}' {count}개 미리 생성 완료.");
    }

    // 반드시 SetActive(false) 상태로 반환해야 함 (Photon 공식 권장)
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(prefabId))
            poolDict[prefabId] = new Queue<GameObject>();
        if (!activeCountDict.ContainsKey(prefabId))
            activeCountDict[prefabId] = 0;

        int maxCount = GetMaxCount(prefabId);
        int totalCount = poolDict[prefabId].Count + activeCountDict[prefabId];
        if (totalCount >= maxCount)
        {
            Debug.LogWarning($"[PhotonPool] '{prefabId}' 최대 수량({maxCount}) 초과. 생성 차단.");
            return null;
        }

        GameObject obj;
        if (poolDict[prefabId].Count > 0)
        {
            obj = poolDict[prefabId].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"[PhotonPool] '{prefabId}' 프리팹이 Resources 폴더에 없습니다.");
                return null;
            }
            obj = Instantiate(prefab, position, rotation);
            obj.name = prefabId;
        }

        // 반드시 비활성화 상태로 반환 (Photon이 내부적으로 SetActive(true)로 활성화)
        obj.SetActive(false);
        activeCountDict[prefabId]++;
        // OnEnable에서 상태 초기화 필수!
        return obj;
    }

    // 오브젝트 반환 (비활성화 후 풀에 저장)
    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();

        if (!poolDict.ContainsKey(prefabId))
            poolDict[prefabId] = new Queue<GameObject>();
        if (!activeCountDict.ContainsKey(prefabId))
            activeCountDict[prefabId] = 0;

        gameObject.SetActive(false);
        poolDict[prefabId].Enqueue(gameObject);
        activeCountDict[prefabId] = Mathf.Max(0, activeCountDict[prefabId] - 1);
    }

    private int GetMaxCount(string prefabId)
    {
        var setting = prewarmSettings.Find(x => x.prefabId == prefabId);
        return setting != null ? setting.maxCount : 100; // 기본 최대값 100
    }
}
