using System.Collections.Generic;
using UnityEngine;

public class UI_PanelFriendAccept : UI_Popup
{
    [SerializeField] private Transform content; // Content 오브젝트
    [SerializeField] private GameObject requestFriendPrefab;

    private void OnEnable()
    {
        RefreshRequestList();
    }

    private void RefreshRequestList()
    {
        // 기존 프리팹 정리
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 받은 친구 요청 목록 조회 (콜백 패턴)
        FriendManagerLegacy.Instance.GetReceivedFriendRequests((success, requestList) =>
        {
            if (success && requestList != null)
            {
                foreach (var request in requestList)
                {
                    GameObject go = Instantiate(requestFriendPrefab, content);
                    var ui = go.GetComponent<UI_RequestFriendslot>();

                    // FriendInfo에서 닉네임과 inDate 사용
                    ui.Refresh(request.nickname, request.inDate);
                }
            }
            else
            {
                Debug.LogError("친구 요청 목록 조회 실패");
            }
        });
    }
}