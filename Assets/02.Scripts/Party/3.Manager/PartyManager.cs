using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 파티 관리 매니저 (리팩토링 버전)
/// Backend Chat(UIChatManager)에 위임하여 채팅 기능 사용
/// </summary>
public class PartyManager : DontDestroySingleton<PartyManager>
{
    // ==================== 이벤트 ====================
    public event Action<HashSet<string>> OnPartyMemberChanged;

    // ==================== 파티 상태 ====================
    private string _currentPartyId = null;
    private bool _isPartyLeader = false;
    private HashSet<string> _partyMembers = new HashSet<string>();

    // ==================== Properties ====================
    public bool IsPartyLeader => _isPartyLeader;
    public string CurrentPartyId => _currentPartyId;
    public int PartyMemberCount => _partyMembers.Count;
    public string[] PartyMembers => _partyMembers.ToArray();

    private void Awake()
    {
        // UIChatManager 이벤트 구독
        if (UIChatManager.Instance != null)
        {
            SubscribeToUIChatManager();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (UIChatManager.Instance != null)
        {
            UnsubscribeFromUIChatManager();
        }
    }

    private void SubscribeToUIChatManager()
    {
        // 채널 입장/퇴장 이벤트는 UIChatManager의 OnJoinChannelPlayer/OnLeaveChannelPlayer에서 처리됨
        // 여기서는 구독할 필요 없음 (채널 정보는 UIChatManager가 관리)
    }

    private void UnsubscribeFromUIChatManager()
    {
        // 구독 해제
    }

    // ==================== 파티 생성 ====================

    /// <summary>
    /// 새 파티 생성 (파티장이 됨)
    /// </summary>
    public void CreateParty(string partyId, uint maxCount = 8)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            Debug.LogError("[PartyManager] PartyId가 비어있습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(_currentPartyId))
        {
            Debug.LogWarning("[PartyManager] 이미 파티에 참여 중입니다. 먼저 떠나주세요.");
            return;
        }

        _currentPartyId = partyId;
        _isPartyLeader = true;
        _partyMembers.Clear();
        _partyMembers.Add(GetMyNickname());

        // UIChatManager에게 파티 채널 생성 요청
        UIChatManager.Instance.CreatePartyChannel(partyId, maxCount);

        Debug.Log($"[PartyManager] 파티 생성: {partyId} (리더: {GetMyNickname()})");
        OnPartyMemberChanged?.Invoke(_partyMembers);
    }

    // ==================== 파티 참여 ====================

    /// <summary>
    /// 기존 파티에 참여
    /// </summary>
    public void JoinParty(string partyId)
    {
        if (string.IsNullOrEmpty(partyId))
        {
            Debug.LogError("[PartyManager] PartyId가 비어있습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(_currentPartyId))
        {
            Debug.LogWarning("[PartyManager] 이미 파티에 참여 중입니다. 먼저 떠나주세요.");
            return;
        }

        _currentPartyId = partyId;
        _isPartyLeader = false;
        _partyMembers.Clear();
        _partyMembers.Add(GetMyNickname());

        // UIChatManager에게 파티 채널 입장 요청
        UIChatManager.Instance.JoinPartyChannel(partyId);

        Debug.Log($"[PartyManager] 파티 참여: {partyId}");
        OnPartyMemberChanged?.Invoke(_partyMembers);
    }

    // ==================== 파티 떠나기 ====================

    /// <summary>
    /// 현재 파티 떠나기
    /// </summary>
    public void LeaveParty()
    {
        if (string.IsNullOrEmpty(_currentPartyId))
        {
            Debug.LogWarning("[PartyManager] 참여 중인 파티가 없습니다.");
            return;
        }

        string leavingPartyId = _currentPartyId;

        // UIChatManager에게 파티 채널 퇴장 요청
        UIChatManager.Instance.LeavePartyChannel(leavingPartyId);

        // 상태 초기화
        _currentPartyId = null;
        _isPartyLeader = false;
        _partyMembers.Clear();

        Debug.Log($"[PartyManager] 파티 떠남: {leavingPartyId}");
        OnPartyMemberChanged?.Invoke(_partyMembers);
    }

    // ==================== 파티 초대 ====================

    /// <summary>
    /// 친구를 파티에 초대
    /// </summary>
    public void SendPartyInvite(string friendNickname)
    {
        if (!_isPartyLeader)
        {
            Debug.LogWarning("[PartyManager] 파티장만 초대할 수 있습니다.");
            return;
        }

        if (string.IsNullOrEmpty(_currentPartyId))
        {
            Debug.LogWarning("[PartyManager] 참여 중인 파티가 없습니다.");
            return;
        }

        // UIChatManager의 귓속말 기능 사용
        UIChatManager.Instance.SendPartyInvite(friendNickname, _currentPartyId);

        Debug.Log($"[PartyManager] 파티 초대 전송: {friendNickname} → {_currentPartyId}");
    }

    // ==================== 파티원 관리 ====================

    /// <summary>
    /// 파티원 추가 (채널 입장 시 호출)
    /// </summary>
    public void AddPartyMember(string nickname)
    {
        if (string.IsNullOrEmpty(_currentPartyId)) return;

        if (_partyMembers.Add(nickname))
        {
            Debug.Log($"[PartyManager] 파티원 추가: {nickname} (총 {_partyMembers.Count}명)");
            OnPartyMemberChanged?.Invoke(_partyMembers);
        }
    }

    /// <summary>
    /// 파티원 제거 (채널 퇴장 시 호출)
    /// </summary>
    public void RemovePartyMember(string nickname)
    {
        if (string.IsNullOrEmpty(_currentPartyId)) return;

        if (_partyMembers.Remove(nickname))
        {
            Debug.Log($"[PartyManager] 파티원 제거: {nickname} (총 {_partyMembers.Count}명)");
            OnPartyMemberChanged?.Invoke(_partyMembers);

            // 파티장이 떠난 경우, 첫 번째 멤버가 파티장이 됨
            if (!_isPartyLeader && _partyMembers.Count > 0 && _partyMembers.First() == GetMyNickname())
            {
                _isPartyLeader = true;
                Debug.Log($"[PartyManager] {GetMyNickname()}이(가) 새로운 파티장이 되었습니다.");
            }
        }
    }

    // ==================== 유틸리티 ====================

    private string GetMyNickname()
    {
        if (AccountManager.Instance?.CurrentAccount != null)
        {
            return AccountManager.Instance.CurrentAccount.Nickname;
        }
        return "Unknown";
    }

    /// <summary>
    /// 파티에 참여 중인지 확인
    /// </summary>
    public bool IsInParty()
    {
        return !string.IsNullOrEmpty(_currentPartyId);
    }
}
