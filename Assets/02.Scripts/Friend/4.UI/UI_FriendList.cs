using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_FriendList : UI_Popup
{
    public Transform contentParent;
    public GameObject friendItemPrefab;

    private void OnEnable()
    {
        LoadFriendList();
    }

    private void LoadFriendList()
    {
        // 기존 아이템 정리
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 친구 목록 조회 (콜백 패턴)
        FriendManagerLegacy.Instance.GetFriendList(100, (success, friendList) =>
        {
            if (success && friendList != null)
            {
                foreach (var friend in friendList)
                {
                    GameObject item = Instantiate(friendItemPrefab, contentParent);
                    var friendSlot = item.GetComponent<UI_PanelFriendSlot>();

                    // FriendInfo에서 닉네임과 inDate 사용
                    friendSlot.Refresh(friend.nickname);
                    friendSlot.SetFriendInDate(friend.inDate); // inDate 설정
                }
            }
            else
            {
                Debug.LogError("친구 목록 조회 실패");
            }
        });
    }
}