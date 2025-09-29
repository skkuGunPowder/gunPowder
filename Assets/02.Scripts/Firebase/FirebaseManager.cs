using UnityEngine;
using Firebase;
using Firebase.Firestore;
using System;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseApp App { get; private set; }
    public FirebaseFirestore DB { get; private set; }
    public FirebaseAuth Auth { get; private set; }

    private bool _initialized;
    private bool _firestoreConfigured;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (!_initialized)
        {
            Init();
        }
    }

    private async void Init()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            #if UNITY_STANDALONE && !UNITY_EDITOR
                AppOptions options = new AppOptions()
                {
                    ProjectId = "gunpowder-c52bd",
                    AppId = "1:1043528855185:android:402dbd4fbbd7f250205930",
                    ApiKey = "AIzaSyDzJBfM-ymyBjYLdqvkw9B9-WlYJJf3HAE",
                    StorageBucket = "gunpowder-c52bd.firebasestorage.app",
                };

            if (App == null)
            {
                App = FirebaseApp.Create(options);
            }
            #else
            if (App == null)
            {
                App = FirebaseApp.DefaultInstance;
            }
            #endif

            if (DB == null)
            {
                DB = FirebaseFirestore.DefaultInstance;
            }

            if (!_firestoreConfigured && DB != null)
            {
                DB.Settings.PersistenceEnabled = false;
                _firestoreConfigured = true;
            }

            if (Auth == null)
            {
                Auth = FirebaseAuth.DefaultInstance;
            }

            _initialized = true;
        }
        else
        {
            Debug.LogError($"Firebase 연결 실패: {dependencyStatus}");
        }
    }
}
