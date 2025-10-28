using TMPro;
using UnityEngine;

public class UI_PanelFriendSlot : MonoBehaviour
{
    public TextMeshProUGUI FriendName;
    public TextMeshProUGUI FriendConnectState;

    public void Refresh(string nickname)
    {
        FriendName.text = nickname;
        //FriendConnectState.text = "online"; // TODO : Account랑 firestore에 추후 필드 추가
    }
    public void OnClickFriendInvite()
    {
        string friendNickname = FriendName.text;
        string myNickname = AccountManager.Instance.CurrentAccount.Nickname;
    
        Debug.Log("파티 초대 보내기 버튼 누름");
        // 친구 초대 전송
        PartyManager.Instance.SendFriendInvite(friendNickname, myNickname);
    }
}