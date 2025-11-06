using System.Collections.Generic;
using BackndChat;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_IngameChat : UI_Popup
{
    public static UI_IngameChat Instance { get; private set; }

    public GameObject ChatContent = null;
    public InputField ChatInput = null;
    public Button SendButton = null;
    public GameObject ChatListPrefab;

    // 인게임 채팅이 동작할 씬 목록 (대화내용 유지)
    private readonly string[] _activeScenes = { "WaitingRoom", "Beach1", "Dock1", "Forest1" };

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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

        // 씬 변경 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived; // 중복 방지
            UIChatManager.Instance.OnChatMessageReceived += OnChatMessageReceived;

            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft; // 중복 방지
            UIChatManager.Instance.OnChannelLeft += OnChannelLeft;
        }

        // 시작 시 현재 씬 체크
        CheckCurrentScene();
    }

    private void Update()
    {
        // 인게임 씬이 아니면 동작하지 않음
        if (!IsInGameScene()) return;

        // 채팅창이 열려있지 않고, InputField에 포커스가 없을 때 Enter로 열기
        if (!gameObject.activeSelf && ChatInput != null && !ChatInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OpenChatPopup();
            }
        }
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
    /// 씬 로드 시 호출
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }

    /// <summary>
    /// 현재 씬에 따라 UI 활성화/비활성화
    /// </summary>
    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (IsInGameScene())
        {
            // 인게임 씬이면 채팅 UI 표시 가능하게 설정 (하지만 Popup은 닫힌 상태)
            Close(); // UI_Popup의 Close() 호출
            Debug.Log($"[UI_IngameChat] 인게임 씬 진입: {currentScene}");
        }
        else
        {
            // 로비 등 다른 씬으로 이동 시 채팅 내용 삭제
            if (currentScene == "Lobby" || currentScene == "Photon" || currentScene == "StartSequence")
            {
                ClearAllMessages();
                Debug.Log($"[UI_IngameChat] 로비 씬 진입 - 채팅 내용 삭제: {currentScene}");
            }

            // 팝업 닫기
            Close();
        }
    }

    /// <summary>
    /// 채팅 Popup 열기
    /// </summary>
    private void OpenChatPopup()
    {
        Open(); // UI_Popup의 Open() 호출
        FocusInputField();
        Debug.Log("[UI_IngameChat] 채팅 Popup 열림");
    }

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
        if (ChatContent == null) return;

        // 채팅 리스트 UI 생성
        //GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);
        GameObject chatList = Instantiate(ChatListPrefab, ChatContent.transform);

        if (chatList == null)
        {
            Debug.LogError("[UI_IngameChat] ChatList 프리팹을 로드할 수 없습니다.");
            return;
        }

        UIChatList chatListComponent = chatList.GetComponent<UIChatList>();
        if (chatListComponent == null)
        {
            Debug.LogError("[UI_IngameChat] UIChatList 컴포넌트를 찾을 수 없습니다.");
            Destroy(chatList);
            return;
        }

        // 자신의 메시지인지 확인
        bool isMyMessage = messageInfo.GamerName == AccountManager.Instance.CurrentAccount.Nickname;

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

        // 채팅 메시지 전송
        UIChatManager.Instance.SendChatMessage(text);

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
        // 씬 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 채팅 메시지 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
            UIChatManager.Instance.OnChannelLeft -= OnChannelLeft;
        }
    }
}
