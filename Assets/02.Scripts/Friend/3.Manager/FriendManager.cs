using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FriendManager : Singleton<FriendManager>
{
    private readonly FriendRepository _repository = new FriendRepository();

    // 친구 요청 보내기
    public async Task SendFriendRequest(string senderUid, string recipientUid)
    {
        var requests = await _repository.GetFriendRequestsAsync(recipientUid);
        await _repository.AddFriendRequestAsync(recipientUid, senderUid);
    }

    // 친구 요청 수락
    public async Task AcceptFriendRequest(string userUid, string requesterUid)
    {
        var requests = await _repository.GetFriendRequestsAsync(userUid);

        var currentFriends = await _repository.GetFriendsAsync(userUid);
        if (!currentFriends.Contains(requesterUid))
        {
            await _repository.AddFriendAsync(userUid, requesterUid);
        }

        await _repository.RemoveFriendRequestAsync(userUid, requesterUid);
    }

    // 친구 요청 거절
    public async Task DeclineFriendRequest(string userUid, string requesterUid)
    {
        var requests = await _repository.GetFriendRequestsAsync(userUid);
        await _repository.RemoveFriendRequestAsync(userUid, requesterUid);
    }

    // 친구 삭제
    public async Task RemoveFriend(string userUid, string friendUid)
    {
        var currentFriends = await _repository.GetFriendsAsync(userUid);
        if (currentFriends.Contains(friendUid))
        {
            await _repository.RemoveFriendAsync(userUid, friendUid);
        }
    }

    // 친구 목록 가져오기
    public async Task<List<string>> GetFriendUids(string userUid)
    {
        return await _repository.GetFriendsAsync(userUid);
    }

    // 친구 요청 목록 가져오기
    public async Task<List<string>> GetFriendRequests(string userUid)
    {
        return await _repository.GetFriendRequestsAsync(userUid);
    }
}
