using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Firebase.Auth;
using Assets.SimpleSignIn.Google.Scripts;

public class UI_GoogleLogin : MonoBehaviour
{
    [Header("구글 로그인 UI")]
    public Button GoogleLoginButton;
    public Button GoogleLogoutButton;
    public TextMeshProUGUI ResultText;

    [Header("닉네임 설정 UI")]
    public GameObject NicknamePanel;
    public TMP_InputField NicknameInputField;
    public TextMeshProUGUI NicknameResultText;
    public Button SaveNicknameButton;
    public Button CancelNicknameButton;

    private FirebaseUser _pendingUser;
    private UserInfo _pendingUserInfo;
    private string _pendingDiscriminator;

    private void Start()
    {
        // 버튼 이벤트 연결
        if (GoogleLoginButton != null)
        {
            GoogleLoginButton.onClick.AddListener(OnGoogleLoginClick);
        }
        
        if (GoogleLogoutButton != null)
        {
            GoogleLogoutButton.onClick.AddListener(OnGoogleLogoutClick);
        }
        
        if (SaveNicknameButton != null)
        {
            SaveNicknameButton.onClick.AddListener(OnSaveNicknameClick);
        }
        
        if (CancelNicknameButton != null)
        {
            CancelNicknameButton.onClick.AddListener(OnCancelNicknameClick);
        }

        // GoogleLogIn 이벤트 구독
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.OnLoginResult += OnLoginResult;
            GoogleLogIn.Instance.OnLoginSuccess += OnLoginSuccess;
            GoogleLogIn.Instance.OnLoginError += OnLoginError;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.OnLoginResult -= OnLoginResult;
            GoogleLogIn.Instance.OnLoginSuccess -= OnLoginSuccess;
            GoogleLogIn.Instance.OnLoginError -= OnLoginError;
        }
    }

    /// <summary>
    /// 구글 로그인 버튼 클릭
    /// </summary>
    public void OnGoogleLoginClick()
    {
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.SignIn();
        }
    }

    /// <summary>
    /// 구글 로그아웃 버튼 클릭
    /// </summary>
    public void OnGoogleLogoutClick()
    {
        if (GoogleLogIn.Instance != null)
        {
            GoogleLogIn.Instance.SignOut();
        }
    }

    /// <summary>
    /// 로그인 결과 처리
    /// </summary>
    private void OnLoginResult(string message)
    {
        if (ResultText != null)
        {
            ResultText.text = message;
        }
    }

    /// <summary>
    /// 로그인 성공 처리
    /// </summary>
    private async void OnLoginSuccess(FirebaseUser user, UserInfo userInfo)
    {
        Debug.Log($"Google Login Success: {user.DisplayName}");
        
        // 닉네임 확인 및 설정
        await CheckAndSetupNickname(user, userInfo);
    }

    /// <summary>
    /// 로그인 에러 처리
    /// </summary>
    private void OnLoginError(string error)
    {
        Debug.LogError($"Google Login Error: {error}");
        if (ResultText != null)
        {
            ResultText.text = $"로그인 실패: {error}";
        }
    }

    /// <summary>
    /// 닉네임 확인 및 설정
    /// </summary>
    private async System.Threading.Tasks.Task CheckAndSetupNickname(FirebaseUser user, UserInfo userInfo)
    {
        try
        {
            // Firestore에서 사용자 정보 가져오기
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            var snapshot = await userDoc.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                string discriminator = data.ContainsKey("discriminator") ? data["discriminator"].ToString() : "0000";
                string nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "";
                
                // 닉네임이 설정되어 있지 않으면 닉네임 설정 UI 호출
                if (string.IsNullOrEmpty(nickname))
                {
                    Debug.LogWarning("닉네임이 설정되지 않음. 닉네임 설정 UI 호출");
                    ShowNicknameSetupUI(user, userInfo, discriminator);
                    return;
                }
                
                // 닉네임이 있으면 포톤 서버 연결
                ConnectToPhotonServer();
            }
            else
            {
                // 새 사용자의 경우 닉네임 설정 UI 호출
                Debug.LogWarning("새 사용자. 닉네임 설정 UI 호출");
                ShowNicknameSetupUI(user, userInfo, "");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"닉네임 확인 중 오류: {e.Message}");
            if (ResultText != null)
            {
                ResultText.text = $"닉네임 확인 실패: {e.Message}";
            }
        }
    }

    /// <summary>
    /// 닉네임 설정 UI 표시
    /// </summary>
    private void ShowNicknameSetupUI(FirebaseUser user, UserInfo userInfo, string discriminator)
    {
        _pendingUser = user;
        _pendingUserInfo = userInfo;
        _pendingDiscriminator = discriminator;
        
        // 닉네임 설정 패널 활성화
        if (NicknamePanel != null)
        {
            NicknamePanel.SetActive(true);
            
            // 기본값으로 구글 이름 설정
            if (NicknameInputField != null)
            {
                NicknameInputField.text = userInfo.name;
                NicknameInputField.Select();
            }
            
            if (NicknameResultText != null)
            {
                NicknameResultText.text = "닉네임을 설정해주세요.";
            }
        }
        else
        {
            Debug.LogError("NicknamePanel이 설정되지 않았습니다!");
            if (ResultText != null)
            {
                ResultText.text = "닉네임 설정 UI를 찾을 수 없습니다.";
            }
        }
    }

    /// <summary>
    /// 닉네임 저장 버튼 클릭
    /// </summary>
    public async void OnSaveNicknameClick()
    {
        if (NicknameInputField == null || string.IsNullOrEmpty(NicknameInputField.text))
        {
            if (NicknameResultText != null)
            {
                NicknameResultText.text = "닉네임을 입력해주세요.";
            }
            return;
        }

        string nickname = NicknameInputField.text.Trim();
        
        try
        {
            // Firestore에 닉네임 정보 저장
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(_pendingUser.UserId);
            
            // Discriminator가 없으면 새로 생성
            if (string.IsNullOrEmpty(_pendingDiscriminator))
            {
                _pendingDiscriminator = await GenerateDiscriminatorForNickname(nickname);
            }
            
            await userDoc.SetAsync(new
            {
                nickname = nickname,
                discriminator = _pendingDiscriminator,
                email = _pendingUserInfo.email,
                emailVerified = _pendingUserInfo.email_verified,
                picture = _pendingUserInfo.picture,
                createdAt = DateTime.UtcNow,
                lastLogin = DateTime.UtcNow,
                authProvider = "Google"
            });
            
            if (NicknameResultText != null)
            {
                NicknameResultText.text = $"닉네임 설정 완료: {nickname}#{_pendingDiscriminator}";
            }
            
            // 닉네임 설정 패널 비활성화
            if (NicknamePanel != null)
            {
                NicknamePanel.SetActive(false);
            }
            
            // 포톤 서버 연결
            ConnectToPhotonServer();
        }
        catch (Exception e)
        {
            Debug.LogError($"닉네임 설정 실패: {e.Message}");
            if (NicknameResultText != null)
            {
                NicknameResultText.text = $"닉네임 설정 실패: {e.Message}";
            }
        }
    }

    /// <summary>
    /// 닉네임 설정 취소 버튼 클릭
    /// </summary>
    public void OnCancelNicknameClick()
    {
        if (NicknamePanel != null)
        {
            NicknamePanel.SetActive(false);
        }
        
        if (NicknameResultText != null)
        {
            NicknameResultText.text = "닉네임 설정이 취소되었습니다.";
        }
    }

    /// <summary>
    /// 닉네임에 대한 Discriminator 생성
    /// </summary>
    private async System.Threading.Tasks.Task<string> GenerateDiscriminatorForNickname(string nickname)
    {
        var usersRef = FirebaseManager.Instance.DB.Collection("users");
        var query = usersRef.WhereEqualTo("nickname", nickname);
        var snapshot = await query.GetSnapshotAsync();
        
        var used = new System.Collections.Generic.HashSet<string>();
        foreach (var doc in snapshot.Documents)
        {
            if (doc.TryGetValue("discriminator", out object value) && value != null)
                used.Add(value.ToString());
        }
        
        for (int i = 0; i < 10000; i++)
        {
            string candidate = i.ToString("D4");
            if (!used.Contains(candidate))
                return candidate;
        }
        
        throw new Exception("해당 닉네임의 Discriminator가 모두 사용 중입니다.");
    }

    /// <summary>
    /// 포톤 서버 연결
    /// </summary>
    private void ConnectToPhotonServer()
    {
        if (PhotonServerManager.Instance != null)
        {
            PhotonServerManager.Instance.Connect();
        }
        else
        {
            Debug.LogWarning("PhotonServerManager를 찾을 수 없습니다.");
        }
    }
}
