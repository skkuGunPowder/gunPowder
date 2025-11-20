using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RequestFriendslot : MonoBehaviour
{
    public TextMeshProUGUI SenderNickname;
    public Button AcceptButton;
    public Button DenyButton;
    private string _inDate; // UID 대신 inDate 사용

    public void Refresh(string nickname, string inDate)
    {
        SenderNickname.text = nickname;
        _inDate = inDate;
    }

    // 친구 요청 수락 (inDate 기반)
    public void OnClickAccept()
    {
        FriendManagerLegacy.Instance.AcceptFriendRequest(_inDate, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"친구 요청 수락 성공: {message}");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"친구 요청 수락 실패: {message}");
            }
        });
    }

    // 친구 요청 거절 (inDate 기반)
    public void OnClickDecline()
    {
        FriendManagerLegacy.Instance.DeclineFriendRequest(_inDate, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"친구 요청 거절 성공: {message}");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"친구 요청 거절 실패: {message}");
            }
        });
    }
}