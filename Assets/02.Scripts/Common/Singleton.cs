using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
     // Check if an instance exists in the scene without creating a new one
    public static bool HasInstance
    {
        get
        {
            if (instance != null) return true;
            instance = FindAnyObjectByType<T>();
            return instance != null;
        }
    }

    // Try to get an existing instance without auto-creation (returns null if not found)
    public static T TryGetInstance()
    {
        if (instance != null) return instance;
        instance = FindAnyObjectByType<T>();
        return instance;
    }
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
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}