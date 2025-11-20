using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파티 초대 팝업
/// UIChatManager의 OnPartyInviteReceived 이벤트로부터 생성됨
/// </summary>
public class UI_PartyInvitePopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI inviterNicknameText;
    public Button acceptButton;
    public Button declineButton;

    private string _partyId;
    private string _inviterNickname;

    private void Awake()
    {
        // 버튼 이벤트 등록
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAccept);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDecline);
    }

    /// <summary>
    /// 초대 정보 설정
    /// </summary>
    public void SetInviteInfo(string partyId, string inviterUid, string inviterNickname)
    {
        _partyId = partyId;
        _inviterNickname = inviterNickname;

        if (inviterNicknameText != null)
        {
            inviterNicknameText.text = $"{inviterNickname}님이 파티에 초대하셨습니다.";
        }

        Debug.Log($"[UI_PartyInvitePopup] 초대 팝업 생성: {inviterNickname} → {partyId}");
    }

    /// <summary>
    /// 수락 버튼 클릭
    /// </summary>
    private void OnAccept()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("[UI_PartyInvitePopup] PartyManager가 없습니다.");
            Destroy(gameObject);
            return;
        }

        // 파티 참여 (PartyManager → UIChatManager 위임)
        PartyManager.Instance.JoinParty(_partyId);

        Debug.Log($"[UI_PartyInvitePopup] 파티 초대 수락: {_partyId}");
        Destroy(gameObject);
    }

    /// <summary>
    /// 거절 버튼 클릭
    /// </summary>
    private void OnDecline()
    {
        Debug.Log($"[UI_PartyInvitePopup] 파티 초대 거절: {_partyId}");
        Destroy(gameObject);
    }
}