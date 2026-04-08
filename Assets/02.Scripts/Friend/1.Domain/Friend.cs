using System.Collections.Generic;

// 친구 도메인 Entity
// 친구 데이터의 명세, 제한사항, 도메인 규칙을 정의한다.
// 기능 로직은 Repository/Manager에서 구현한다.
public class Friend
{
    // === 상수 ===
    public const int MAX_FRIEND_COUNT = 50;
    public const int DEFAULT_QUERY_LIMIT = 100;

    // === 데이터 필드 ===
    public string Nickname { get; private set; }
    public string InDate { get; private set; }
    public string CreatedAt { get; private set; }
    public string LastLogin { get; private set; }
    public EFriendStatus Status { get; private set; }
    public List<string> OutfitIds { get; private set; }

    // === 생성자 ===
    public Friend(string nickname, string inDate, string createdAt, string lastLogin)
    {
        Nickname = nickname ?? "";
        InDate = inDate;
        CreatedAt = createdAt;
        LastLogin = lastLogin;
        Status = EFriendStatus.Offline;
        OutfitIds = new List<string>();
    }

    // === 도메인 함수 ===

    // 온라인 상태 갱신 (Firebase에서 조회한 값 반영)
    public void UpdateStatus(EFriendStatus newStatus)
    {
        Status = newStatus;
    }

    // 프로필 아웃핏 갱신 (Firebase에서 조회한 값 반영)
    public void UpdateOutfitIds(List<string> outfitIds)
    {
        OutfitIds = outfitIds ?? new List<string>();
    }

    // "닉네임#태그" 형태의 표시명 반환
    // (Discriminator는 AccountManager에서 별도 조회 필요)
    public string GetDisplayName(string discriminator = null)
    {
        if (string.IsNullOrEmpty(discriminator))
            return Nickname;
        return $"{Nickname}#{discriminator}";
    }

    // 유효한 친구 데이터인지 검증
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(InDate);
    }

    // 읽기전용 DTO로 변환
    public FriendDTO ToDTO()
    {
        return new FriendDTO(Nickname, InDate, CreatedAt, LastLogin, Status, OutfitIds);
    }

    // === 정적 유틸리티 ===

    // 주어진 친구 수가 최대치에 도달했는지 검사
    public static bool IsFriendListFull(int currentCount)
    {
        return currentCount >= MAX_FRIEND_COUNT;
    }
}
