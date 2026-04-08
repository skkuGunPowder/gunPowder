using UnityEngine;
using UnityEngine.UI;

// 파티 채팅 슬롯: 파티 참여 중일 때 표시, 클릭 시 파티 채팅창 팝업
public class UI_PartyChatSlot : MonoBehaviour
{
    [SerializeField] private Button chatButton;

    private void Awake()
    {
        if (chatButton != null)
            chatButton.onClick.AddListener(OnClickPartyChat);
    }

    private void OnClickPartyChat()
    {
        // TODO: PartyManager.Instance.IsInParty() 체크 후 파티 채팅 팝업 열기
        Debug.Log("[UI_PartyChatSlot] 파티 채팅 클릭");
    }

    // 파티 상태에 따라 슬롯 활성/비활성
    public void UpdateVisibility(bool isInParty)
    {
        gameObject.SetActive(isInParty);
    }
}
