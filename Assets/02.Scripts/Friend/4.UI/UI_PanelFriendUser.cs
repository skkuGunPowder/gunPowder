using TMPro;
using UnityEngine;

public class UI_PanelFriendUser : MonoBehaviour
{
    public TextMeshProUGUI NicknameText;
    public TextMeshProUGUI UidText;
    private string _nickname;
    private string _uid;

    public void Refresh(string displayName, string uid)
    {
        NicknameText.text = displayName;
        UidText.text = uid;
        _nickname = displayName;
        _uid = uid;
    }

    // 친구 요청 보내기 버튼 (닉네임 기반)
    public void OnRequestFriendSendButtonClicked()
    {
        FriendManager.Instance.RequestFriendByNickname(_nickname, (success, message) =>
        {
            if (success)
            {
                Debug.Log($"친구 요청 성공: {message}");
                // TODO: 성공 UI 표시
            }
            else
            {
                Debug.LogError($"친구 요청 실패: {message}");
                // TODO: 실패 UI 표시
            }
        });
    }
}
