using System;
using UnityEngine;
using BackndChat; // MessageInfo 사용

public class ChatBubbleListener : MonoBehaviour
{
    [Header("Settings")]
    public Transform bubbleAnchor; // 말풍선 뜰 위치 (슬롯 머리 위 빈 오브젝트)
    public GameObject chatBubblePrefab; // ChatOutsidePrefab 연결

    private string _ownerNickname = ""; // 현재 이 슬롯 주인의 닉네임
    private GameObject _currentBubble;  // 현재 떠있는 말풍선

    private void Start()
    {
        // 씬이 시작되면 자동으로 채팅 매니저 구독
        if (UIChatManager.Instance != null)
            UIChatManager.Instance.OnChatMessageReceived += OnChatMessage;
    }

    private void OnDestroy()
    {
        // 씬이 바뀌거나 슬롯이 사라지면 구독 해제 (자동 관리)
        if (UIChatManager.Instance != null)
            UIChatManager.Instance.OnChatMessageReceived -= OnChatMessage;
    }

    // ★ UI_ProfileSlot에서 호출할 함수
    public void SetOwner(string nickname)
    {
        _ownerNickname = nickname;
    }

    private void OnChatMessage(MessageInfo info)
    {
        if (this.gameObject.activeInHierarchy == false)
        {
            return;
        }
        
        // 주인이 없거나, 보낸 사람이 주인이 아니면 무시
        if (string.IsNullOrEmpty(_ownerNickname) || info.GamerName != _ownerNickname)
            return;
            
        // 시스템 메시지 제외
        if(info.GamerName == "SYSTEM") return;

        ShowBubble(info.Message);
    }

    private void ShowBubble(string message)
    {
        // 이전 말풍선 삭제 (새 대사로 교체)
        if (_currentBubble != null) Destroy(_currentBubble);

        if (chatBubblePrefab != null && bubbleAnchor != null)
        {
            _currentBubble = Instantiate(chatBubblePrefab, bubbleAnchor);
            
            // 위치 초기화 (Anchor 기준 0,0,0)
            _currentBubble.transform.localPosition = Vector3.zero;
            _currentBubble.transform.localRotation = Quaternion.identity;
            _currentBubble.transform.localScale = Vector3.one;

            // 텍스트 세팅
            var bubble = _currentBubble.GetComponent<ChatBubble>();
            if (bubble != null) bubble.Setup(message);
        }
    }

    private void OnDisable()
    {
        if (_currentBubble != null) Destroy(_currentBubble);
    }
}