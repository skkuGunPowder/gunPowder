using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[Serializable]
public class UI_InputFields
{
    public TextMeshProUGUI ResultText;
    public TMP_InputField IDInputField;
    public TMP_InputField NicknameInputField;
    public TMP_InputField PasswordInputField;
    public TMP_InputField PasswordConfirmInputField;

    public Button ConfirmButton;
}

public class UI_LoginScene : MonoBehaviour
{
    [Header("패널")]
    public GameObject LoginPanel;
    public GameObject SignupPanel;
    public GameObject NicknamePanel;

    [Header("로그인")]
    public UI_InputFields LoginInputFields;

    [Header("회원가입")]
    public UI_InputFields SignupInputFields;

    [Header("닉네임")]
    public UI_InputFields NicknameInputFields;

    private void Start()
    {
        OnClickGoToLoginButton();
        LoginCheck();
        
        // 구글 로그인 이벤트 구독
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.OnLoginResult += OnGoogleLoginResult;
            GoogleLogIn.Instance.OnLoginSuccess += OnGoogleLoginSuccess;
            GoogleLogIn.Instance.OnLoginError += OnGoogleLoginError;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.OnLoginResult -= OnGoogleLoginResult;
            GoogleLogIn.Instance.OnLoginSuccess -= OnGoogleLoginSuccess;
            GoogleLogIn.Instance.OnLoginError -= OnGoogleLoginError;
        }
    }

    public void OnClickGoToSignupButton()
    {
        LoginPanel.SetActive(false);
        SignupPanel.SetActive(true);
    }

    public void OnClickGoToLoginButton()
    {
        LoginPanel.SetActive(true);
        SignupPanel.SetActive(false);
        NicknamePanel.SetActive(false);
    }

    public void LoginCheck()
    {
        string id = LoginInputFields.IDInputField.text;
        string password = LoginInputFields.PasswordInputField.text;
    }

    // 1. 인증 메일 발송
    public async void OnClickSendVerificationEmail()
    {
        string email = SignupInputFields.IDInputField.text;
        
        if (string.IsNullOrEmpty(email))
        {
            SignupInputFields.ResultText.text = "이메일을 입력해주세요.";
            return;
        }
        
        var result = await AccountManager.Instance.RequestEmailVerification(email);
        SignupInputFields.ResultText.text = result.Message;
        
        if (!result.IsSuccess)
        {
            SignupInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
        }
    }

    // 2. 인증 완료 확인
    public async void OnClickCheckEmailVerified()
    {
        var result = await AccountManager.Instance.CheckEmailVerified();
        SignupInputFields.ResultText.text = result.Message;
    }

    // 3. 최종 회원가입 (비밀번호 입력 후)
    public async void OnClickCompleteRegister()
    {
        string email = SignupInputFields.IDInputField.text;
        string password = SignupInputFields.PasswordInputField.text;
        string confirmPwd = SignupInputFields.PasswordConfirmInputField.text;

        if (password != confirmPwd)
        {
            SignupInputFields.ResultText.text = "비밀번호가 일치하지 않습니다.";
            return;
        }

        var result = await AccountManager.Instance.CompleteRegister(email, password);
        SignupInputFields.ResultText.text = result.Message;

        if (result.IsSuccess)
        {
            // 회원가입 성공 시 로그인 패널로 이동 등
            OnClickGoToLoginButton();
        }
    }

    // 로그인
    public async void Login()
    {
        string email = LoginInputFields.IDInputField.text;
        string password = LoginInputFields.PasswordInputField.text;

        if (string.IsNullOrEmpty(email))
        {
            LoginInputFields.ResultText.text = "이메일을 입력해주세요.";
            LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            LoginInputFields.ResultText.text = "비밀번호를 입력해주세요.";
            LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            return;
        }

        var result = await AccountManager.Instance.TryLogin(email, password);
        LoginInputFields.ResultText.text = result.Message;
        if (result.IsSuccess)
        {
            // 닉네임이 없으면 닉네임 입력 패널 표시
            if (!AccountManager.Instance.HasNickname())
            {
                NicknamePanel.SetActive(true);
            }
            else
            {
                PhotonServerManager.Instance.Connect();
            }
        }
    }

    // 구글 로그인 버튼에 연결
    public void OnClickGoogleLogin()
    {
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.SignIn();
        }
    }

    public void OnClickGoogleLogOut()
    {
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.SignOut();
        }
    }

    // 구글 로그인 결과 처리
    private void OnGoogleLoginResult(string message)
    {
        LoginInputFields.ResultText.text = message;
    }

    // 구글 로그인 성공 처리
    private void OnGoogleLoginSuccess(Firebase.Auth.FirebaseUser user, Assets.SimpleSignIn.Google.Scripts.UserInfo userInfo)
    {
        // 닉네임이 없으면 닉네임 입력 패널 표시
        if (!AccountManager.Instance.HasNickname())
        {
            NicknamePanel.SetActive(true);
        }
        else
        {
            PhotonServerManager.Instance.Connect();
        }
    }

    // 구글 로그인 에러 처리
    private void OnGoogleLoginError(string error)
    {
        LoginInputFields.ResultText.text = $"구글 로그인 실패: {error}";
        LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
    }

    // 닉네임 저장 버튼에 연결
    public async void OnClickSaveNickname()
    {
        string nickname = NicknameInputFields.NicknameInputField.text;
        if (string.IsNullOrEmpty(nickname))
        {
            NicknameInputFields.ResultText.text = "닉네임을 입력해주세요.";
            return;
        }
        var result = await AccountManager.Instance.SetNickname(nickname);
        NicknameInputFields.ResultText.text = result.Message;
        if (result.IsSuccess)
        {
            NicknamePanel.SetActive(false);
            PhotonServerManager.Instance.Connect();
        }
    }
}
