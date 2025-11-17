using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 친구 목록 통합 팝업 (내 정보, 파티 채팅, 친구 채팅)
/// </summary>
public class UI_FriendsListPopup : UI_Popup
{
    [Header("Top - My Profile")]
    public UI_MyProfilePanel MyProfilePanel;

    // [Header("Middle - Tab Buttons")]
    // public Button Button_Friends;
    // public Button Button_PartyChat;

    [Header("Content Panels")]
    public UI_FriendList FriendListPanel;       // 기존 UI_FriendList
    public UI_PartyChatPanel PartyChatPanel;
    public UI_FriendChatPanel FriendChatPanel;

    [Header("Prefab for Party Invite")]
    public GameObject PartyInvitePopupPrefab;
    public Transform PartyInvitePopupParent;

    // private enum TabType
    // {
    //     FriendList,
    //     PartyChat,
    //     FriendChat
    // }
    //
    // private TabType _currentTab = TabType.FriendList;

    private void Awake()
    {
        // 탭 버튼 이벤트 등록
        // if (TabButton_Friends != null)
        //     TabButton_Friends.onClick.AddListener(() => ShowTab(TabType.FriendList));
        //
        // if (TabButton_PartyChat != null)
        //     TabButton_PartyChat.onClick.AddListener(() => ShowTab(TabType.PartyChat));
    }

    private void OnEnable()
    {
        // UIChatManager 초기화 (로비 진입 시)
        if (UIChatManager.Instance != null && UIChatManager.Instance.ChatClient != null)
        {
            UIChatManager.Instance.InitializeChatClient();
        }

        // 파티 초대 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnPartyInviteReceived += OnPartyInviteReceived;
        }

        // 친구 슬롯의 채팅 버튼 이벤트 구독
        SubscribeToFriendListEvents();

        // 기본 탭 표시
        //ShowTab(TabType.FriendList);

        // 내 프로필 갱신
        if (MyProfilePanel != null)
        {
            MyProfilePanel.RefreshFriendCount();
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnPartyInviteReceived -= OnPartyInviteReceived;
        }

        UnsubscribeFromFriendListEvents();
    }

    /// <summary>
    /// 탭 전환
    /// </summary>
    // private void ShowTab(TabType tabType)
    // {
    //     _currentTab = tabType;
    //
    //     // 모든 패널 비활성화
    //     if (FriendListPanel != null)
    //         FriendListPanel.gameObject.SetActive(false);
    //
    //     if (PartyChatPanel != null)
    //         PartyChatPanel.gameObject.SetActive(false);
    //
    //     if (FriendChatPanel != null)
    //         FriendChatPanel.gameObject.SetActive(false);
    //
    //     // 선택된 탭만 활성화
    //     switch (tabType)
    //     {
    //         case TabType.FriendList:
    //             if (FriendListPanel != null)
    //                 FriendListPanel.gameObject.SetActive(true);
    //             break;
    //
    //         case TabType.PartyChat:
    //             if (PartyChatPanel != null)
    //             {
    //                 PartyChatPanel.gameObject.SetActive(true);
    //
    //                 // 파티에 참여 중이면 초기화
    //                 if (PartyManager.Instance != null && PartyManager.Instance.IsInParty())
    //                 {
    //                     PartyChatPanel.Initialize(PartyManager.Instance.CurrentPartyId);
    //                 }
    //             }
    //             break;
    //
    //         case TabType.FriendChat:
    //             if (FriendChatPanel != null)
    //                 FriendChatPanel.gameObject.SetActive(true);
    //             break;
    //     }
    //
    //     // 탭 버튼 강조 (TODO: 버튼 색상 변경)
    //     UpdateTabButtonHighlight();
    // }

    /// <summary>
    /// 탭 버튼 강조 표시
    /// </summary>
    private void UpdateTabButtonHighlight()
    {
        // TODO: 현재 선택된 탭 버튼 강조 표시
        // 예: 버튼 색상 변경, 언더라인 표시 등
    }

    /// <summary>
    /// 친구 목록의 이벤트 구독
    /// </summary>
    private void SubscribeToFriendListEvents()
    {
        if (FriendListPanel == null || FriendListPanel.contentParent == null) return;

        foreach (Transform child in FriendListPanel.contentParent)
        {
            var friendSlot = child.GetComponent<UI_PanelFriendSlot>();
            if (friendSlot != null)
            {
                friendSlot.OnChatButtonClicked += OnFriendChatButtonClicked;
            }
        }
    }

    /// <summary>
    /// 친구 목록의 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromFriendListEvents()
    {
        if (FriendListPanel == null || FriendListPanel.contentParent == null) return;

        foreach (Transform child in FriendListPanel.contentParent)
        {
            var friendSlot = child.GetComponent<UI_PanelFriendSlot>();
            if (friendSlot != null)
            {
                friendSlot.OnChatButtonClicked -= OnFriendChatButtonClicked;
            }
        }
    }

    /// <summary>
    /// 친구 슬롯의 채팅 버튼 클릭 이벤트
    /// </summary>
    private void OnFriendChatButtonClicked(string friendUid, string friendNickname)
    {
        Debug.Log($"[UI_FriendsListPopup] 친구 채팅 시작: {friendNickname}");

        // 친구 채팅 패널로 전환
        //ShowTab(TabType.FriendChat);

        // 친구 채팅 시작
        if (FriendChatPanel != null)
        {
            FriendChatPanel.StartChatWithFriend(friendUid, friendNickname);
        }
    }

    /// <summary>
    /// 파티 초대 수신 이벤트
    /// </summary>
    private void OnPartyInviteReceived(string inviterName, string partyId)
    {
        Debug.Log($"[UI_FriendsListPopup] 파티 초대 수신: {inviterName} → {partyId}");

        // 파티 초대 팝업 생성
        if (PartyInvitePopupPrefab != null && PartyInvitePopupParent != null)
        {
            GameObject popupObj = Instantiate(PartyInvitePopupPrefab, PartyInvitePopupParent);
            var invitePopup = popupObj.GetComponent<UI_PartyInvitePopup>();

            if (invitePopup != null)
            {
                invitePopup.SetInviteInfo(partyId, inviterName, inviterName);
            }
        }
    }
}
