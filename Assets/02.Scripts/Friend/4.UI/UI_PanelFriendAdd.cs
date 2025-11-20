using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UI_PanelFriendAdd : UI_Popup
{
    public Transform contentParent;
    public GameObject requestItemPrefab;
    public TMP_InputField NicknameInputField;

    // 친구 검색 버튼
    public async void OnClickFriendSearchButton()
    {
        string inputNickname = NicknameInputField.text;
        if (string.IsNullOrEmpty(inputNickname))
            return;

        // 기존 항목 삭제
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        
        // 1. 닉네임으로 UID 리스트 검색
        List<string> uids = await AccountManager.Instance.GetUidsWithNickname(inputNickname);
    
        // 2. 각 UID로 표시 이름(닉네임#discriminator) 가져오기
        foreach (string uid in uids)
        {
            string displayName = await AccountManager.Instance.GetUserDisplayNameWithUid(uid);
            // UI에 표시: "홍길동#0001", "홍길동#0002", ...
            GameObject item = Instantiate(requestItemPrefab, contentParent);
            var panel = item.GetComponent<UI_PanelFriendUser>();
            panel.Refresh(displayName, uid); // "홍길동#0001" 형태로 표시
        }
        
        // // 닉네임으로 UID 리스트 가져오기
        // List<string> uids = await AccountManager.Instance.GetUidsWithNickname(inputNickname);
        // if (uids.Count == 0)
        //     return;
        //
        // // 각 UID에 대해 항목 생성
        // foreach (string uid in uids)
        // {
        //     GameObject item = Instantiate(requestItemPrefab, contentParent);
        //     var panel = item.GetComponent<UI_PanelFriendUser>();
        //     panel.Refresh(inputNickname, uid);
        // }
    }
}