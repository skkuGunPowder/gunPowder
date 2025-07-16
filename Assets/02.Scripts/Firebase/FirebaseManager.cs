using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseApp App { get; private set; }
    public FirebaseFirestore DB { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = null;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        Init();
    }

    private void Init()
    {
        var dependencyStatus = FirebaseApp.CheckAndFixDependenciesAsync().Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            App = FirebaseApp.DefaultInstance;
            DB = FirebaseFirestore.DefaultInstance;
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }
}
