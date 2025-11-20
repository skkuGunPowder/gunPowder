using BackEnd; // Base SDK
using UnityEngine;
using System.Collections.Generic;

public class FriendManager : DontDestroySingleton<FriendManager>
{
    public List<BackendReturnObject> FriendList = new List<BackendReturnObject>();

    // 친구 목록 갱신 (로비 진입 시 호출)
    public void RefreshFriendList()
    {
        Backend.Friend.GetFriendList(100, callback =>
        {
            if (callback.IsSuccess())
            {
                var rows = callback.Rows();
                FriendList.Clear();
                for (int i = 0; i < rows.Count; i++)
                {
                    // 친구 정보 파싱
                    // 친구의 닉네임, indate 등을 저장
                    // UI 갱신 이벤트 호출
                }
                Debug.Log($"친구 목록 갱신 완료: {FriendList.Count}명");
            }
        });
    }

    // 친구 요청 보내기
    public void RequestFriend(string nickname)
    {
        Backend.Friend.RequestFriend(nickname, callback =>
        {
            if (callback.IsSuccess()) Debug.Log("친구 요청 전송 성공");
            else Debug.LogError($"친구 요청 실패: {callback.ToString()}");
        });
    }
    
    // 친구 수락 등 추가 구현 필요...
}