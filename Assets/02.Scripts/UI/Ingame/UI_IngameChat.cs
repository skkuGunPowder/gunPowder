using System.Collections.Generic;
using BackndChat;
using UnityEngine;
using UnityEngine.UI;

public class UI_IngameChat : UI_Popup
{
    public GameObject ChatContent = null;
    public InputField ChatInput = null;
    public Button SendButton = null;

    private void Start()
    {
        // UIChatManager의 채팅 메시지 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived += OnChatMessageReceived;
            Debug.Log("[UI_IngameChat] 채팅 메시지 이벤트 구독 완료");
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
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SendChatMessage();
                }
            });
        }
    }

    /// <summary>
    /// 채팅 메시지 수신 시 호출되는 콜백
    /// </summary>
    private void OnChatMessageReceived(MessageInfo messageInfo)
    {
        if (ChatContent == null) return;

        // 채팅 리스트 UI 생성
        GameObject chatList = Instantiate(Resources.Load<GameObject>("Prefabs/ChatList"), ChatContent.transform);

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

    private void SendChatMessage()
    {
        // 인게임 채팅 UI에서 검사할 부분은 내용이 비어있는가?
        if (ChatInput == null) return;
        if (ChatInput.text.Length == 0) return;

        string text = ChatInput.text;
        ChatInput.text = string.Empty;

        if (string.IsNullOrEmpty(text)) return;

        UIChatManager.Instance.SendChatMessage(text);
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (안전장치)
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessageReceived;
        }
    }
}
