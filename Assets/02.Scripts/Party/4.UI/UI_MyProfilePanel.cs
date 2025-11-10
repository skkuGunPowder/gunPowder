using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// 내 프로필 정보 표시 패널
/// </summary>
public class UI_MyProfilePanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI MyNameText;
    public TextMeshProUGUI MyLevelText;
    public TextMeshProUGUI FriendCountText;
    public Image MyProfileImage;

    private void OnEnable()
    {
        LoadMyProfile();
    }

    /// <summary>
    /// 내 프로필 정보 로드
    /// </summary>
    private async void LoadMyProfile()
    {
        if (AccountManager.Instance?.CurrentAccount == null)
        {
            Debug.LogWarning("[UI_MyProfilePanel] 계정 정보가 없습니다.");
            return;
        }

        var account = AccountManager.Instance.CurrentAccount;

        // 닉네임 설정
        if (MyNameText != null)
        {
            MyNameText.text = account.Nickname;
        }

        // TODO: 레벨 설정
        // if (MyLevelText != null)
        // {
        //     MyLevelText.text = $"Lv. {account.Level}";
        // }

        // 친구 수 로드
        await UpdateFriendCount();

        // 프로필 이미지 설정 (TODO: 실제 이미지 로드)
        if (MyProfileImage != null)
        {
            // TODO: Firebase나 Resources에서 프로필 이미지 로드
            // MyProfileImage.sprite = ...
        }
    }

    /// <summary>
    /// 친구 수 업데이트
    /// </summary>
    public async Task UpdateFriendCount()
    {
        if (AccountManager.Instance?.CurrentAccount == null) return;

        string myUid = AccountManager.Instance.CurrentAccount.Account_ID;
        var friendUids = await FriendManager.Instance.GetFriendUids(myUid);

        if (FriendCountText != null)
        {
            FriendCountText.text = $"친구: {friendUids.Count}명";
        }
    }

    /// <summary>
    /// 외부에서 친구 수 갱신 요청
    /// </summary>
    public void RefreshFriendCount()
    {
        _ = UpdateFriendCount();
    }
}
