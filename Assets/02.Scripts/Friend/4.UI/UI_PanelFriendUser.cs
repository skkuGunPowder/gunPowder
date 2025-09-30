using TMPro;
using UnityEngine;

public class UI_PanelFriendUser : MonoBehaviour
{
    public TextMeshProUGUI NicknameText;
    public TextMeshProUGUI UidText;
    private string _uid;

    
    public void Refresh(string senderNickname, string uid)
    {
        NicknameText.text = senderNickname;
        UidText.text = uid;
        _uid = uid;
    }
    // 친구 요청 보내기 버튼
    public async void OnRequestFriendSendButtonClicked()
    {
        await FriendManager.Instance.SendFriendRequest(AccountManager.Instance.CurrencAccount.Account_ID, _uid);
    }
}