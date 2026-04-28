using System;
using System.Collections.Generic;
using System.Linq;
using BackndChat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using PhotonPlayer = Photon.Realtime.Player;

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
    private static readonly string[] _activeScenes =
        Enum.GetValues(typeof(EMap))
            .Cast<EMap>()
            .Where(m => m != EMap.Random && m != EMap.Count)
            .Select(m => m.ToString())
            .Prepend("WaitingRoom")
            .ToArray();

    // 채팅 도배 방지 시스템
    private Queue<float> _recentChatTimes = new Queue<float>(); // 최근 채팅 시간 기록
    private const int MAX_MESSAGES_THRESHOLD = 4; // 연속 채팅 제한 횟수
    private const float SPAM_CHECK_WINDOW = 3f; // 도배 체크 시간 (3초 내 4번 초과)
    private const float CHAT_BAN_DURATION = 10f; // 채팅 금지 시간 (10초)
    private float _chatBanEndTime = 0f; // 채팅 금지 종료 시간
    private bool _isChatBanned = false; // 채팅 금지 상태
    private bool _chatBanMessageShown = false; // 채팅 금지 메시지 표시 여부 (중복 방지)

    // 채팅 개수 제한
    private const int MAX_CHAT_MESSAGES = 50; // 최대 채팅 메시지 개수

    // 채팅 채널 관련
    private ChatChannel _currentChannel = ChatChannel.All;
    private string _whisperTargetName = ""; // 귓속말 대상
    
    // 방금 닫혔는가?
    private float _lastCloseTime = 0f;
    
    // [추가] 게임 종료 상태 플래그
    private bool _isGameEnded = false;
    
    private void Awake()
    {
        // InputField 컴포넌트들의 OnSubmit 이벤트 구독
        if (inputFieldMini != null)
        {
            inputFieldMini.OnSubmit += OnChatSubmit;
            // [변경] Close -> CloseByEnter 로 변경
            inputFieldMini.OnEmptySubmit += CloseByEnter;
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.OnSubmit += OnChatSubmit;
            // [변경] Close -> CloseByEnter 로 변경
            inputFieldFull.OnEmptySubmit += CloseByEnter;
        }

        _closeCallback = Close;

        // ChatScrollRect가 설정되지 않았으면 자동으로 찾기
        if (ChatScrollRect == null && ChatContent != null)
        {
            ChatScrollRect = ChatContent.GetComponentInParent<ScrollRect>();
        }

        
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

           
        }
        else
        {
            Debug.LogError("[UI_IngameChatPopup] UIChatManager.Instance가 null입니다!");
        }

    
        // ★ 팀 컬러 변경 이벤트 구독 추가 (여기서 해도 되고 Start에서 해도 됨)
        // 중복 구독 방어: Popup이 SetActive 토글로 재사용되는 경우 Start가 다시 호출될 가능성에 대비.
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnPlayerColorChanged -= OnPlayerColorChangedWrapper;
            EventManager.Instance.OnPlayerColorChanged += OnPlayerColorChangedWrapper;
            EventManager.Instance.OnGameStart -= OnGameStartWrapper;
            EventManager.Instance.OnGameStart += OnGameStartWrapper;
            EventManager.Instance.OnGameOver -= OnGameOverWrapper;
            EventManager.Instance.OnGameOver += OnGameOverWrapper;
        }
            // 기존 메시지 복원
        LoadPreviousMessages();
    }
    private void OnEnable()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }
    // [추가] 게임 시작 시 상태 초기화 (재시작 시 채팅 가능)
    private void OnGameStartWrapper()
    {
        _isGameEnded = false;
        Close();
    }

    private void CreateChatListUI(MessageInfo messageInfo)
    {
        if (ChatContent == null || ChatListLeftPrefab == null) return;

        bool isMyMessage = false;
        bool isSystemMessage = messageInfo.GamerName == "SYSTEM";

        // 내 메시지인지 확인
        if (!isSystemMessage && AccountManager.Instance != null && AccountManager.Instance.CurrentAccount != null)
        {
            isMyMessage = messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname;
        }

        // 프리팹 생성
        GameObject chatListObj = isMyMessage
            ? Instantiate(ChatListRightPrefab, ChatContent.transform)
            : Instantiate(ChatListLeftPrefab, ChatContent.transform);

        UIChatList chatListComponent = chatListObj.GetComponent<UIChatList>();
        if (chatListComponent == null) return;

        // ★ 핵심: 메시지 작성자의 팀 정보 가져오기 (닉네임 기반)
        EInGameTeam playerTeam = EInGameTeam.Red;
        int actualActorNumber = -1;
        
        if (!isSystemMessage)
        {
            // 닉네임으로 실제 Photon ActorNumber 찾기
            actualActorNumber = GetActorNumberByNickname(messageInfo.GamerName);

            // 닉네임으로 팀 정보 가져오기
            playerTeam = GetTeamByNickname(messageInfo.GamerName);
        }

        // 데이터 설정 (team 정보 전달)
        // ★ 실제 ActorNumber를 Index로 전달 (음수가 아닐 때만)
        UInt64 indexToUse = actualActorNumber > 0 ? (UInt64)actualActorNumber : messageInfo.Index;

        // ★ [수정] UIChatManager에서 이미 파싱 완료 - 옷 정보만 가져오기
        List<string> outfitList = UIChatManager.Instance.GetMessageOutfit(messageInfo.Index);

        chatListComponent.SetData(
            outfitList,
            indexToUse,
            messageInfo.Avatar,
            messageInfo.GamerName,
            messageInfo.Message, // ★ UIChatManager에서 이미 파싱된 메시지
            messageInfo.Time,
            messageInfo.Tag,
            null,
            null,
            isMyMessage,
            playerTeam // ★ 찾아낸 팀 정보를 넘겨줌
        );

        if (isSystemMessage)
        {
            chatListComponent.ApplySystemMessageStyle();
        }

        // 채팅 개수 제한: 50개 초과 시 가장 오래된 것 삭제
        LimitChatMessageCount();
    }


    /// <summary>
    /// ActorNumber를 이용해 현재 룸에 있는 플레이어의 팀 정보를 가져오는 헬퍼 함수
    /// </summary>
    private EInGameTeam GetTeamByActorNumber(int actorNumber)
    {
        if (PhotonNetwork.CurrentRoom == null) return EInGameTeam.Red;

        // 현재 방의 플레이어 리스트에서 검색
        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out PhotonPlayer targetPlayer))
        {
            // 커스텀 프로퍼티에서 Team 정보 추출
            if (targetPlayer.CustomProperties.TryGetValue(EProperties.Team.ToString(), out object teamObj))
            {
                return (EInGameTeam)teamObj;
            }
        }

        return EInGameTeam.Red;
    }

    /// <summary>
    /// 닉네임으로 Photon 플레이어를 찾아서 ActorNumber를 반환
    /// </summary>
    private int GetActorNumberByNickname(string nickname)
    {
        if (PhotonNetwork.CurrentRoom == null) return -1;

        foreach (var playerPair in PhotonNetwork.CurrentRoom.Players)
        {
            if (playerPair.Value.NickName == nickname)
            {
                return playerPair.Value.ActorNumber;
            }
        }

        return -1;
    }

    /// <summary>
    /// 닉네임으로 플레이어의 팀 정보를 가져오는 헬퍼 함수
    /// </summary>
    private EInGameTeam GetTeamByNickname(string nickname)
    {
        if (PhotonNetwork.CurrentRoom == null) return EInGameTeam.Red;

        foreach (var playerPair in PhotonNetwork.CurrentRoom.Players)
        {
            if (playerPair.Value.NickName == nickname)
            {
                // 커스텀 프로퍼티에서 Team 정보 추출
                if (playerPair.Value.CustomProperties.TryGetValue(EProperties.Team.ToString(), out object teamObj))
                {
                    return (EInGameTeam)teamObj;
                }
            }
        }

        return EInGameTeam.Red;
    }
    // [추가] 게임 종료 시 플래그 설정 및 창 닫기
    private void OnGameOverWrapper()
    {
        _isGameEnded = true;
        Close();
    }

    
    private void Update()
    {
        
        if(this.isActiveAndEnabled)
            InputHandler.BlockInput = true;
            
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

            // [추가] Full 모드가 열릴 때 스크롤을 맨 아래로 내림
            StartCoroutine(ScrollToBottomCoroutine());
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
        StartCoroutine(ScrollToBottomCoroutine());
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
        // [추가] 게임이 끝났으면 채팅창 열기 차단
        if (_isGameEnded) return false;
        // 인게임 씬이 아니면 열지 않음
        if (!IsInGameScene()) return false;

        // 이미 열려있으면 열지 않음
        if (gameObject.activeSelf) return false;
        
        // [추가] 닫힌지 0.2초가 안 지났으면(같은 프레임 포함) 열지 않음
        if (Time.time - _lastCloseTime < 0.2f) return false;

        Debug.Log("[UI_IngameChat] 채팅 Popup 열림");

        return true;
    }

    // [추가] 엔터키 입력으로 닫힐 때 호출되는 함수
    private void CloseByEnter()
    {
        _lastCloseTime = Time.time; // 닫힌 시간 기록
        Close();
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

        // [수정] 오브젝트가 켜져있을 때만 코루틴을 실행하도록 방어 코드 추가
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(ScrollToBottomCoroutine());
        }
    }

    /// <summary>
    /// 채팅 메시지 UI 생성 (신규 메시지 및 기존 메시지 복원에 공통 사용)
    /// </summary>
    // private void CreateChatListUI(MessageInfo messageInfo)
    // {
    //     if (ChatContent == null)
    //     {
    //         Debug.LogError("[UI_IngameChatPopup] ChatContent가 null입니다!");
    //         return;
    //     }
    //
    //     if (ChatListLeftPrefab == null)
    //     {
    //         Debug.LogError("[UI_IngameChatPopup] ChatListPrefab이 null입니다!");
    //         return;
    //     }
    //
    //     bool isMyMessage = false;
    //     bool isSystemMessage = messageInfo.GamerName == "SYSTEM";
    //     GameObject chatList;
    //
    //     // 시스템 메시지가 아닌 경우 자신의 메시지인지 확인
    //     if (!isSystemMessage && AccountManager.Instance != null && AccountManager.Instance.CurrentAccount != null)
    //     {
    //         isMyMessage = messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname;
    //     }
    //
    //     // 채팅 리스트 UI 생성 (시스템 메시지는 항상 왼쪽)
    //     if(isMyMessage)
    //         chatList = Instantiate(ChatListRightPrefab, ChatContent.transform);
    //     else
    //         chatList = Instantiate(ChatListLeftPrefab, ChatContent.transform);
    //
    //     if (chatList == null)
    //     {
    //         Debug.LogError("[UI_IngameChatPopup] ChatList 프리팹 Instantiate 실패!");
    //         return;
    //     }
    //
    //     UIChatList chatListComponent = chatList.GetComponent<UIChatList>();
    //     if (chatListComponent == null)
    //     {
    //         Debug.LogError("[UI_IngameChatPopup] UIChatList 컴포넌트를 찾을 수 없습니다.");
    //         Destroy(chatList);
    //         return;
    //     }
    //
    //     // 채팅 리스트에 데이터 설정
    //     chatListComponent.SetData(
    //         messageInfo.Index,
    //         messageInfo.Avatar,
    //         messageInfo.GamerName,
    //         messageInfo.Message,
    //         messageInfo.Time,
    //         messageInfo.Tag,
    //         null, // OnReportButton - 인게임에서는 사용하지 않음
    //         null, // OnTranslateCheckButton - 인게임에서는 사용하지 않음
    //         isMyMessage
    //     );
    //
    //     // 시스템 메시지인 경우 노란색 스타일 적용
    //     if (isSystemMessage)
    //     {
    //         chatListComponent.ApplySystemMessageStyle();
    //         Debug.Log($"[UI_IngameChatPopup] 시스템 메시지 표시: {messageInfo.Message}");
    //     }
    // }

    /// <summary>
    /// 스크롤을 맨 아래로 이동 (최신 메시지 보이도록)
    /// Coroutine으로 지연 처리하여 레이아웃이 완전히 업데이트된 후 스크롤
    /// </summary>
    private System.Collections.IEnumerator ScrollToBottomCoroutine()
    {
        if (ChatScrollRect == null) yield break;

        // 레이아웃이 갱신될 때까지 1프레임 대기 (가장 확실한 방법은 WaitForEndOfFrame)
        yield return new WaitForEndOfFrame();

        // 강제 업데이트 (혹시 모를 레이아웃 꼬임 방지)
        Canvas.ForceUpdateCanvases();

        // 스크롤 내리기
        ChatScrollRect.verticalNormalizedPosition = 0f;
        // [방어 코드] 한 번 더 확실하게 내리기 (복잡한 레이아웃에서 튕김 방지)
        yield return null; 
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
        StartCoroutine(ScrollToBottomCoroutine());

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

    /// <summary>
    /// 채팅 메시지 개수를 최대 50개로 제한
    /// </summary>
    private void LimitChatMessageCount()
    {
        if (ChatContent == null) return;

        // ChatContent의 자식 개수 확인
        int childCount = ChatContent.transform.childCount;

        // 50개 초과 시 가장 오래된 것부터 삭제
        while (childCount > MAX_CHAT_MESSAGES)
        {
            Transform oldestChild = ChatContent.transform.GetChild(0);
            Destroy(oldestChild.gameObject);
            childCount--;
        }
    }

    // ★ 플레이어 팀(색상) 변경 시 호출되는 콜백
    private void OnPlayerColorChangedWrapper(int actorNumber, EInGameTeam newTeam)
    {
        // ChatContent 산하의 모든 UIChatList를 찾아서 갱신
        if (ChatContent != null)
        {
            // ActorNumber로 닉네임 찾기
            string targetNickname = GetNicknameByActorNumber(actorNumber);

            if (string.IsNullOrEmpty(targetNickname))
            {
                Debug.LogWarning($"[UI_IngameChatPopup] ActorNumber {actorNumber}에 해당하는 플레이어를 찾을 수 없습니다.");
                return;
            }

            UIChatList[] chatLists = ChatContent.GetComponentsInChildren<UIChatList>();

            int updatedCount = 0;
            foreach (var chatItem in chatLists)
            {
                // ★ 닉네임으로 비교 (더 안전함)
                if (chatItem.GamerName == targetNickname)
                {
                    chatItem.UpdateTeamColor(newTeam);
                    updatedCount++;
                }
            }

            Debug.Log($"[UI_IngameChatPopup] {targetNickname}의 채팅 {updatedCount}개 색상을 {newTeam}으로 변경했습니다.");
        }
    }

    /// <summary>
    /// ActorNumber로 닉네임 찾기
    /// </summary>
    private string GetNicknameByActorNumber(int actorNumber)
    {
        if (PhotonNetwork.CurrentRoom == null) return string.Empty;

        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out PhotonPlayer player))
        {
            return player.NickName;
        }

        return string.Empty;
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
            inputFieldMini.OnEmptySubmit -= CloseByEnter;
        }

        if (inputFieldFull != null)
        {
            inputFieldFull.OnSubmit -= OnChatSubmit;
            inputFieldFull.OnEmptySubmit -= CloseByEnter;
        }

        // ★ 팀 컬러 변경 이벤트 구독 해제
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameStart -= OnGameStartWrapper;
            EventManager.Instance.OnGameOver -= OnGameOverWrapper;
            EventManager.Instance.OnPlayerColorChanged -= OnPlayerColorChangedWrapper;
        }
        Debug.Log("[UI_IngameChatPopup] Destroyed - 이벤트 구독 해제 완료");
    }
}
