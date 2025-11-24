using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSlot : MonoBehaviour
{
    [Header("Data")]
    public string gamerName; // 이 슬롯에 앉은 유저의 닉네임 (입장 시 할당됨)

    [Header("Settings")]
    [SerializeField] private Transform chatBubbleAnchor; // 말풍선이 생성될 위치 (슬롯 머리 위 빈 오브젝트)
    [SerializeField] private GameObject chatBubblePrefab; // ChatOutsidePrefab 연결

    private GameObject _currentBubble; // 현재 떠있는 말풍선 추적용

    // 유저가 슬롯에 들어올 때 호출해서 닉네임 세팅
    public void InitializeSlot(string nickname)
    {
        gamerName = nickname;
    }

    // 외부에서 채팅이 오면 이 함수를 호출
    public void ShowChatBubble(string message)
    {
        // 1. 이미 떠있는 말풍선이 있다면 즉시 삭제 (겟앰프드 스타일: 갱신)
        if (_currentBubble != null)
        {
            Destroy(_currentBubble);
        }

        // 2. 앵커 위치에 말풍선 생성
        _currentBubble = Instantiate(chatBubblePrefab, chatBubbleAnchor);
        
        // 3. 텍스트 세팅
        ChatBubble bubbleScript = _currentBubble.GetComponent<ChatBubble>();
        if (bubbleScript != null)
        {
            bubbleScript.Setup(message);
        }
    }
}