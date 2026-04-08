using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 개별 초대 슬롯: 게임/파티 초대 1건 표시
public class UI_InviteSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image inviteIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    [Header("아이콘")]
    [SerializeField] private Sprite gameInviteIcon;
    [SerializeField] private Sprite partyInviteIcon;

    private string _inviteData;
    private EInviteType _inviteType;
    private Action<UI_InviteSlot> _onRemoved;

    public void Init(string senderNickname, EInviteType inviteType, string inviteData, Action<UI_InviteSlot> onRemoved)
    {
        _inviteType = inviteType;
        _inviteData = inviteData;
        _onRemoved = onRemoved;

        nameText.text = senderNickname;

        switch (inviteType)
        {
            case EInviteType.Game:
                descText.text = "게임 초대를 보냈습니다.";
                if (inviteIcon != null && gameInviteIcon != null)
                    inviteIcon.sprite = gameInviteIcon;
                break;
            case EInviteType.Party:
                descText.text = "파티 초대를 보냈습니다.";
                if (inviteIcon != null && partyInviteIcon != null)
                    inviteIcon.sprite = partyInviteIcon;
                break;
        }

        acceptButton.onClick.AddListener(OnClickAccept);
        declineButton.onClick.AddListener(OnClickDecline);
    }

    private void OnClickAccept()
    {
        // TODO: PartyManager 연동 후 초대 수락 로직 구현
        // case EInviteType.Party: PartyManager.Instance.JoinParty(_inviteData);
        // case EInviteType.Game: 게임 초대 수락 로직
        Debug.Log($"[UI_InviteSlot] 초대 수락: {_inviteType}, data={_inviteData}");
        _onRemoved?.Invoke(this);
    }

    private void OnClickDecline()
    {
        _onRemoved?.Invoke(this);
    }
}
