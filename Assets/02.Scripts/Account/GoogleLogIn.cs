using UnityEngine;
using System;
using System.Threading.Tasks;
using Firebase.Auth;
using System.Collections.Generic;
using Assets.SimpleSignIn.Google.Scripts;

public class GoogleLogIn : Singleton<GoogleLogIn>
{
    public GoogleAuth GoogleAuth;

    // 이벤트 - UI에서 구독하여 사용
    public event Action<string> OnLoginResult;
    public event Action<FirebaseUser, UserInfo> OnLoginSuccess;
    public event Action<string> OnLoginError;

    protected void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        GoogleAuth = new GoogleAuth();
    }

    /// <summary>
    /// 구글 로그인 시작
    /// </summary>
    public void SignIn()
    {
        GoogleAuth.SignIn(OnSignIn, caching: true);
    }

    /// <summary>
    /// 구글 로그아웃
    /// </summary>
    public void SignOut()
    {
        GoogleAuth.SignOut(revokeAccessToken: true);
        OnLoginResult?.Invoke("로그아웃 성공");
    }

    private async void OnSignIn(bool success, string error, UserInfo userInfo)
    {
        if (success)
        {
            OnLoginResult?.Invoke($"구글 로그인 성공 : {userInfo.name}");
            Debug.Log($"구글 로그인 성공: {userInfo.name} ({userInfo.email})");
            
            // Firebase Auth에 구글 계정으로 로그인
            await SignInToFirebaseWithGoogle(userInfo);
        }
        else
        {
            OnLoginResult?.Invoke($"구글 로그인 실패: {error}");
            OnLoginError?.Invoke(error);
            Debug.LogError($"구글 로그인 실패: {error}");
        }
    }

    /// <summary>
    /// SimpleSignIn으로 얻은 구글 정보로 Firebase Auth에 로그인
    /// </summary>
    private async Task SignInToFirebaseWithGoogle(UserInfo userInfo)
    {
        try
        {
            // 1. 구글 ID Token 획득
            GoogleAuth.GetTokenResponse(async (tokenSuccess, tokenError, tokenResponse) =>
            {
                if (tokenSuccess)
                {
                    Debug.Log("구글 ID Token 획득 성공");
                    
                    // 2. Firebase Auth에 구글 계정으로 로그인
                    await SignInToFirebaseWithGoogleToken(userInfo, tokenResponse.IdToken);
                }
                else
                {
                    Debug.LogError($"구글 ID Token 획득 실패: {tokenError}");
                    OnLoginResult?.Invoke($"토큰 획득 실패: {tokenError}");
                    OnLoginError?.Invoke(tokenError);
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase 구글 로그인 중 오류: {e.Message}");
            OnLoginResult?.Invoke($"Firebase 로그인 실패: {e.Message}");
            OnLoginError?.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 구글 ID Token으로 Firebase Auth에 로그인
    /// </summary>
    private async Task SignInToFirebaseWithGoogleToken(UserInfo userInfo, string idToken)
    {
        try
        {
            // Firebase Auth에 구글 계정으로 로그인
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
            var result = await FirebaseManager.Instance.Auth.SignInWithCredentialAsync(credential);
            FirebaseUser user = result;
            
            Debug.Log($"Firebase Auth 로그인 성공: {user.DisplayName} ({user.UserId})");
            
            // 3. Firestore에 사용자 정보 저장/업데이트 (닉네임은 빈 문자열로)
            await SaveUserInfoToFirestore(user, userInfo);
            
            // 4. AccountManager에 로그인 정보 설정
            await SetAccountInfo(user, userInfo);
            
            OnLoginResult?.Invoke($"Firebase 로그인 성공: {user.DisplayName}");
            OnLoginSuccess?.Invoke(user, userInfo);
            
        }
        catch (Firebase.FirebaseException fe)
        {
            Debug.LogError($"Firebase Auth 로그인 실패: {fe.Message}");
            
            // 기존 계정이 없는 경우 새로 생성
            if (fe.ErrorCode == (int)Firebase.Auth.AuthError.UserNotFound)
            {
                await CreateNewFirebaseAccount(userInfo);
            }
            else
            {
                OnLoginResult?.Invoke($"Firebase 로그인 실패: {fe.Message}");
                OnLoginError?.Invoke(fe.Message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase 로그인 중 예외: {e.Message}");
            OnLoginResult?.Invoke($"Firebase 로그인 실패: {e.Message}");
            OnLoginError?.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 새로운 Firebase 계정 생성
    /// </summary>
    private async Task CreateNewFirebaseAccount(UserInfo userInfo)
    {
        try
        {
            // 임시 비밀번호 생성 (구글 계정이므로 실제로는 사용되지 않음)
            string tempPassword = Guid.NewGuid().ToString("N");
            
            // Firebase Auth에 새 계정 생성
            FirebaseUser user = (await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(userInfo.email, tempPassword)).User;
            
            // 사용자 프로필 업데이트 (닉네임은 빈 문자열로)
            UserProfile profile = new UserProfile
            {
                DisplayName = "", // 닉네임을 빈 문자열로 설정
                PhotoUrl = new System.Uri(userInfo.picture)
            };
            await user.UpdateUserProfileAsync(profile);
            
            Debug.Log($"새 Firebase 계정 생성 성공: {user.UserId}");
            
            // Firestore에 사용자 정보 저장
            await SaveUserInfoToFirestore(user, userInfo);
            
            // AccountManager에 로그인 정보 설정
            await SetAccountInfo(user, userInfo);
            
            OnLoginResult?.Invoke($"새 계정 생성 및 로그인 성공");
            OnLoginSuccess?.Invoke(user, userInfo);
            
        }
        catch (Exception e)
        {
            Debug.LogError($"새 계정 생성 실패: {e.Message}");
            OnLoginResult?.Invoke($"계정 생성 실패: {e.Message}");
            OnLoginError?.Invoke(e.Message);
        }
    }

    /// <summary>
    /// Firestore에 사용자 정보 저장 (닉네임은 빈 문자열로)
    /// </summary>
    private async Task SaveUserInfoToFirestore(FirebaseUser user, UserInfo userInfo)
    {
        try
        {
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            
            // 기존 사용자 정보 확인
            var snapshot = await userDoc.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                // 기존 사용자 정보 업데이트 (닉네임은 변경하지 않음)
                var updateData = new Dictionary<string, object>
                {
                    { "email", userInfo.email },
                    { "emailVerified", userInfo.email_verified },
                    { "picture", userInfo.picture },
                    { "lastLogin", DateTime.UtcNow },
                    { "authProvider", "Google" }
                };
                await userDoc.UpdateAsync(updateData);
                Debug.Log("기존 사용자 정보 업데이트 완료");
            }
            else
            {
                // 새 사용자 정보 저장 (닉네임은 빈 문자열로)
                await userDoc.SetAsync(new
                {
                    nickname = "", // 닉네임을 빈 문자열로 설정
                    discriminator = "", // 닉네임이 없으므로 discriminator도 빈 문자열
                    email = userInfo.email,
                    emailVerified = userInfo.email_verified,
                    picture = userInfo.picture,
                    createdAt = DateTime.UtcNow,
                    lastLogin = DateTime.UtcNow,
                    authProvider = "Google"
                });
                Debug.Log("새 사용자 정보 저장 완료 (닉네임 없음)");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firestore 사용자 정보 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// AccountManager에 로그인 정보 설정
    /// </summary>
    private async Task SetAccountInfo(FirebaseUser user, UserInfo userInfo)
    {
        try
        {
            // Firestore에서 사용자 정보 가져오기
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            var snapshot = await userDoc.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                string nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "";
                string discriminator = data.ContainsKey("discriminator") ? data["discriminator"].ToString() : "";
                
                // AccountManager에 로그인 정보 설정
                var accountDTO = new AccountDTO(
                    user.UserId,
                    userInfo.email,
                    "", // 구글 계정이므로 비밀번호 없음
                    "", // 구글 계정이므로 SALT 없음
                    AuthProvider.Google,
                    nickname, // Firestore에서 가져온 닉네임 (빈 문자열일 수 있음)
                    discriminator,
                    userInfo.email_verified,
                    AccountFlags.None
                );
                
                // AccountManager에 로그인 정보 설정
                if (AccountManager.Instance != null)
                {
                    await AccountManager.Instance.SetCurrentAccount(accountDTO);
                }
                
                Debug.Log($"AccountManager 설정 완료: 닉네임='{nickname}'");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"AccountManager 설정 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 액세스 토큰 획득 (디버깅용)
    /// </summary>
    public void GetAccessToken()
    {
        GoogleAuth.GetTokenResponse(OnGetTokenResponse);
    }

    private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
    {
        if (success)
        {
            OnLoginResult?.Invoke($"Access token: {tokenResponse.AccessToken}");

            var jwt = new JWT(tokenResponse.IdToken);
            Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");
            jwt.ValidateSignature(GoogleAuth.ClientId, OnValidateSignature);
        }
        else
        {
            OnLoginResult?.Invoke($"토큰 획득 실패: {error}");
        }
    }

    private void OnValidateSignature(bool success, string error)
    {
        OnLoginResult?.Invoke(success ? "JWT signature validated" : error);
    }
}
