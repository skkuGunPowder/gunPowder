using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PanelFriendSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI FriendName;
    public TextMeshProUGUI FriendConnectState;
    public ProfileSkin friendProfileSkin;

    [Header("Buttons")]
    public Button InviteButton;

    private string _friendUid;
    private string _friendNickname;
    private string _friendInDate;

    public event Action<string, string> OnChatButtonClicked;

    private void Awake()
    {
        if (InviteButton != null)
            InviteButton.onClick.AddListener(OnClickFriendInvite);
    }

    // 새 Friend 도메인 객체 기반 Refresh
    public void Refresh(Friend friend)
    {
        _friendNickname = friend.Nickname;
        _friendInDate = friend.InDate;
        FriendName.text = friend.Nickname;

        // 상태 텍스트 및 색상
        if (FriendConnectState != null)
        {
            switch (friend.Status)
            {
                case EFriendStatus.Online:
                    FriendConnectState.text = "온라인";
                    FriendConnectState.color = new Color(0.56f, 0.93f, 0.56f);
                    break;
                case EFriendStatus.InGame:
                    FriendConnectState.text = "게임 중";
                    FriendConnectState.color = new Color(0.53f, 0.81f, 0.92f);
                    break;
                case EFriendStatus.Offline:
                    FriendConnectState.text = "오프라인";
                    FriendConnectState.color = new Color(0.41f, 0.41f, 0.41f);
                    break;
            }
        }

        // 프로필 아웃핏 표시
        if (friendProfileSkin != null && friend.OutfitIds != null && friend.OutfitIds.Count > 0)
            friendProfileSkin.Init(friend.OutfitIds);
    }

    // 레거시 호환용 Refresh (닉네임만)
    public void Refresh(string nickname)
    {
        _friendNickname = nickname;
        FriendName.text = nickname;
    }

    public void SetFriendUid(string uid)
    {
        _friendUid = uid;
    }

    public void SetFriendInDate(string inDate)
    {
        _friendInDate = inDate;
    }

    private void OnClickChat()
    {
        if (string.IsNullOrEmpty(_friendNickname))
        {
            Debug.LogWarning("[UI_PanelFriendSlot] 친구 정보가 없습니다.");
            return;
        }

        Debug.Log($"[UI_PanelFriendSlot] 채팅 시작: {_friendNickname}");

        // UIChatManager를 통해 1:1 채팅 시작
        if (!string.IsNullOrEmpty(_friendUid))
        {
            string myUid = FirebaseManager.Instance.Auth.CurrentUser.UserId;
            UIChatManager.Instance.StartFriendChat(myUid, _friendUid);
        }

        OnChatButtonClicked?.Invoke(_friendUid, _friendNickname);
    }

    public void OnClickFriendInvite()
    {
        string friendNickname = FriendName.text;
        Debug.Log("[UI_PanelFriendSlot] 파티 초대 보내기");

        // TODO: PartyManager 연동 후 파티 초대 로직 구현
        // if (!PartyManager.Instance.IsInParty())
        //     PartyManager.Instance.CreateParty(AccountManager.Instance.CurrentAccount.Account_ID);
        // PartyManager.Instance.SendPartyInvite(friendNickname);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            Debug.Log("[UI_PanelFriendSlot] 채팅 채널 열기");
            
            OnClickChat();
        }
    }
}
