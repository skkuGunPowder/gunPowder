using System;
using System.Collections.Generic;
using BackEnd;
using Firebase.Firestore;
using UnityEngine;

// 친구 시스템 매니저 (DontDestroySingleton).
// Repository를 통해 CRUD를 수행하고, 캐시 관리 및 이벤트 발행을 담당한다.
// Backend.Notification을 통해 실시간 접속 상태 및 친구 요청 이벤트를 처리한다.
public class FriendManager : DontDestroySingleton<FriendManager>
{
    private FriendRepository _repo;
    private List<Friend> _cachedFriendList = new List<Friend>();

    public event Action OnFriendListChanged;
    public event Action<string> OnFriendError;
    public event Action OnFriendRequestReceived;

    public IReadOnlyList<Friend> CachedFriendList => _cachedFriendList;

    protected override void Awake()
    {
        base.Awake();
        _repo = new FriendRepository();
    }

    // ===== 뒤끝 실시간 알림 연결 =====

    // 로그인 완료 후 1회 호출. 서버가 접속 상태를 자동 추적하므로 heartbeat 불필요.
    public void ConnectNotification()
    {
        Backend.Notification.OnAuthorize = (bool result, string reason) =>
        {
            if (result)
                Debug.Log("[FriendManager] 실시간 알림 서버 연결 성공");
            else
                Debug.LogWarning($"[FriendManager] 실시간 알림 서버 연결 실패: {reason}");
        };

        // 친구 접속 이벤트
        Backend.Notification.OnFriendConnected = (string friendInDate, string friendNickname) =>
        {
            Debug.Log($"[FriendManager] 친구 접속: {friendNickname}");
            UpdateFriendStatus(friendInDate, EFriendStatus.Online);
        };

        // 친구 종료 이벤트
        Backend.Notification.OnFriendDisconnected = (string friendInDate, string friendNickname) =>
        {
            Debug.Log($"[FriendManager] 친구 종료: {friendNickname}");
            UpdateFriendStatus(friendInDate, EFriendStatus.Offline);
        };

        // 접속 여부 조회 응답 (UserIsConnectByIndate 호출에 대한 콜백)
        Backend.Notification.OnIsConnectUser = (bool isConnect, string friendInDate, string friendNickname) =>
        {
            EFriendStatus status = isConnect ? EFriendStatus.Online : EFriendStatus.Offline;
            Debug.Log($"[FriendManager] 접속 조회 응답: {friendNickname} → {status}");
            UpdateFriendStatus(friendInDate, status);
        };

        // 친구 요청 수신
        Backend.Notification.OnReceivedFriendRequest = () =>
        {
            Debug.Log($"[FriendManager] 친구 요청 수신");
            OnFriendRequestReceived?.Invoke();
        };

        // 친구 요청 수락됨 (내가 보낸 요청이 수락됨)
        Backend.Notification.OnAcceptedFriendRequest = () =>
        {
            Debug.Log($"[FriendManager] 친구 요청 수락됨");
            RefreshFriendList();
        };

        // 친구 요청 거절됨
        Backend.Notification.OnRejectedFriendRequest = () =>
        {
            Debug.Log($"[FriendManager] 친구 요청 거절됨");
        };

        Backend.Notification.Connect();
    }

    // 캐시된 친구의 inDate로 접속 상태 일괄 조회
    public void QueryAllFriendConnectionStatus()
    {
        foreach (var friend in _cachedFriendList)
        {
            if (!string.IsNullOrEmpty(friend.InDate))
                Backend.Notification.UserIsConnectByIndate(friend.InDate);
        }
    }

    // inDate로 캐시된 친구를 찾아 상태 갱신
    private void UpdateFriendStatus(string friendInDate, EFriendStatus status)
    {
        foreach (var friend in _cachedFriendList)
        {
            if (friend.InDate == friendInDate)
            {
                friend.UpdateStatus(status);
                OnFriendListChanged?.Invoke();
                return;
            }
        }
    }

    // ===== Create (친구 요청) =====

    public void RequestFriend(string gamerIndate, Action<bool, string> callback = null)
    {
        _repo.RequestFriend(gamerIndate, (success, msg) =>
        {
            if (!success) OnFriendError?.Invoke(msg);
            callback?.Invoke(success, msg);
        });
    }

