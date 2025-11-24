using UnityEngine;
using System.Collections.Generic;
using BackndChat; // MessageInfo 사용을 위해

public class LobbyChatController : MonoBehaviour
{
    [Header("UI References")]
    // 4개의 슬롯을 인스펙터에서 연결하거나 Find로 찾아옵니다.
    [SerializeField] private LobbyPlayerSlot[] playerSlots; 

    private void Start()
    {
        // UIChatManager의 채팅 수신 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived += HandleChatMessage;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnChatMessageReceived -= HandleChatMessage;
        }
    }

    // 채팅 메시지가 왔을 때 호출됨
    private void HandleChatMessage(MessageInfo messageInfo)
    {
        string senderName = messageInfo.GamerName;
        string message = messageInfo.Message;

        // 4개의 슬롯을 순회하며 닉네임이 일치하는 슬롯을 찾음
        foreach (var slot in playerSlots)
        {
            // 슬롯이 활성화되어 있고, 닉네임이 일치한다면
            if (slot.gameObject.activeInHierarchy && slot.gamerName == senderName)
            {
                slot.ShowChatBubble(message);
                return; // 찾았으면 종료
            }
        }
    }
}