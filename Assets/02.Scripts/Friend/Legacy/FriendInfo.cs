// 친구 정보 클래스
[System.Serializable]
public class FriendInfo
{
    public string nickname;
    public string inDate;

    public FriendInfo(string nickname, string inDate)
    {
        this.nickname = nickname;
        this.inDate = inDate;
    }
}
