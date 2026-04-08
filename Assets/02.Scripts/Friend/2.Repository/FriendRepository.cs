using System;
using System.Collections.Generic;
using BackEnd;
using LitJson;

// Backend.Friend API를 래핑하여 도메인 객체(Friend)로 변환하는 Repository.
// Raw 데이터의 저장/불러오기를 담당한다. 비동기 콜백 패턴 사용.
public class FriendRepository
{
    // ===== 쓰기 (Command) =====

    // 닉네임 → inDate 조회 → gamerIndate 기반 친구 요청
    public void RequestFriendByNickname(string nickname, Action<bool, string> callback)
    {
        Backend.Social.GetUserInfoByNickNameV2(nickname, bro =>
        {
            if (bro.IsSuccess())
            {
                JsonData inDateNode = bro.GetReturnValuetoJSON()["row"]["inDate"];
                // GetUserInfoByNickNameV2 응답은 plain string 또는 {"S":"..."} 형태일 수 있음
                string inDate = inDateNode.IsObject && ((IDictionary<string, JsonData>)inDateNode).ContainsKey("S")
                    ? inDateNode["S"].ToString()
                    : inDateNode.ToString();
                RequestFriend(inDate, callback);
            }
            else
            {
                callback?.Invoke(false, $"유저 정보 조회 실패: {bro.GetMessage()}");
            }
        });
    }

    // gamerIndate 기반 친구 요청
    public void RequestFriend(string gamerIndate, Action<bool, string> callback)
    {
        Backend.Friend.RequestFriend(gamerIndate, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, "친구 요청을 보냈습니다.");
            else
                callback?.Invoke(false, ParseError(bro));
        });
    }

    public void AcceptFriend(string gamerIndate, Action<bool, string> callback)
    {
        Backend.Friend.AcceptFriend(gamerIndate, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, "친구 요청을 수락했습니다.");
            else
                callback?.Invoke(false, ParseError(bro));
        });
    }

    public void RejectFriend(string gamerIndate, Action<bool, string> callback)
    {
        Backend.Friend.RejectFriend(gamerIndate, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, "친구 요청을 거절했습니다.");
            else
                callback?.Invoke(false, ParseError(bro));
        });
    }

    public void RevokeSentRequest(string gamerIndate, Action<bool, string> callback)
    {
        Backend.Friend.RevokeSentRequest(gamerIndate, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, "친구 요청을 취소했습니다.");
            else
                callback?.Invoke(false, ParseError(bro));
        });
    }

    public void BreakFriend(string gamerIndate, Action<bool, string> callback)
    {
        Backend.Friend.BreakFriend(gamerIndate, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, "친구를 삭제했습니다.");
            else
                callback?.Invoke(false, ParseError(bro));
        });
    }

    // ===== 읽기 (Query) → Friend 도메인 객체로 변환하여 반환 =====

    public void GetFriendList(int limit, int offset, Action<bool, List<Friend>> callback)
    {
        Backend.Friend.GetFriendList(limit, offset, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, ParseRows(bro));
            else
                callback?.Invoke(false, null);
        });
    }

    public void GetReceivedRequestList(int limit, int offset, Action<bool, List<Friend>> callback)
    {
        Backend.Friend.GetReceivedRequestList(limit, offset, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, ParseRows(bro));
            else
                callback?.Invoke(false, null);
        });
    }

    public void GetSentRequestList(int limit, int offset, Action<bool, List<Friend>> callback)
    {
        Backend.Friend.GetSentRequestList(limit, offset, bro =>
        {
            if (bro.IsSuccess())
                callback?.Invoke(true, ParseRows(bro));
            else
                callback?.Invoke(false, null);
        });
    }

    // ===== Raw 데이터 파싱 =====

    private List<Friend> ParseRows(BackendReturnObject bro)
    {
        List<Friend> list = new List<Friend>();
        JsonData rows = bro.GetReturnValuetoJSON()["rows"];

        if (rows == null || rows.Count == 0)
            return list;

        for (int i = 0; i < rows.Count; i++)
        {
            JsonData row = rows[i];
            string nickname = row.ContainsKey("nickname") ? row["nickname"]["S"].ToString() : "";
            string inDate   = row.ContainsKey("inDate")   ? row["inDate"]["S"].ToString()   : "";
            string createdAt = row.ContainsKey("createdAt") ? row["createdAt"]["S"].ToString() : "";
            string lastLogin = row.ContainsKey("lastLogin") ? row["lastLogin"]["S"].ToString() : "";

            list.Add(new Friend(nickname, inDate, createdAt, lastLogin));
        }

        return list;
    }

    private string ParseError(BackendReturnObject bro)
    {
        int statusCode = int.Parse(bro.GetStatusCode());

        if (statusCode == 412)
        {
            string errorCode = bro.GetErrorCode();
            if (errorCode.Contains("maxRequestedGamerFriend"))
                return "상대방의 친구가 최대 인원수에 도달하였습니다.";
            if (errorCode.Contains("maxGamerFriend"))
                return "친구가 최대 인원수에 도달하였습니다.";
        }

        return $"오류 발생: {bro.GetMessage()}";
    }
}
