using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_PanelFriendSlot : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI FriendName;
    public TextMeshProUGUI FriendConnectState;

    [Header("Buttons")]
    public Button ChatButton;           // 1:1 채팅 시작 버튼
    public Button InviteButton;         // 파티 초대 버튼

    private string _friendUid;
    private string _friendNickname;

    // 채팅 시작 이벤트
    public event Action<string, string> OnChatButtonClicked; // (friendUid, friendNickname)

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (ChatButton != null)
            ChatButton.onClick.AddListener(OnClickChat);

        if (InviteButton != null)
            InviteButton.onClick.AddListener(OnClickFriendInvite);
    }

    public void Refresh(string nickname)
    {
        _friendNickname = nickname;
        FriendName.text = nickname;
        //FriendConnectState.text = "online"; // TODO : Account랑 firestore에 추후 필드 추가
    }

    /// <summary>
    /// 친구 UID 설정 (Refresh 후 호출)
    /// </summary>
    public void SetFriendUid(string uid)
    {
        _friendUid = uid;
    }

    /// <summary>
    /// 1:1 채팅 버튼 클릭
    /// </summary>
    private void OnClickChat()
    {
        if (string.IsNullOrEmpty(_friendUid) || string.IsNullOrEmpty(_friendNickname))
        {
            Debug.LogWarning("[UI_PanelFriendSlot] 친구 정보가 없습니다.");
            return;
        }

        Debug.Log($"[UI_PanelFriendSlot] 채팅 시작: {_friendNickname}");
        OnChatButtonClicked?.Invoke(_friendUid, _friendNickname);
    }

    /// <summary>
    /// 파티 초대 버튼 클릭
    /// </summary>
    public void OnClickFriendInvite()
    {
        string friendNickname = FriendName.text;
        string myNickname = AccountManager.Instance.CurrentAccount.Nickname;

        Debug.Log("[UI_PanelFriendSlot] 파티 초대 보내기");

        // 파티가 없으면 생성
        if (!PartyManager.Instance.IsInParty())
        {
            string partyId = AccountManager.Instance.CurrentAccount.Account_ID;
            PartyManager.Instance.CreateParty(partyId);
        }

        // UIChatManager를 통해 파티 초대 전송
        PartyManager.Instance.SendPartyInvite(friendNickname);
    }
}