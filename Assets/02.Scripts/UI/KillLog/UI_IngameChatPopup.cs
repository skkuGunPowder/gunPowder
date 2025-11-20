using System;
using System.Collections.Generic;
using BackndChat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_IngameChatPopup : UI_Popup
{
    public GameObject ChatContent = null;
    public InputField ChatInput = null;
    public Button SendButton = null;
    public GameObject ChatListPrefab;

    private Action _closeCallback;
    // 인게임 채팅이 동작할 씬 목록
    private readonly string[] _activeScenes = { "WaitingRoom", "Beach1", "Dock1", "Forest1" };

    private void Awake()
    {
        // 버튼 및 입력 필드 리스너 설정
        if (SendButton != null)
        {
            SendButton.onClick.AddListener(SendChatMessage);
        }
        if (ChatInput != null)
        {
            ChatInput.onEndEdit.AddListener((string text) =>
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SendChatMessage();
                }
            });
        }

        _closeCallback = Close;
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

        // 각 메시지를 UI에 추가
        foreach (MessageInfo messageInfo in messages)
        {
            CreateChatListUI(messageInfo);
        }

        Debug.Log($"[UI_IngameChatPopup] 기존 메시지 복원 완료: {messages.Count}개");
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

        // InputField에 포커스가 있으면 열지 않음 (중복 방지)
        if (ChatInput != null && ChatInput.isFocused) return false;

        InputHandler.BlockInput = true;
        FocusInputField();
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
    /// InputField에 포커스 설정
    /// </summary>
    private void FocusInputField()
    {
        if (ChatInput != null)
        {
            ChatInput.ActivateInputField();
            ChatInput.Select();
        }
    }

    /// <summary>
    /// 채팅 메시지 수신 시 호출되는 콜백
    /// </summary>
    private void OnChatMessageReceived(MessageInfo messageInfo)
    {
        Debug.Log($"[UI_IngameChatPopup] OnChatMessageReceived 콜백 호출: {messageInfo.GamerName} - {messageInfo.Message}");
        CreateChatListUI(messageInfo);
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

        if (ChatListPrefab == null)
        {
            Debug.LogError("[UI_IngameChatPopup] ChatListPrefab이 null입니다!");
            return;
        }

        // 채팅 리스트 UI 생성
        GameObject chatList = Instantiate(ChatListPrefab, ChatContent.transform);

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

        // 자신의 메시지인지 확인
        bool isMyMessage = false;
        if (AccountManager.Instance != null && AccountManager.Instance.CurrentAccount != null)
        {
            isMyMessage = messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname;
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

    private void SendChatMessage()
    {
        // 인게임 채팅 UI에서 검사할 부분은 내용이 비어있는가?
        if (ChatInput == null) return;
        if (ChatInput.text.Length == 0) return;

        string text = ChatInput.text;
        ChatInput.text = string.Empty;

        if (string.IsNullOrEmpty(text)) return;

        Debug.Log($"[UI_IngameChatPopup] 채팅 메시지 전송: {text}");

        // 채팅 메시지 전송
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.SendChatMessage(text);
            Debug.Log($"[UI_IngameChatPopup] UIChatManager.SendChatMessage 호출 완료");
        }
        else
        {
            Debug.LogError("[UI_IngameChatPopup] UIChatManager.Instance가 null입니다!");
        }

        // 전송 후에도 InputField에 포커스 유지 (연속 채팅 가능)
        FocusInputField();
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

    private void OnDestroy()
    {
        // 채팅 메시지 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft;
        }

        Debug.Log("[UI_IngameChatPopup] Destroyed - 이벤트 구독 해제 완료");
    }
}
