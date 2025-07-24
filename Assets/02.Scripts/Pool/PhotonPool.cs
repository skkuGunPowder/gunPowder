using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class PhotonPool : MonoBehaviour, IPunPrefabPool
{
    protected Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Start()
    {
        PhotonNetwork.PrefabPool = this; // PUN2에 커스텀 풀 등록
    }

    // 오브젝트 생성
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;
        if (poolDict.TryGetValue(prefabId, out var queue) && queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            // Resources에서 프리팹 로드 후 생성
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            if (prefab == null)
            {
                Debug.LogError($"[Pool] Resources에 '{prefabId}' 프리팹이 없습니다.");
                return null;
            }
            obj = Instantiate(prefab, position, rotation);
            obj.name = prefabId; // 풀링 관리 편의상 이름 지정
        }
        obj.SetActive(false); // 반드시 비활성화 상태로 반환!
        return obj;
    }

    // 오브젝트 반환
    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();
        if (!poolDict.ContainsKey(prefabId))
            poolDict[prefabId] = new Queue<GameObject>();

        gameObject.SetActive(false);
        poolDict[prefabId].Enqueue(gameObject);
    }
}
