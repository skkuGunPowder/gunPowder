using System;
using System.Collections.Generic;
using BackndChat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum ChatChannel { All, Team, Whisper }
public class UI_IngameChatPopup : UI_Popup
{
    public GameObject ChatContent = null;
    public Button SendButton = null;
    public ScrollRect ChatScrollRect = null; // 채팅 스크롤뷰
    public GameObject ChatBubblePrefab;

    // ChatPrefab
    public GameObject ChatListLeftPrefab;
    public GameObject ChatListRightPrefab; // 내 메세지

    // UI 토글 관련
    [SerializeField] private GameObject MiniStateGroup;
    [SerializeField] private GameObject FullStateGroup;
    private bool _isChatOpen = false;

    // InputField 컴포넌트들
    [SerializeField] private UI_IngameChatInputField inputFieldMini;
    [SerializeField] private UI_IngameChatInputField inputFieldFull;

    private Action _closeCallback;
    // 인게임 채팅이 동작할 씬 목록
    private readonly string[] _activeScenes = { "WaitingRoom", "Beach1", "Dock1", "Forest1" };

    // 채팅 도배 방지 시스템
    private Queue<float> _recentChatTimes = new Queue<float>(); // 최근 채팅 시간 기록
    private const int MAX_MESSAGES_THRESHOLD = 4; // 연속 채팅 제한 횟수
    private const float SPAM_CHECK_WINDOW = 3f; // 도배 체크 시간 (3초 내 4번 초과)
    private const float CHAT_BAN_DURATION = 10f; // 채팅 금지 시간 (10초)
    private float _chatBanEndTime = 0f; // 채팅 금지 종료 시간
    private bool _isChatBanned = false; // 채팅 금지 상태
    private bool _chatBanMessageShown = false; // 채팅 금지 메시지 표시 여부 (중복 방지)

    // 채팅 채널 관련
    private ChatChannel _currentChannel = ChatChannel.All;
    private string _whisperTargetName = ""; // 귓속말 대상
    
    private void Awake()
    {
        // InputField 컴포넌트들의 OnSubmit 이벤트 구독
        if (inputFieldMini != null)
        {
            inputFieldMini.OnSubmit += OnChatSubmit;
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.OnSubmit += OnChatSubmit;
        }

        _closeCallback = Close;

        // ChatScrollRect가 설정되지 않았으면 자동으로 찾기
        if (ChatScrollRect == null && ChatContent != null)
        {
            ChatScrollRect = ChatContent.GetComponentInParent<ScrollRect>();
        }
    }

    private void OnEnable()
    {
        ScrollToBottomCoroutine();
    }


    private void Start()
    {
        // UIChatManager 이벤트 구독 및 기존 메시지 로드
        if (UIChatManager.Instance != null)
        {
            // 이벤트 구독
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived; // 중복 방지
            UIChatManager.Instance.OnChatMessageReceived += OnChatMessageReceived;

            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft; // 중복 방지
            UIChatManager.Instance.OnChannelLeft += OnChannelLeft;

            Debug.Log("[UI_IngameChatPopup] UIChatManager 이벤트 구독 완료");

            // 기존 메시지 복원
            LoadPreviousMessages();
        }
        else
        {
            Debug.LogError("[UI_IngameChatPopup] UIChatManager.Instance가 null입니다!");
        }
    }
    private void Update()
    {
        // Tab 키로 채널 변경
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleChannel();
        }

