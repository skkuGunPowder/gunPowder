using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 초대 패널: 게임/파티 초대를 수신하여 표시
// 초대가 없으면 비활성, 초대가 들어오면 활성화
public class UI_InvitePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI inviteCountText;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject inviteSlotPrefab;

    private List<UI_InviteSlot> _activeSlots = new List<UI_InviteSlot>();

    // 초대 추가
    public void AddInvite(string senderNickname, EInviteType inviteType, string inviteData)
    {
        gameObject.SetActive(true);

        GameObject go = Instantiate(inviteSlotPrefab, slotParent);
        var slot = go.GetComponent<UI_InviteSlot>();
        slot.Init(senderNickname, inviteType, inviteData, OnSlotRemoved);
        _activeSlots.Add(slot);

        UpdateCountText();
    }

    private void OnSlotRemoved(UI_InviteSlot slot)
    {
        _activeSlots.Remove(slot);
        Destroy(slot.gameObject);
        UpdateCountText();

        if (_activeSlots.Count == 0)
            gameObject.SetActive(false);
    }

    private void UpdateCountText()
    {
        if (inviteCountText != null)
        {
            // 2개 이상일 때만 개수 표시
            inviteCountText.gameObject.SetActive(_activeSlots.Count >= 2);
            inviteCountText.text = _activeSlots.Count.ToString();
        }
    }

    public void ClearAll()
    {
        foreach (var slot in _activeSlots)
            Destroy(slot.gameObject);
        _activeSlots.Clear();
        gameObject.SetActive(false);
    }
}
