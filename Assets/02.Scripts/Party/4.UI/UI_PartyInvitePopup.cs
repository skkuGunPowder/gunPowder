using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_PartyInvitePopup : MonoBehaviour
{
    public TextMeshProUGUI inviterNicknameText;
    public Button acceptButton;
    public Button declineButton;

    private string _partyName;
    private string _inviterUid;

    public void OnAccept()
    {
        // 파티 참여
        PartyManager.Instance.JoinParty(_partyName);
        Debug.Log($"파티 '{_partyName}' 초대를 수락했습니다.");
        Destroy(gameObject);
    }

    public void OnDecline()
    {
        Debug.Log($"파티 '{_partyName}' 초대를 거절했습니다.");
        Destroy(gameObject);
    }
    public void SetInviteInfo(string partyName,string sender,string inviterNickname)
    {
        Initialize(partyName, sender, inviterNickname);
    }
    public void Initialize(string partyName, string inviterUid, string inviterNickname)
    {
        _partyName = partyName;
        _inviterUid = inviterUid;
        inviterNicknameText.text = $"{inviterNickname}님이 파티에 초대하셨습니다.";

        acceptButton.onClick.AddListener(OnAccept);
        declineButton.onClick.AddListener(OnDecline);
    }
}