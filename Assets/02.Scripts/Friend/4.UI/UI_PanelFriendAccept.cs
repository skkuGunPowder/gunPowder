using UnityEngine;

public class UI_PanelFriendAccept : UI_Popup
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject requestFriendPrefab;

    private void OnEnable()
    {
        RefreshRequestList();
    }

    private void RefreshRequestList()
    {
        // 기존 프리팹 정리
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 받은 친구 요청 목록 조회
        FriendManager.Instance.GetReceivedRequests((success, requestList) =>
        {
            if (success && requestList != null)
            {
                foreach (var request in requestList)
                {
                    GameObject go = Instantiate(requestFriendPrefab, content);
                    var ui = go.GetComponent<UI_RequestFriendslot>();
                    ui.Refresh(request.Nickname, request.InDate);
                }
            }
            else
            {
                Debug.LogError("친구 요청 목록 조회 실패");
            }
        });
    }
}
