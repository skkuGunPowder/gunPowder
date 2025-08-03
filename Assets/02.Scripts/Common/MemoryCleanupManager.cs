using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class MemoryCleanupManager : MonoBehaviourPun
{
    private static MemoryCleanupManager _instance;
    public static MemoryCleanupManager Instance => _instance;

    [Header("Memory Cleanup Settings")]
    [SerializeField] private float _cleanupInterval = 30f; // 30초마다 정리
    [SerializeField] private bool _autoCleanup = true;
    
    private float _lastCleanupTime = 0f;
    private Coroutine _cleanupCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_autoCleanup)
        {
            StartAutoCleanup();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
        
        if (_cleanupCoroutine != null)
        {
            StopCoroutine(_cleanupCoroutine);
        }
    }

    /// <summary>
    /// 자동 메모리 정리 시작
    /// </summary>
    public void StartAutoCleanup()
    {
        if (_cleanupCoroutine != null)
        {
            StopCoroutine(_cleanupCoroutine);
        }
        _cleanupCoroutine = StartCoroutine(AutoCleanupRoutine());
    }

    /// <summary>
    /// 자동 메모리 정리 중지
    /// </summary>
    public void StopAutoCleanup()
    {
        if (_cleanupCoroutine != null)
        {
            StopCoroutine(_cleanupCoroutine);
            _cleanupCoroutine = null;
        }
    }

    /// <summary>
    /// 즉시 메모리 정리 실행
    /// </summary>
    public void ForceCleanup()
    {
        StartCoroutine(PerformCleanup());
    }

    private IEnumerator AutoCleanupRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_cleanupInterval);
            
            if (Time.time - _lastCleanupTime >= _cleanupInterval)
            {
                yield return StartCoroutine(PerformCleanup());
                _lastCleanupTime = Time.time;
            }
        }
    }

    private IEnumerator PerformCleanup()
    {
        Debug.Log("[MemoryCleanupManager] 메모리 정리 시작...");
        
        // 1. 가비지 컬렉션 실행
        System.GC.Collect();
        yield return null;
        
        // 2. 사용하지 않는 에셋 해제
        yield return Resources.UnloadUnusedAssets();
        
        // 3. 추가 가비지 컬렉션
        System.GC.Collect();
        
        // 4. 메모리 사용량 로그
        long totalMemory = System.GC.GetTotalMemory(false);
        Debug.Log($"[MemoryCleanupManager] 메모리 정리 완료. 현재 메모리 사용량: {totalMemory / 1024 / 1024}MB");
    }

    /// <summary>
    /// 특정 GameObject와 그 자식들을 안전하게 제거
    /// </summary>
    public static void SafeDestroy(GameObject obj)
    {
        if (obj != null)
        {
            // PhotonView가 있는 경우 PhotonNetwork.Destroy 사용
            PhotonView photonView = obj.GetComponent<PhotonView>();
            if (photonView != null && PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// 즉시 제거 (Editor에서만)
    /// </summary>
    public static void SafeDestroyImmediate(GameObject obj)
    {
        if (obj != null)
        {
            DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// 메모리 사용량 정보 출력
    /// </summary>
    public static void LogMemoryUsage()
    {
        long totalMemory = System.GC.GetTotalMemory(false);
        long managedMemory = System.GC.GetTotalMemory(true);
        
        Debug.Log($"[MemoryCleanupManager] 메모리 사용량 - Total: {totalMemory / 1024 / 1024}MB, Managed: {managedMemory / 1024 / 1024}MB");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 앱이 일시정지될 때 메모리 정리
            ForceCleanup();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // 앱이 포커스를 잃을 때 메모리 정리
            ForceCleanup();
        }
    }
} 