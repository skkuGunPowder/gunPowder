using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

// Firestore users/{uid} 컬렉션에 자신의 온라인 상태를 쓰는 역할.
// 상태: "Online", "InGame", "Offline"
// Heartbeat: 매 3분마다 lastStatusUpdate 갱신 (비정상 종료 대비)
public class FriendStatusUpdater : DontDestroySingleton<FriendStatusUpdater>
{
    private const float HEARTBEAT_INTERVAL = 180f; // 3분
    private Coroutine _heartbeatCoroutine;

    // 로그인 성공 시 호출
    public void SetOnline()
    {
        SetStatus("Online");
        StartHeartbeat();
    }

    // 게임 씬 진입 시 호출
    public void SetInGame()
    {
        SetStatus("InGame");
    }

    // 로비 복귀 시 호출
    public void SetOnlineFromGame()
    {
        SetStatus("Online");
    }

    // 로그아웃 시 호출
    public void SetOffline()
    {
        StopHeartbeat();
        SetStatus("Offline");
    }

    private async void SetStatus(string status)
    {
        try
        {
            var user = FirebaseManager.Instance.Auth.CurrentUser;
            if (user == null) return;

            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            await userDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "status", status },
                { "lastStatusUpdate", FieldValue.ServerTimestamp }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FriendStatusUpdater] 상태 업데이트 실패: {e.Message}");
        }
    }

    private void StartHeartbeat()
    {
        StopHeartbeat();
        _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCoroutine != null)
        {
            StopCoroutine(_heartbeatCoroutine);
            _heartbeatCoroutine = null;
        }
    }

    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(HEARTBEAT_INTERVAL);
            UpdateHeartbeat();
        }
    }

    private async void UpdateHeartbeat()
    {
        try
        {
            var user = FirebaseManager.Instance.Auth.CurrentUser;
            if (user == null) return;

            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            await userDoc.UpdateAsync(new Dictionary<string, object>
            {
                { "lastStatusUpdate", FieldValue.ServerTimestamp }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[FriendStatusUpdater] Heartbeat 실패: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        SetStatus("Offline");
    }
}
