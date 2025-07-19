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
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        Init();
    }

    private async void Init()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            App = FirebaseApp.DefaultInstance;
            DB = FirebaseFirestore.DefaultInstance;

            Debug.Log("Firebase 연결 성공");
        }
        else
        {
            Debug.LogError($"Firebase 연결 실패: {dependencyStatus}");
        }
    }
}
