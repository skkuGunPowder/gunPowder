using Photon.Pun;
using UnityEngine;
public class PhotonSingleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("photon singleton instance is null");

                instance = FindAnyObjectByType<T>();
                if (instance == null)
                {
                    Debug.Log("photon singleton instance is null");
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    protected virtual void Awake()
    {
        Debug.Log("photon singleton Awake");
        if (instance == null)
        {
            instance = this as T;
            Debug.Log("instance is set");
            
        }
        else
        { 
            Debug.Log("photon gameobject is already exist");
            Destroy(gameObject);
        }
    }
}