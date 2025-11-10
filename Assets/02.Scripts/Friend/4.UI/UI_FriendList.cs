using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UI_FriendList : UI_Popup
{
    public Transform contentParent;
    public GameObject friendItemPrefab;
    private void OnEnable()
    {
        LoadFriendListAsync();
    }

    private async void LoadFriendListAsync()
    {
        await LoadFriendList();
    }
    
    // public async void Show()
    // {
    //
    //     await LoadFriendList();
    // }

    private async Task LoadFriendList()
    {
        string myUid = AccountManager.Instance.CurrentAccount.Account_ID;

        List<string> friendUids = await FriendManager.Instance.GetFriendUids(myUid);

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (string uid in friendUids)
        {
            GameObject item = Instantiate(friendItemPrefab, contentParent);
            var friendSlot = item.GetComponent<UI_PanelFriendSlot>();

            string nickname = await AccountManager.Instance.GetUserNicknameWithUid(uid);
            friendSlot.Refresh(nickname);
            friendSlot.SetFriendUid(uid); // UID 설정 추가
        }
    }
}