        // T 키로 Mini/Full 전환 (InputField가 포커스 중이 아닐 때만)
        if (Input.GetKeyDown(KeyCode.T) && !IsAnyInputFieldFocused())
        {
            ToggleChatPopup();
        }
    }

    /// <summary>
    /// Mini 또는 Full InputField가 포커스 중인지 확인
    /// </summary>
    private bool IsAnyInputFieldFocused()
    {
        bool miniFocused = inputFieldMini != null && inputFieldMini.IsFocused();
        bool fullFocused = inputFieldFull != null && inputFieldFull.IsFocused();
        return miniFocused || fullFocused;
    }

    public void ToggleChatPopup()
    {
        if (MiniStateGroup.activeInHierarchy)
        {
            MiniStateGroup.SetActive(false);
            FullStateGroup.SetActive(true);
        }
        else
        {
            MiniStateGroup.SetActive(true);
            FullStateGroup.SetActive(false);
        }
    }
    // 채널 변경 로직 (Tab 키)
    private void CycleChannel()
    {
        // 귓속말 대상이 없으면 전체 <-> 팀 전환, 있으면 3개 순환
        int next = (int)_currentChannel + 1;
        // ... 분기 처리 로직 ...
    
       // UpdateChannelUI();
    }
    // 채널 변경시 나올 UI
    private void UpdateChannelUI()
    {
        //TODO: 채팅채널 바뀔때마다 나올 UI 추가
    }

    // 채널 변경 시 호출 - 글자 수 제한 업데이트
    private void UpdateCharacterLimit()
    {
        int limit = (_currentChannel == ChatChannel.Whisper) ? 50 : 20;

        if (inputFieldMini != null)
        {
            inputFieldMini.SetCharacterLimit(limit);
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.SetCharacterLimit(limit);
        }
    }
    
    /// <summary>
    /// UIChatManager에서 기존 메시지를 가져와서 UI에 표시
    /// </summary>
    private void LoadPreviousMessages()
    {
        if (UIChatManager.Instance == null)
        {
            Debug.LogWarning("[UI_IngameChatPopup] UIChatManager.Instance가 null입니다.");
            return;
        }

        // 현재 채널의 메시지 가져오기
        List<MessageInfo> messages = UIChatManager.Instance.GetCurrentChannelMessages();

        if (messages.Count == 0)
        {
            Debug.Log("[UI_IngameChatPopup] 복원할 메시지가 없습니다.");
            return;
        }

        Debug.Log($"[UI_IngameChatPopup] 기존 메시지 {messages.Count}개 복원 중...");

        // 각 메시지를 UI에 추가 (시스템 메시지는 제외)
        foreach (MessageInfo messageInfo in messages)
        {
            CreateChatListUI(messageInfo);
        }

        Debug.Log($"[UI_IngameChatPopup] 기존 메시지 복원 완료: {messages.Count}개");

        // 메시지 복원 후 스크롤을 맨 아래로 (Coroutine으로 지연 처리)
        ScrollToBottomCoroutine();
    }

    /// <summary>
    /// 현재 씬이 인게임 씬인지 확인
    /// </summary>
    private bool IsInGameScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var sceneName in _activeScenes)
        {
            if (currentScene == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 채팅창을 열 수 있는지 체크 (조건 확인만)
    /// </summary>
    public bool TryOpen()
    {
        // 인게임 씬이 아니면 열지 않음
        if (!IsInGameScene()) return false;

        // 이미 열려있으면 열지 않음
        if (gameObject.activeSelf) return false;

        InputHandler.BlockInput = true;

        Debug.Log("[UI_IngameChat] 채팅 Popup 열림");

        return true;
    }

    private void OnDisable()
    {
        InputHandler.BlockInput = false;
        Debug.Log("[UI_IngameChat] 채팅 Popup 닫힘");
                
    }
    /// <summary>
    /// UI_Popup.Open() 오버라이드 - PopupManager에서 호출
    /// </summary>
    // public new void Open(System.Action closeCallback = null)
    // {
    //     base.Open(closeCallback);
    // }


    /// <summary>
    /// 채팅 메시지 수신 시 호출되는 콜백
    /// </summary>
    private void OnChatMessageReceived(MessageInfo messageInfo)
    {
        Debug.Log($"[UI_IngameChatPopup] OnChatMessageReceived 콜백 호출: {messageInfo.GamerName} - {messageInfo.Message}");
        CreateChatListUI(messageInfo);

        // 새 메시지 추가 후 스크롤을 맨 아래로 (Coroutine으로 지연 처리)
        ScrollToBottomCoroutine();
    }

    /// <summary>
    /// 채팅 메시지 UI 생성 (신규 메시지 및 기존 메시지 복원에 공통 사용)
    /// </summary>
    private void CreateChatListUI(MessageInfo messageInfo)
    {
        if (ChatContent == null)
        {
            Debug.LogError("[UI_IngameChatPopup] ChatContent가 null입니다!");
            return;
        }

        if (ChatListLeftPrefab == null)
        {
            Debug.LogError("[UI_IngameChatPopup] ChatListPrefab이 null입니다!");
            return;
        }

        bool isMyMessage = false;
        bool isSystemMessage = messageInfo.GamerName == "SYSTEM";
        GameObject chatList;

        // 시스템 메시지가 아닌 경우 자신의 메시지인지 확인
        if (!isSystemMessage && AccountManager.Instance != null && AccountManager.Instance.CurrentAccount != null)
        {
            isMyMessage = messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname;
        }

        // 채팅 리스트 UI 생성 (시스템 메시지는 항상 왼쪽)
        if(isMyMessage)
            chatList = Instantiate(ChatListRightPrefab, ChatContent.transform);
        else
            chatList = Instantiate(ChatListLeftPrefab, ChatContent.transform);

        if (chatList == null)
        {
            Debug.LogError("[UI_IngameChatPopup] ChatList 프리팹 Instantiate 실패!");
            return;
        }

        UIChatList chatListComponent = chatList.GetComponent<UIChatList>();
        if (chatListComponent == null)
        {
            Debug.LogError("[UI_IngameChatPopup] UIChatList 컴포넌트를 찾을 수 없습니다.");
            Destroy(chatList);
            return;
        }

        // 채팅 리스트에 데이터 설정
        chatListComponent.SetData(
            messageInfo.Index,
            messageInfo.Avatar,
            messageInfo.GamerName,
            messageInfo.Message,
            messageInfo.Time,
            messageInfo.Tag,
            null, // OnReportButton - 인게임에서는 사용하지 않음
            null, // OnTranslateCheckButton - 인게임에서는 사용하지 않음
            isMyMessage
        );

        // 시스템 메시지인 경우 노란색 스타일 적용
        if (isSystemMessage)
        {
            chatListComponent.ApplySystemMessageStyle();
            Debug.Log($"[UI_IngameChatPopup] 시스템 메시지 표시: {messageInfo.Message}");
        }
    }

    /// <summary>
    /// 스크롤을 맨 아래로 이동 (최신 메시지 보이도록)
    /// Coroutine으로 지연 처리하여 레이아웃이 완전히 업데이트된 후 스크롤
    /// </summary>
    private void ScrollToBottomCoroutine()
    {
        if (ChatScrollRect == null) return;

        // Canvas를 강제로 업데이트하여 레이아웃 재계산
        Canvas.ForceUpdateCanvases();

        // verticalNormalizedPosition: 0 = 맨 위, 1 = 맨 아래
        // (Content의 Pivot 설정에 따라 다를 수 있음. 0과 1 중 맨 아래로 가는 값 사용)
        ChatScrollRect.verticalNormalizedPosition = 0f;

        Debug.Log($"[UI_IngameChatPopup] 스크롤을 맨 아래로 이동 완료 (position: {ChatScrollRect.verticalNormalizedPosition})");
    }

    /// <summary>
    /// 채널 퇴장 시 호출되는 콜백
    /// </summary>
    private void OnChannelLeft()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft;
        }
        ClearAllMessages();
        Debug.Log("[UI_IngameChat] 채널 퇴장으로 인한 채팅 메시지 전체 삭제");
    }

    /// <summary>
    /// 채팅 제출 이벤트 핸들러 (UI_IngameChatInputField에서 호출)
    /// </summary>
    private void OnChatSubmit(string text)
    {
        // 채팅 금지 상태 체크
        if (_isChatBanned)
        {
            float remainingTime = _chatBanEndTime - Time.time;
            if (remainingTime > 0)
            {
                Debug.LogWarning($"[UI_IngameChatPopup] 채팅 금지 중입니다. 남은 시간: {remainingTime:F1}초");

                // 채팅 금지 메시지를 한 번만 표시 (중복 방지)
                if (!_chatBanMessageShown)
                {
                    ShowChatBanMessage(Mathf.CeilToInt(remainingTime));
                    _chatBanMessageShown = true;
                }

                return;
            }
            else
            {
                // 금지 시간 종료 - 모든 플래그 리셋
                _isChatBanned = false;
                _chatBanMessageShown = false;
                Debug.Log("[UI_IngameChatPopup] 채팅 금지 해제");
                RestorePlaceholderToDefault();
            }
        }

        // 도배 방지 체크
        if (CheckForSpam())
        {
            // 도배로 판단 - 10초 채팅 금지
            _isChatBanned = true;
            _chatBanMessageShown = false; // 새로운 금지이므로 플래그 리셋
            _chatBanEndTime = Time.time + CHAT_BAN_DURATION;
            Debug.LogWarning($"[UI_IngameChatPopup] 도배 감지! {CHAT_BAN_DURATION}초간 채팅 금지");
            ShowChatBanMessage(Mathf.CeilToInt(CHAT_BAN_DURATION));
            _chatBanMessageShown = true; // 메시지 표시했으므로 플래그 설정
            return;
        }

        // 채팅 시간 기록
        _recentChatTimes.Enqueue(Time.time);

        Debug.Log($"[UI_IngameChatPopup] 채팅 메시지 전송: {text}");

        // 채팅 메시지 전송
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.SendChatMessage(text);
        }
        else
        {
            Debug.LogError("[UI_IngameChatPopup] UIChatManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 도배 여부 체크 (5초 내 4번 초과 채팅하면 도배로 판단)
    /// </summary>
    private bool CheckForSpam()
    {
        float currentTime = Time.time;

        // 오래된 채팅 시간 제거 (5초 이상 지난 것들)
        while (_recentChatTimes.Count > 0 && currentTime - _recentChatTimes.Peek() > SPAM_CHECK_WINDOW)
        {
            _recentChatTimes.Dequeue();
        }

        // 현재 큐에 5번째 메시지가 들어가려고 할 때 체크 (4번 초과)
        if (_recentChatTimes.Count >= MAX_MESSAGES_THRESHOLD)
        {
            // 첫 번째 메시지와 현재 시간의 차이가 5초 이하면 도배
            float firstMessageTime = _recentChatTimes.Peek();
            if (currentTime - firstMessageTime <= SPAM_CHECK_WINDOW)
            {
                // 도배 감지 - 큐 초기화
                _recentChatTimes.Clear();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 채팅 금지 메시지 표시
    /// </summary>
    private void ShowChatBanMessage(int seconds)
    {
        Debug.LogWarning($"[채팅 금지] 도배 방지를 위해 {seconds}초간 채팅이 제한됩니다.");

        // SYSTEM 메시지 생성하여 채팅 리스트에 추가
        BackndChat.MessageInfo systemMessage = new BackndChat.MessageInfo()
        {
            GamerName = "SYSTEM",
            Message = "요청이 너무 빠르게 입력되고 있어요. 잠시 후 다시 시도해주세요.",
            Avatar = "",
            Time = System.DateTime.Now.ToString("HH:mm"),
            Index = 0,
            Tag = "",
            ChannelGroup = "",
            ChannelName = "",
            ChannelNumber = 0
        };

        // 채팅 리스트에 시스템 메시지 추가
        CreateChatListUI(systemMessage);

        // 스크롤을 맨 아래로 이동
        ScrollToBottomCoroutine();

        // 두 InputField의 placeholder 업데이트
        if (inputFieldMini != null)
        {
            inputFieldMini.UpdatePlaceholder($"채팅 금지 ({seconds}초 남음)", 1f);
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.UpdatePlaceholder($"채팅 금지 ({seconds}초 남음)", 1f);
        }
    }

    /// <summary>
    /// 생성된 모든 채팅 메시지를 삭제
    /// </summary>
    public void ClearAllMessages()
    {
        if (ChatContent == null) return;

        // ChatContent의 모든 자식 오브젝트 삭제
        foreach (Transform child in ChatContent.transform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("[UI_IngameChat] 모든 채팅 메시지 삭제됨");
    }

    /// <summary>
    /// 모든 InputField의 Placeholder를 기본값으로 복구
    /// </summary>
    private void RestorePlaceholderToDefault()
    {
        if (inputFieldMini != null)
        {
            inputFieldMini.RestorePlaceholderToDefault();
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.RestorePlaceholderToDefault();
        }

        Debug.Log("[UI_IngameChatPopup] Placeholder 복구: ENTER MESSAGE...");
    }

    private void OnDestroy()
    {
        // 채팅 메시지 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft;
        }

        // InputField 이벤트 구독 해제
        if (inputFieldMini != null)
        {
            inputFieldMini.OnSubmit -= OnChatSubmit;
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.OnSubmit -= OnChatSubmit;
        }

        Debug.Log("[UI_IngameChatPopup] Destroyed - 이벤트 구독 해제 완료");
    }
}
