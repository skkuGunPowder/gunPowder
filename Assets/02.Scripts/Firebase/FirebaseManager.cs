using UnityEngine;
using Firebase;
using Firebase.Firestore;
using System;

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

            try
            {
                ItemDatabase.Instance.Init();
                Debug.Log("아이템 데이터 로드 성공");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        else
        {
            Debug.LogError($"Firebase 연결 실패: {dependencyStatus}");
        }
    }
}
