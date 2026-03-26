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

                instance = FindAnyObjectByType<T>();
                if (instance == null)
                {
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
        if (instance == null || instance == this)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}