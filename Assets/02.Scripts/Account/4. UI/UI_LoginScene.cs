using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

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
    public Toggle RememberLoginToggle;

    [Header("회원가입")]
    public UI_InputFields SignupInputFields;

    [Header("닉네임")]
    public UI_InputFields NicknameInputFields;

    public Button RegisterConfirmButton;

    private bool _isLoginCoolingDown;
    private bool _isNicknameCoolingDown;
    private bool _isRegisterCoolingDown;

	[Header("씬 전환")]
	public string LogoutRedirectSceneName = "Photon";

    private void Start()
    {
        OnClickGoToLoginButton();
        LoginCheck();
        // 자동 로그인 토글 초기화
        InitRememberToggle();

        // 구글 로그인 이벤트 구독
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.OnLoginResult += OnGoogleLoginResult;
            GoogleLogIn.Instance.OnLoginSuccess += OnGoogleLoginSuccess;
            GoogleLogIn.Instance.OnLoginError += OnGoogleLoginError;
        }

        if(STOVEManager.Instance != null)
        {
            STOVEManager.Instance.OnLoginResult += OnSTOVELoginResult;
            STOVEManager.Instance.OnLoginSuccess += OnSTOVELoginSuccess;
        }

        // 비밀번호 입력 필드와 회원가입 버튼 비활성화
        SignupInputFields.PasswordInputField.interactable = false;
        SignupInputFields.PasswordConfirmInputField.interactable = false;
        RegisterConfirmButton.interactable = false;
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

        if(STOVEManager.Instance != null)
        {
            STOVEManager.Instance.OnLoginResult = null;
            STOVEManager.Instance.OnLoginSuccess = null;
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
            //SignupInputFields.ResultText.text = "メールアドレスを入力してください。";
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
        if(result.IsSuccess)
        {
            SignupInputFields.ResultText.text = "이메일 인증이 완료되었습니다. 비밀번호를 설정해주세요.";
            //SignupInputFields.ResultText.text = "メール認証が完了しました。パスワードを設定してください。";
            SignupInputFields.PasswordInputField.interactable = true;
            SignupInputFields.PasswordConfirmInputField.interactable = true;
            RegisterConfirmButton.interactable = true;
        }
    }

    // 3. 최종 회원가입 (비밀번호 입력 후)
    public async void OnClickCompleteRegister()
    {
        if (_isRegisterCoolingDown)
        {
            return;
        }
        _isRegisterCoolingDown = true;
        if (RegisterConfirmButton != null)
        {
            RegisterConfirmButton.interactable = false;
        }

        string email = SignupInputFields.IDInputField.text;
        string password = SignupInputFields.PasswordInputField.text;
        string confirmPwd = SignupInputFields.PasswordConfirmInputField.text;

        if (password != confirmPwd)
        {
            SignupInputFields.ResultText.text = "비밀번호가 일치하지 않습니다.";
            //SignupInputFields.ResultText.text = "パスワードが一致しません。";
            SignupInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            Invoke(nameof(ResetRegisterCooldown), 1f);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SignupInputFields.ResultText.text = "비밀번호를 입력해주세요.";
            //SignupInputFields.ResultText.text = "パスワードを入力してください。";
            SignupInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            Invoke(nameof(ResetRegisterCooldown), 1f);
            return;
        }

        var result = await AccountManager.Instance.CompleteRegister(email, password);
        SignupInputFields.ResultText.text = result.Message;

        if (result.IsSuccess)
        {
            // 회원가입 성공 시 로그인 패널로 이동 (버튼 비활성화 유지)
            OnClickGoToLoginButton();
        }
        else
        {
            // 회원가입 실패 시 1초 후 버튼 다시 활성화
            Invoke(nameof(ResetRegisterCooldown), 1f);
        }
    }

    private void ResetRegisterCooldown()
    {
        _isRegisterCoolingDown = false;
        if (RegisterConfirmButton != null)
        {
            RegisterConfirmButton.interactable = true;
        }
    }

    // 로그인
    public async void Login()
    {
        if (_isLoginCoolingDown)
        {
            return;
        }
        _isLoginCoolingDown = true;
        if (LoginInputFields != null && LoginInputFields.ConfirmButton != null)
        {
            LoginInputFields.ConfirmButton.interactable = false;
        }

        string email = LoginInputFields.IDInputField.text;
        string password = LoginInputFields.PasswordInputField.text;

        if (string.IsNullOrEmpty(email))
        {
            LoginInputFields.ResultText.text = "이메일을 입력해주세요.";
            //LoginInputFields.ResultText.text = "メールアドレスを入力してください。";
            LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            Invoke(nameof(ResetLoginCooldown), 1f);
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            LoginInputFields.ResultText.text = "비밀번호를 입력해주세요.";
            //LoginInputFields.ResultText.text = "パスワードを入力してください。";
            LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            Invoke(nameof(ResetLoginCooldown), 1f);
            return;
        }

        var result = await AccountManager.Instance.TryLogin(email, password);
        LoginInputFields.ResultText.text = result.Message;
        if (result.IsSuccess)
        {
            // 닉네임 자동 저장
            if (!string.IsNullOrEmpty(email))
            {
                if(RememberLoginToggle.isOn)
                {
                    PlayerPrefs.SetString("SavedLoginEmail", email);
                }
                else
                {
                    PlayerPrefs.DeleteKey("SavedLoginEmail");
                }
                PlayerPrefs.Save();
            }
            // 닉네임이 없으면 닉네임 입력 패널 표시
            if (!AccountManager.Instance.HasNickname())
            {
                LoginPanel.SetActive(false);
                NicknamePanel.SetActive(true);
            }
            else
            {
                PhotonServerManager.Instance.Connect();
            }
        }
        else
        {
            // 로그인 실패 시 1초 후 버튼 다시 활성화
            Invoke(nameof(ResetLoginCooldown), 1f);
        }
    }

    private void ResetLoginCooldown()
    {
        _isLoginCoolingDown = false;
        if (LoginInputFields != null && LoginInputFields.ConfirmButton != null)
        {
            LoginInputFields.ConfirmButton.interactable = true;
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

    public void OnClickSTOVELogin()
    {
        if(STOVEManager.Instance == null)
        {
            Debug.LogError("STOVEManager가 초기화 되지 않았습니다.");
            return;
        }

        STOVEManager.Instance.STOVELogin();
    }

	private void InitRememberToggle()
	{
		if (RememberLoginToggle == null)
		{
			return;
		}

		bool defaultOn = true;
        Debug.Log("RememberLoginToggle : " + defaultOn);
        Debug.Log("RememberLoginToggle : " + PlayerPrefs.GetInt("RememberLoginToggle", defaultOn ? 1 : 0));
		int saved = PlayerPrefs.GetInt("RememberLoginToggle", defaultOn ? 1 : 0);
		bool isOn = saved == 1;
		Debug.Log("RememberLoginToggle : " + isOn);
		RememberLoginToggle.isOn = isOn;
	}

	public void OnRememberToggleChanged()
	{
        Debug.Log("RememberLoginToggle : " + RememberLoginToggle.isOn);
		PlayerPrefs.SetInt("RememberLoginToggle", RememberLoginToggle.isOn ? 1 : 0);
		PlayerPrefs.Save();
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
            LoginPanel.SetActive(false);
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
        if (_isNicknameCoolingDown)
        {
            return;
        }
        _isNicknameCoolingDown = true;
        if (NicknameInputFields != null && NicknameInputFields.ConfirmButton != null)
        {
            NicknameInputFields.ConfirmButton.interactable = false;
        }

        string nickname = NicknameInputFields.NicknameInputField.text;
        if (string.IsNullOrEmpty(nickname))
        {
            NicknameInputFields.ResultText.text = "닉네임을 입력해주세요.";
            //NicknameInputFields.ResultText.text = "ニックネームを入力してください。";
            NicknameInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
            Invoke(nameof(ResetNicknameCooldown), 1f);
            return;
        }

        var result = await AccountManager.Instance.SetNickname(nickname);
        NicknameInputFields.ResultText.text = result.Message;
        if (result.IsSuccess)
        {
            NicknamePanel.SetActive(false);
            PhotonServerManager.Instance.Connect(true);
        }
        else
        {
            // 닉네임 저장 실패 시 1초 후 버튼 다시 활성화
            Invoke(nameof(ResetNicknameCooldown), 1f);
        }
    }

    private void ResetNicknameCooldown()
    {
        _isNicknameCoolingDown = false;
        if (NicknameInputFields != null && NicknameInputFields.ConfirmButton != null)
        {
            NicknameInputFields.ConfirmButton.interactable = true;
        }
    }

    private void OnSTOVELoginResult(Result result)
    {
        LoginInputFields.ResultText.text = result.Message;
        if (!result.IsSuccess)
        {
            LoginInputFields.ResultText.transform.DOShakePosition(0.5f, 15);
        }
    }

    private void OnSTOVELoginSuccess()
    {
        if (!AccountManager.Instance.HasNickname())
        {
            LoginPanel.SetActive(false);
            NicknamePanel.SetActive(true);
        }
        else
        {
            PhotonServerManager.Instance.Connect();
        }
    }
}
