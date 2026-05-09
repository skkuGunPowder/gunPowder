using System;
using TMPro;
using UnityEngine;

public class UI_InformationPopup : UI_Popup
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _emailText;
    [SerializeField] private TextMeshProUGUI _tagText;
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TMP_InputField _nicknameInputField;

    private const int NicknameChangeCost = 50;


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

        UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
        //messagePopup.Init("이메일로 비밀번호 변경 메일을 보냈습니다.", false);
        messagePopup.Init("TX0115", false);
    }

    public void OnClickDeleteAccount()
    {
        PopupManager.Instance.Open(EPopupType.UI_WithdrawPopup);
    }

    public void OnClickChangeNickname()
    {
        UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
        //messagePopup.Init($"닉네임을 변경하시겠습니까? ({NicknameChangeCost} 다이아파우더 소모).", true, OnSetNickname);
        messagePopup.Init($"TX0116 ({NicknameChangeCost} TX0117).", true, OnSetNickname);
    }

    public void OnClickMyInfo()
    {
        // TODO: 내 정보 팝업 구현 필요
        UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup);
        //messagePopup.Init("내 정보 기능은 현재 지원하지 않습니다.", false);
        messagePopup.Init("TX0118", false);
    }

    public void OnClickPrivacyPolicy()
    {
        Application.OpenURL("http://storage.thebackend.io/af78c3be09a5c6bccf1701b1983b1277f8993519b7b3537b6c9676db3f2a0af4/privacy.html");

    }

    public void OnClickGameTerms()
    {
        Application.OpenURL("http://storage.thebackend.io/af78c3be09a5c6bccf1701b1983b1277f8993519b7b3537b6c9676db3f2a0af4/terms.html");
    }

    public async void OnSetNickname()
    {
        Result result = await AccountManager.Instance.SetNickname(_nicknameInputField.text);
        if (result.IsSuccess == true)
        {
            UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup, null);
            //messagePopup.Init("닉네임이 변경되었습니다.", false);
            messagePopup.Init("TX0119", false);
            Refresh();
        }
        else
        {
            UI_MessagePopup messagePopup = (UI_MessagePopup)PopupManager.Instance.Open(EPopupType.UI_MessagePopup, null);
            //messagePopup.Init($"닉네임 변경에 실패했습니다: {result.Message}", false);
            messagePopup.Init($"TX0120 : {result.Message}", false);
            return;
        }

        CurrencyManager.Instance.SubtractCurrency(ECurrencyType.Diamond, NicknameChangeCost);
    }
}
