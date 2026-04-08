using System.Collections.Generic;

// UI에 전달되는 읽기전용 친구 정보.
// Friend Entity의 스냅샷으로, UI 계층에서는 이 DTO만 참조한다.
public class FriendDTO
{
    public readonly string Nickname;
    public readonly string InDate;
    public readonly string CreatedAt;
    public readonly string LastLogin;
    public readonly EFriendStatus Status;
    public readonly IReadOnlyList<string> OutfitIds;

    public FriendDTO(string nickname, string inDate, string createdAt, string lastLogin,
                     EFriendStatus status, List<string> outfitIds)
    {
        Nickname = nickname;
        InDate = inDate;
        CreatedAt = createdAt;
        LastLogin = lastLogin;
        Status = status;
        OutfitIds = outfitIds?.AsReadOnly();
    }
}