    public void AcceptFriend(string gamerIndate, Action<bool, string> callback = null)
    {
        _repo.AcceptFriend(gamerIndate, (success, msg) =>
        {
            if (success) RefreshFriendList();
            else OnFriendError?.Invoke(msg);
            callback?.Invoke(success, msg);
        });
    }

    public void RejectFriend(string gamerIndate, Action<bool, string> callback = null)
    {
        _repo.RejectFriend(gamerIndate, callback);
    }

    public void RevokeSentRequest(string gamerIndate, Action<bool, string> callback = null)
    {
        _repo.RevokeSentRequest(gamerIndate, callback);
    }

    // ===== Delete (친구 삭제) =====

    public void BreakFriend(string gamerIndate, Action<bool, string> callback = null)
    {
        _repo.BreakFriend(gamerIndate, (success, msg) =>
        {
            if (success) RefreshFriendList();
            else OnFriendError?.Invoke(msg);
            callback?.Invoke(success, msg);
        });
    }

    // ===== Read (목록 조회) =====

    public void RefreshFriendList(int limit = Friend.DEFAULT_QUERY_LIMIT, int offset = 0)
    {
        _repo.GetFriendList(limit, offset, (success, list) =>
        {
            if (success)
            {
                _cachedFriendList = list;
                OnFriendListChanged?.Invoke();
            }
        });
    }

    public void GetReceivedRequests(Action<bool, List<Friend>> callback,
        int limit = Friend.DEFAULT_QUERY_LIMIT, int offset = 0)
    {
        _repo.GetReceivedRequestList(limit, offset, callback);
    }

    public void GetSentRequests(Action<bool, List<Friend>> callback,
        int limit = Friend.DEFAULT_QUERY_LIMIT, int offset = 0)
    {
        _repo.GetSentRequestList(limit, offset, callback);
    }

    // ===== 프로필 아웃핏 조회 (Firebase 유지) =====

    // 친구 목록의 프로필 아웃핏을 Firestore에서 읽어와 갱신
    public async void RefreshFriendOutfits()
    {
        var db = FirebaseManager.Instance.DB;

        foreach (var friend in _cachedFriendList)
        {
            try
            {
                // nickname → uid 조회
                List<string> uids = await AccountManager.Instance.GetUidsWithNickname(friend.Nickname);
                if (uids == null || uids.Count == 0) continue;
                string uid = uids[0];

                // Inventory/{uid} 에서 Equipments 읽기
                var invSnap = await db.Collection("Inventory").Document(uid).GetSnapshotAsync();
                if (invSnap.Exists && invSnap.ContainsField("Equipments"))
                {
                    var equipments = invSnap.GetValue<Dictionary<string, object>>("Equipments");
                    List<string> idList = new List<string>();
                    foreach (var kvp in equipments)
                    {
                        if (kvp.Value is Dictionary<string, object> item &&
                            item.ContainsKey("ID") && item.ContainsKey("IsEquipped"))
                        {
                            bool isEquipped = Convert.ToBoolean(item["IsEquipped"]);
                            if (isEquipped)
                                idList.Add(item["ID"].ToString());
                        }
                    }
                    friend.UpdateOutfitIds(idList);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FriendManager] {friend.Nickname} 아웃핏 조회 실패: {e.Message}");
            }
        }

        OnFriendListChanged?.Invoke();
    }

    // ===== 닉네임 기반 친구 요청 =====

    // 닉네임으로 친구 요청 (뒤끝 SDK가 닉네임 기반 RequestFriend 지원)
    public void RequestFriendByNickname(string nickname, Action<bool, string> callback = null)
    {
        _repo.RequestFriendByNickname(nickname, (success, msg) =>
        {
            if (!success) OnFriendError?.Invoke(msg);
            callback?.Invoke(success, msg);
        });
    }

    // ===== 유틸리티 =====

    public int GetFriendCount() => _cachedFriendList.Count;

    public bool IsFriendListFull() => Friend.IsFriendListFull(_cachedFriendList.Count);

    // 캐시된 친구 목록을 FriendDTO 리스트로 변환하여 반환 (UI 전달용)
    public List<FriendDTO> GetFriendDTOList()
    {
        List<FriendDTO> dtoList = new List<FriendDTO>(_cachedFriendList.Count);
        foreach (var friend in _cachedFriendList)
        {
            dtoList.Add(friend.ToDTO());
        }
        return dtoList;
    }
}
