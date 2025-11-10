using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BackndChat;

/// <summary>
/// 파티 채팅 패널
/// </summary>
public class UI_PartyChatPanel : MonoBehaviour
{
    [Header("Party Member List")]
    public Transform PartyMemberListParent;
    public GameObject PartyMemberSlotPrefab;
    public TextMeshProUGUI PartyMemberCountText;

    [Header("Chat")]
    public Transform ChatContentParent;
    public GameObject ChatMessagePrefab;
    public TMP_InputField ChatInputField;
    public Button SendButton;
    public ScrollRect ChatScrollRect;

    [Header("Party Control")]
    public Button LeavePartyButton;

    private string _currentPartyId;
    private bool _isInitialized = false;

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (SendButton != null)
            SendButton.onClick.AddListener(OnSendButtonClicked);

        if (LeavePartyButton != null)
            LeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);

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
            UIChatManager.Instance.OnPartyChatReceived += OnReceivePartyChatMessage;
        }

        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.OnPartyMemberChanged += OnPartyMemberChanged;
        }

        // 파티 정보 갱신
        RefreshPartyInfo();
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnPartyChatReceived -= OnReceivePartyChatMessage;
        }

        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.OnPartyMemberChanged -= OnPartyMemberChanged;
        }
    }

    /// <summary>
    /// 파티 채팅 패널 초기화
    /// </summary>
    public void Initialize(string partyId)
    {
        _currentPartyId = partyId;
        _isInitialized = true;

        // 채팅 기록 로드
        LoadChatHistory();

        // 파티원 목록 갱신
        RefreshPartyMemberList();

        Debug.Log($"[UI_PartyChatPanel] 초기화 완료: {partyId}");
    }

    /// <summary>
    /// 파티 정보 갱신
    /// </summary>
    private void RefreshPartyInfo()
    {
        if (PartyManager.Instance == null) return;

        _currentPartyId = PartyManager.Instance.CurrentPartyId;

        if (string.IsNullOrEmpty(_currentPartyId))
        {
            // 파티에 참여하지 않은 상태
            ClearChatMessages();
            ClearPartyMemberList();
            return;
        }

        // 파티원 목록 갱신
        RefreshPartyMemberList();

        // 채팅 기록 로드
        LoadChatHistory();
    }

    /// <summary>
    /// 파티원 목록 갱신
    /// </summary>
    private void RefreshPartyMemberList()
    {
        if (PartyManager.Instance == null) return;

        // 기존 목록 삭제
        ClearPartyMemberList();

        // 파티원 목록 가져오기
        string[] members = PartyManager.Instance.PartyMembers;

        // 파티원 수 표시
        if (PartyMemberCountText != null)
        {
            PartyMemberCountText.text = $"파티원 ({members.Length}명)";
        }

        // 파티원 슬롯 생성
        foreach (string memberName in members)
        {
            CreatePartyMemberSlot(memberName);
        }
    }

    /// <summary>
    /// 파티원 슬롯 생성
    /// </summary>
    private void CreatePartyMemberSlot(string memberName)
    {
        if (PartyMemberSlotPrefab == null || PartyMemberListParent == null) return;

        GameObject slotObj = Instantiate(PartyMemberSlotPrefab, PartyMemberListParent);

        // 파티원 이름 표시
        var nameText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = memberName;

            // 파티장 표시
            if (PartyManager.Instance.IsPartyLeader && memberName == GetMyNickname())
            {
                nameText.text += " (리더)";
            }
        }
    }

    /// <summary>
    /// 파티원 목록 클리어
    /// </summary>
    private void ClearPartyMemberList()
    {
        if (PartyMemberListParent == null) return;

        foreach (Transform child in PartyMemberListParent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 채팅 기록 로드
    /// </summary>
    private void LoadChatHistory()
    {
        if (string.IsNullOrEmpty(_currentPartyId)) return;
        if (UIChatManager.Instance == null) return;

        // 채팅 메시지 클리어
        ClearChatMessages();

        // Backend Chat에서 채팅 기록 가져오기
        string channelGroup = "party";
        string channelName = $"party_{_currentPartyId}";
        ulong channelNumber = (ulong)System.Math.Abs(_currentPartyId.GetHashCode());

        var messages = UIChatManager.Instance.GetChannelMessages(channelGroup, channelName, channelNumber);

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
    private void OnReceivePartyChatMessage(MessageInfo messageInfo)
    {
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
        if (string.IsNullOrEmpty(_currentPartyId))
        {
            Debug.LogWarning("[UI_PartyChatPanel] 파티에 참여하지 않았습니다.");
            return;
        }

        if (UIChatManager.Instance == null)
        {
            Debug.LogError("[UI_PartyChatPanel] UIChatManager가 없습니다.");
            return;
        }

        // UIChatManager를 통해 파티 채팅 전송
        UIChatManager.Instance.SendPartyMessage(_currentPartyId, text);
    }

    /// <summary>
    /// 파티 떠나기 버튼 클릭
    /// </summary>
    private void OnLeavePartyButtonClicked()
    {
        if (PartyManager.Instance == null) return;

        // 확인 팝업 (선택 사항)
        // TODO: 확인 팝업 구현

        // 파티 떠나기
        PartyManager.Instance.LeaveParty();

        // UI 갱신
        RefreshPartyInfo();
    }

    /// <summary>
    /// 파티원 변경 이벤트 핸들러
    /// </summary>
    private void OnPartyMemberChanged(HashSet<string> members)
    {
        RefreshPartyMemberList();
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
