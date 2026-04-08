using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_FriendList : UI_Popup
{
    [Header("내 프로필")]
    [SerializeField] private ProfileSkin myProfileSkin;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text friendCountText;

    [Header("친구 목록")]
    [SerializeField] private Transform _contentParent;
    [SerializeField] private GameObject friendItemPrefab;

    // UI_FriendsListPopup에서 슬롯 이벤트 구독 시 사용
    public Transform contentParent => _contentParent;

    [Header("하단")]
    [SerializeField] private Button addFriendButton;

    [Header("패널")]
    [SerializeField] private UI_InvitePanel invitePanel;
    [SerializeField] private UI_PartyChatSlot partyChatSlot;

    private void OnEnable()
    {
        // 내 프로필 표시
        RefreshMyProfile();

        // 친구 목록 로드
        FriendManager.Instance.OnFriendListChanged += OnFriendListChanged;
        FriendManager.Instance.RefreshFriendList();

        // 뒤끝 실시간 알림으로 접속 상태 일괄 조회 (폴링 불필요)
        FriendManager.Instance.QueryAllFriendConnectionStatus();

        // 프로필 아웃핏 조회 (Firebase)
        FriendManager.Instance.RefreshFriendOutfits();
    }

    private void OnDisable()
    {
        if (FriendManager.Instance != null)
            FriendManager.Instance.OnFriendListChanged -= OnFriendListChanged;
    }

    private void RefreshMyProfile()
    {
        // 내 닉네임 표시
        if (nicknameText != null)
            nicknameText.text = AccountManager.Instance.CurrentAccount.Nickname;

        // TODO: 내 장착 아이템으로 프로필 표시 (ItemStorage 연동 후)
        // if (myProfileSkin != null)
        //     myProfileSkin.Init(ItemStorage.Instance.GetEquippedItemIds());
    }

    private void OnFriendListChanged()
    {
        // 기존 슬롯 정리
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // 친구 수 표시
        int count = FriendManager.Instance.GetFriendCount();
        if (friendCountText != null)
            friendCountText.text = $"{count} / {Friend.MAX_FRIEND_COUNT}";

        // 하단 버튼 상태
        if (addFriendButton != null)
            addFriendButton.interactable = !FriendManager.Instance.IsFriendListFull();

        // 온라인 → 게임 중 → 오프라인 정렬
        var sortedList = FriendManager.Instance.CachedFriendList
            .OrderBy(f => f.Status)
            .ToList();

        // 슬롯 생성
        foreach (var friend in sortedList)
        {
            GameObject item = Instantiate(friendItemPrefab, contentParent);
            var slot = item.GetComponent<UI_PanelFriendSlot>();
            slot.Refresh(friend);
        }
    }

    // 친구 추가 버튼 클릭 (Inspector에서 연결)
    public void OnClickAddFriend()
    {
        if (FriendManager.Instance.IsFriendListFull())
        {
            Debug.LogWarning("더 이상 친구 추가를 할 수 없습니다.");
            return;
        }

        // 친구 추가 팝업 열기
        var addPopup = FindObjectOfType<UI_PanelFriendAdd>(true);
        if (addPopup != null)
            addPopup.Open();
    }
}
