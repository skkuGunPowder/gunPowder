using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackndChat;

/// <summary>
/// 친구 1:1 채팅 패널
/// </summary>
public class UI_FriendChatPanel : MonoBehaviour
{
    [Header("Friend Info")]
    public TextMeshProUGUI FriendNameText;
    public Image FriendProfileImage;
    public Button BackButton;

    [Header("Chat")]
    public Transform ChatContentParent;
    public GameObject ChatMessagePrefab;
    public TMP_InputField ChatInputField;
    public Button SendButton;
    public ScrollRect ChatScrollRect;

    private string _friendUid;
    private string _friendNickname;
    private string _myUid;
    private string _channelGroup = "friend";
    private string _channelName;
    private bool _isInitialized = false;

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (SendButton != null)
            SendButton.onClick.AddListener(OnSendButtonClicked);

        if (BackButton != null)
            BackButton.onClick.AddListener(OnBackButtonClicked);

        // 인풋 필드 엔터키 처리
        if (ChatInputField != null)
        {
            ChatInputField.onSubmit.AddListener((text) => OnSendButtonClicked());
        }
    }

    private void OnEnable()
    {
        // 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnFriendChatReceived += OnReceiveFriendChatMessage;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnFriendChatReceived -= OnReceiveFriendChatMessage;
        }

        // 채널 퇴장
        if (_isInitialized && !string.IsNullOrEmpty(_myUid) && !string.IsNullOrEmpty(_friendUid))
        {
            UIChatManager.Instance?.LeaveFriendChat(_myUid, _friendUid);
        }
    }

    /// <summary>
    /// 친구 채팅 시작
    /// </summary>
    public void StartChatWithFriend(string friendUid, string friendNickname)
    {
        if (AccountManager.Instance?.CurrentAccount == null)
        {
            Debug.LogError("[UI_FriendChatPanel] 계정 정보가 없습니다.");
            return;
        }

        _myUid = AccountManager.Instance.CurrentAccount.Account_ID;
        _friendUid = friendUid;
        _friendNickname = friendNickname;
        _channelName = CreateFriendChannelName(_myUid, _friendUid);
        _isInitialized = true;

        // 친구 이름 표시
        if (FriendNameText != null)
        {
            FriendNameText.text = _friendNickname;
        }

        // 프로필 이미지 설정 (TODO)
        if (FriendProfileImage != null)
        {
            // TODO: 친구 프로필 이미지 로드
        }

        // UIChatManager에게 친구 채팅 채널 입장 요청
        UIChatManager.Instance.StartFriendChat(_myUid, _friendUid);

        // 채팅 기록 로드
        LoadChatHistory();

        Debug.Log($"[UI_FriendChatPanel] 친구 채팅 시작: {_friendNickname} ({_friendUid})");
    }

    /// <summary>
    /// 친구 채널명 생성 (항상 정렬된 순서)
    /// </summary>
    private string CreateFriendChannelName(string uid1, string uid2)
    {
        return string.Compare(uid1, uid2) < 0
            ? $"{uid1}_{uid2}"
            : $"{uid2}_{uid1}";
    }

    /// <summary>
    /// 채팅 기록 로드
    /// </summary>
    private void LoadChatHistory()
    {
        if (UIChatManager.Instance == null) return;

        // 채팅 메시지 클리어
        ClearChatMessages();

        // Backend Chat에서 채팅 기록 가져오기
        ulong channelNumber = 0;
        var messages = UIChatManager.Instance.GetChannelMessages(_channelGroup, _channelName, channelNumber);

        // 메시지 표시
        foreach (var message in messages)
        {
            AddChatMessage(message);
        }

        // 스크롤 맨 아래로
        ScrollToBottom();
    }

    /// <summary>
    /// 채팅 메시지 수신 이벤트 핸들러
    /// </summary>
    private void OnReceiveFriendChatMessage(MessageInfo messageInfo)
    {
        // 현재 채팅 중인 친구의 메시지인지 확인
        if (messageInfo.ChannelName != _channelName) return;

        AddChatMessage(messageInfo);
        ScrollToBottom();
    }

    /// <summary>
    /// 채팅 메시지 추가
    /// </summary>
    private void AddChatMessage(MessageInfo messageInfo)
    {
        if (ChatMessagePrefab == null || ChatContentParent == null) return;

        GameObject messageObj = Instantiate(ChatMessagePrefab, ChatContentParent);
        var messageSlot = messageObj.GetComponent<UI_ChatMessageSlot>();

        if (messageSlot != null)
        {
            bool isMyMessage = messageInfo.GamerName == GetMyNickname();
            messageSlot.SetMessage(messageInfo, isMyMessage);
        }
    }

    /// <summary>
    /// 채팅 메시지 클리어
    /// </summary>
    private void ClearChatMessages()
    {
        if (ChatContentParent == null) return;

        foreach (Transform child in ChatContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 채팅 전송 버튼 클릭
    /// </summary>
    private void OnSendButtonClicked()
    {
        if (ChatInputField == null) return;

        string message = ChatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        // 메시지 전송
        SendChatMessage(message);

        // 인풋 필드 클리어
        ChatInputField.text = "";
        ChatInputField.ActivateInputField();
    }

    /// <summary>
    /// 채팅 메시지 전송
    /// </summary>
    private void SendChatMessage(string text)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[UI_FriendChatPanel] 채팅이 초기화되지 않았습니다.");
            return;
        }

        if (UIChatManager.Instance == null)
        {
            Debug.LogError("[UI_FriendChatPanel] UIChatManager가 없습니다.");
            return;
        }

        // UIChatManager를 통해 친구 채팅 전송
        UIChatManager.Instance.SendFriendMessage(_myUid, _friendUid, text);
    }

    /// <summary>
    /// 뒤로 가기 버튼 클릭
    /// </summary>
    private void OnBackButtonClicked()
    {
        // 친구 목록으로 돌아가기
        // UI_FriendsListPopup에서 처리
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 스크롤 맨 아래로
    /// </summary>
    private void ScrollToBottom()
    {
        if (ChatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            ChatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 내 닉네임 가져오기
    /// </summary>
    private string GetMyNickname()
    {
        if (AccountManager.Instance?.CurrentAccount != null)
        {
            return AccountManager.Instance.CurrentAccount.Nickname;
        }
        return "Unknown";
    }
}
