using System;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using LitJson;

public class FriendManagerLegacy : Singleton<FriendManagerLegacy>
{
    // 친구 요청 보내기 (닉네임 기반)
    public void SendFriendRequest(string recipientNickname, Action<bool, string> callback = null)
    {
        Backend.Friend.RequestFriend(recipientNickname, bro =>
        {
            if (bro.IsSuccess())
            {
                Debug.Log($"친구 요청 전송 성공: {recipientNickname}");
                callback?.Invoke(true, "친구 요청을 보냈습니다.");
            }
            else
            {
                Debug.LogError($"친구 요청 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, bro.GetMessage());
            }
        });
    }

    // 친구 요청 수락 (inDate 기반)
    public void AcceptFriendRequest(string requesterInDate, Action<bool, string> callback = null)
    {
        Backend.Friend.AcceptFriend(requesterInDate, bro =>
        {
            if (bro.IsSuccess())
            {
                Debug.Log($"친구 요청 수락 성공: {requesterInDate}");
                callback?.Invoke(true, "친구 요청을 수락했습니다.");
            }
            else
            {
                Debug.LogError($"친구 요청 수락 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, bro.GetMessage());
            }
        });
    }

    // 친구 요청 거절 (inDate 기반)
    public void DeclineFriendRequest(string requesterInDate, Action<bool, string> callback = null)
    {
        Backend.Friend.RejectFriend(requesterInDate, bro =>
        {
            if (bro.IsSuccess())
            {
                Debug.Log($"친구 요청 거절 성공: {requesterInDate}");
                callback?.Invoke(true, "친구 요청을 거절했습니다.");
            }
            else
            {
                Debug.LogError($"친구 요청 거절 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, bro.GetMessage());
            }
        });
    }

    // 친구 삭제 (inDate 기반)
    public void RemoveFriend(string friendInDate, Action<bool, string> callback = null)
    {
        Backend.Friend.BreakFriend(friendInDate, bro =>
        {
            if (bro.IsSuccess())
            {
                Debug.Log($"친구 삭제 성공: {friendInDate}");
                callback?.Invoke(true, "친구를 삭제했습니다.");
            }
            else
            {
                Debug.LogError($"친구 삭제 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, bro.GetMessage());
            }
        });
    }

    // 친구 목록 가져오기
    public void GetFriendList(int limit, Action<bool, List<FriendInfo>> callback = null)
    {
        Backend.Friend.GetFriendList(limit, bro =>
        {
            if (bro.IsSuccess())
            {
                List<FriendInfo> friendList = new List<FriendInfo>();

                JsonData rows = bro.GetReturnValuetoJSON()["rows"];

                if (rows != null && rows.Count > 0)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JsonData row = rows[i];
                        string nickname = row.ContainsKey("nickname") ? row["nickname"]["S"].ToString() : "";
                        string inDate = row.ContainsKey("inDate") ? row["inDate"]["S"].ToString() : "";

                        friendList.Add(new FriendInfo(nickname, inDate));
                    }
                }

                Debug.Log($"친구 목록 조회 성공: {friendList.Count}명");
                callback?.Invoke(true, friendList);
            }
            else
            {
                Debug.LogError($"친구 목록 조회 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, null);
            }
        });
    }

    // 받은 친구 요청 목록 가져오기
    public void GetReceivedFriendRequests(Action<bool, List<FriendInfo>> callback = null)
    {
        Backend.Friend.GetReceivedRequestList(bro =>
        {
            if (bro.IsSuccess())
            {
                List<FriendInfo> requestList = new List<FriendInfo>();

                JsonData rows = bro.GetReturnValuetoJSON()["rows"];

                if (rows != null && rows.Count > 0)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JsonData row = rows[i];
                        string nickname = row.ContainsKey("nickname") ? row["nickname"]["S"].ToString() : "";
                        string inDate = row.ContainsKey("inDate") ? row["inDate"]["S"].ToString() : "";

                        requestList.Add(new FriendInfo(nickname, inDate));
                    }
                }

                Debug.Log($"친구 요청 목록 조회 성공: {requestList.Count}개");
                callback?.Invoke(true, requestList);
            }
            else
            {
                Debug.LogError($"친구 요청 목록 조회 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, null);
            }
        });
    }

    // 보낸 친구 요청 목록 가져오기
    public void GetSentFriendRequests(Action<bool, List<FriendInfo>> callback = null)
    {
        Backend.Friend.GetSentRequestList(bro =>
        {
            if (bro.IsSuccess())
            {
                List<FriendInfo> requestList = new List<FriendInfo>();

                JsonData rows = bro.GetReturnValuetoJSON()["rows"];

                if (rows != null && rows.Count > 0)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JsonData row = rows[i];
                        string nickname = row.ContainsKey("nickname") ? row["nickname"]["S"].ToString() : "";
                        string inDate = row.ContainsKey("inDate") ? row["inDate"]["S"].ToString() : "";

                        requestList.Add(new FriendInfo(nickname, inDate));
                    }
                }

                Debug.Log($"보낸 친구 요청 목록 조회 성공: {requestList.Count}개");
                callback?.Invoke(true, requestList);
            }
            else
            {
                Debug.LogError($"보낸 친구 요청 목록 조회 실패: {bro.GetStatusCode()} - {bro.GetMessage()}");
                callback?.Invoke(false, null);
            }
        });
    }
}
