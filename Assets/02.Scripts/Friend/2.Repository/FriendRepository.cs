using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public class FriendRepository
{
    private const string COLLECTION_NAME = "UserAccount";
    private const string FRIEND_REQUESTS = "FriendRequests";
    private const string FRIENDS = "Friends";
    private CollectionReference _userCollection => FirebaseManager.Instance.DB.Collection(COLLECTION_NAME);

    // 친구요청 목록 가져오기
    public async Task<List<string>> GetFriendRequestsAsync(string userUid)
    {
        var doc = await _userCollection.Document(userUid).GetSnapshotAsync();
        return doc.TryGetValue(FRIEND_REQUESTS, out List<string> list) ? list : new List<string>();
    }

    // 친구요청 보내기
    public async Task AddFriendRequestAsync(string recipientUid, string senderUid)
    {
        await _userCollection.Document(recipientUid)
            .UpdateAsync(FRIEND_REQUESTS, FieldValue.ArrayUnion(senderUid));
    }

    // 친구요청 삭제
    public async Task RemoveFriendRequestAsync(string recipientUid, string senderUid)
    {
        await _userCollection.Document(recipientUid)
            .UpdateAsync(FRIEND_REQUESTS, FieldValue.ArrayRemove(senderUid));
    }

    // 친구 목록 가져오기
    public async Task<List<string>> GetFriendsAsync(string userUid)
    {
        var doc = await _userCollection.Document(userUid).GetSnapshotAsync();
        return doc.TryGetValue(FRIENDS, out List<string> list) ? list : new List<string>();
    }

    // 친구 추가
    public async Task AddFriendAsync(string userUid, string friendUid)
    {
        WriteBatch batch = FirebaseManager.Instance.DB.StartBatch();
        var userDoc = _userCollection.Document(userUid);
        var friendDoc = _userCollection.Document(friendUid);

        batch.Update(userDoc, new Dictionary<string, object>
        {
            { FRIENDS, FieldValue.ArrayUnion(friendUid) }
        });

        batch.Update(friendDoc, new Dictionary<string, object>
        {
            { FRIENDS, FieldValue.ArrayUnion(userUid) }
        });

        await batch.CommitAsync();
    }

    // 친구 삭제
    public async Task RemoveFriendAsync(string userUid, string friendUid)
    {
        WriteBatch batch = FirebaseManager.Instance.DB.StartBatch();
        var userDoc = _userCollection.Document(userUid);
        var friendDoc = _userCollection.Document(friendUid);

        batch.Update(userDoc, new Dictionary<string, object>
        {
            { FRIENDS, FieldValue.ArrayRemove(friendUid) }
        });

        batch.Update(friendDoc, new Dictionary<string, object>
        {
            { FRIENDS, FieldValue.ArrayRemove(userUid) }
        });

        await batch.CommitAsync();
    }
}