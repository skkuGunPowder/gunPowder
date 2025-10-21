using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InformationPopup : UI_Popup
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _emailText;
    [SerializeField] private TextMeshProUGUI _tagText;
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private InputField _nicknameInputField;

    private void Awake()
    {
        Refresh();
    }

    public void Refresh()
    {
        _emailText.text = $"{FirebaseManager.Instance.Auth.CurrentUser.Email}";
        _tagText.text = $"#{FirebaseManager.Instance.Auth.CurrentUser.UserId}";
        _nicknameText.text = $"{FirebaseManager.Instance.Auth.CurrentUser.DisplayName}";
    }

    public void OnClickPasswordChange()
    {
        AccountManager.Instance.ChangePassword();
    }

    public void OnClickDeleteAccount()
    {
        PopupManager.Instance.Open(EPopupType.UI_WithDrawPopup);
    }

    public void OnClickChangeNickname()
    {
        //TODO: 닉네임 변경 팝업 구현 필요
        // PopupManager.Instance.Open(EPopupType.UI_ChangeNicknamePopup);
    }

    public void OnClickMyInfo()
    {
        // TODO: 내 정보 팝업 구현 필요
        // UI_Manager.Instance.ShowPopupUI<UI_MyInfoPopup>();
    }

    public void OnClickPrivacyPolicy()
    {
        Application.OpenURL("http://storage.thebackend.io/af78c3be09a5c6bccf1701b1983b1277f8993519b7b3537b6c9676db3f2a0af4/privacy.html");

    }
    
    public void OnClickGameTerms()
    {
        Application.OpenURL("http://storage.thebackend.io/af78c3be09a5c6bccf1701b1983b1277f8993519b7b3537b6c9676db3f2a0af4/terms.html");
    }
}
