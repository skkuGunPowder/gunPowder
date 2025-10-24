using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic; // Added for HashSet
using System.Linq;
using System.Data.Common;
using Firebase.Firestore; // Added for Count() method

public class AccountRepository
{
    /// <summary>
    /// 이메일 중복 체크 (Firebase Auth와 Firestore 모두 확인)
    /// </summary>
    public async Task<bool> IsEmailExists(string email)
    {
        try
        {
            // 1. Firestore에서 이메일로 사용자 검색
            var usersRef = FirebaseManager.Instance.DB.Collection("users");
            var query = usersRef.WhereEqualTo("email", email);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                Debug.LogError($"이메일이 이미 존재합니다: {email}");
                return true;
            }

            // 2. Firebase Auth에서도 확인 (추가 안전장치)
            try
            {
                // Firebase Auth에서 이메일 존재 여부 확인
                var methods = await FirebaseManager.Instance.Auth.FetchProvidersForEmailAsync(email);
                if (methods.Count() > 0)
                {
                    Debug.LogError($"Firebase Auth에서 이메일이 이미 존재합니다: {email}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Firebase Auth 이메일 체크 중 오류: {e.Message}");
                // Firestore에서 이미 확인했으므로 계속 진행
            }

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"이메일 중복 체크 실패: {e.Message}");
            return false; // 오류 시 중복이 아닌 것으로 처리
        }
    }

    public async Task<bool> TryAddAccount(AccountDTO account)
    {
        try
        {
            // 1. 이메일 중복 체크
            if (await IsEmailExists(account.Login_ID))
            {
                Debug.LogError($"이미 존재하는 이메일입니다: {account.Login_ID}");
                return false;
            }

            // 2. Discriminator 할당
            string discriminator = await GenerateDiscriminatorForNickname(account.Nickname);

            // 3. Firebase Auth 계정 생성
            FirebaseUser user = (await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(account.Login_ID, account.Password_Hash)).User;
            UserProfile profile = new UserProfile { DisplayName = account.Nickname };
            await user.UpdateUserProfileAsync(profile);

            // 4. Firestore에 사용자 정보 저장
            await SaveUserInfoToFirestore(user, account, discriminator);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("회원가입 실패: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Firestore에 사용자 정보 저장 (공통 메서드)
    /// </summary>
    private async Task SaveUserInfoToFirestore(FirebaseUser user, AccountDTO account, string discriminator)
    {
        var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
        await userDoc.SetAsync(new
        {
            nickname = account.Nickname,
            discriminator = discriminator,
            email = account.Login_ID,
            emailVerified = user.IsEmailVerified,
            createdAt = DateTime.UtcNow,
            lastLogin = DateTime.UtcNow,
            authProvider = account.Auth_Provider.ToString()
        });
    }

    // Firestore에서 해당 닉네임의 모든 discriminator를 조회하고, 가장 작은 미사용 번호를 반환
    private async Task<string> GenerateDiscriminatorForNickname(string nickname)
    {
        var usersRef = FirebaseManager.Instance.DB.Collection("users");
        var query = usersRef.WhereEqualTo("nickname", nickname);
        var snapshot = await query.GetSnapshotAsync();
        var used = new HashSet<string>();
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

    public async Task<AccountDTO> GetAccount(AccountDTO account)
    {
        try
        {
            FirebaseUser user = (await FirebaseManager.Instance.Auth.SignInWithEmailAndPasswordAsync(account.Login_ID, account.Password_Hash)).User;

            // 세션 아이디생성;
            string sessionID = Guid.NewGuid().ToString();

            // Firestore에서 사용자 정보 가져오기
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);

            // 세션정보 저장
            await userDoc.SetAsync(new { activeSession = sessionID }, SetOptions.MergeAll);
            ListenForSessionChanges(user.UserId, sessionID);


            var snapshot = await userDoc.GetSnapshotAsync();

            string nickname = "";
            string discriminator = "";

            if (snapshot.Exists)
            {
                var data = snapshot.ToDictionary();
                nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "";
                discriminator = data.ContainsKey("discriminator") ? data["discriminator"].ToString() : "";
            }

            AccountDTO result = new AccountDTO(
                user.UserId,
                user.Email,
                account.Password_Hash,
                account.SALT,
                AuthProvider.Local,
                nickname,
                discriminator,
                user.IsEmailVerified,
                AccountFlags.None
            );
            return result;
        }
        catch (FirebaseException fe)
        {
            Debug.LogError("Firebase 로그인 오류: " + fe.Message);
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"예외 발생 - 타입: {e.GetType().Name}");
            Debug.LogError($"예외 메시지: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");

            Debug.LogError("예외 발생: " + e.Message);
            return null;
        }
    }

    // 이메일 인증 메일 발송 (임시 계정 생성)
    public async Task<bool> SendEmailVerification(string email, string password)
    {
        try
        {
            // 1. 이메일 중복 체크
            if (await IsEmailExists(email))
            {
                Debug.LogError($"이미 존재하는 이메일입니다: {email}");
                return false;
            }

            // 2. Firebase Auth 계정 생성
            FirebaseUser user = (await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password)).User;

            // 3. Firestore에 임시 사용자 정보 저장 (닉네임은 빈 문자열)
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            await userDoc.SetAsync(new
            {
                nickname = "",
                discriminator = "",
                email = email,
                emailVerified = false,
                createdAt = DateTime.UtcNow,
                lastLogin = DateTime.UtcNow,
                authProvider = "Local"
            });

            // 4. 인증 메일 발송
            await user.SendEmailVerificationAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("인증 메일 발송 실패: " + e.Message);
            return false;
        }
    }

    // 이메일 인증 여부 확인
    public async Task<bool> IsEmailVerified()
    {
        var user = FirebaseManager.Instance.Auth.CurrentUser;
        if (user == null) return false;
        await user.ReloadAsync();
        return user.IsEmailVerified;
    }

    // 비밀번호 업데이트
    public async Task<bool> UpdatePassword(string newPassword)
    {
        var user = FirebaseManager.Instance.Auth.CurrentUser;
        if (user == null) return false;
        try
        {
            await user.UpdatePasswordAsync(newPassword);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("비밀번호 업데이트 실패: " + e.Message);
            return false;
        }
    }

    // 닉네임 업데이트
    public async Task<bool> UpdateNickname(string nickname)
    {
        var user = FirebaseManager.Instance.Auth.CurrentUser;
        if (user == null) return false;
        try
        {
            // 1. Discriminator 생성
            string discriminator = await GenerateDiscriminatorForNickname(nickname);

            // 2. Firebase Auth 프로필 업데이트
            UserProfile profile = new UserProfile { DisplayName = nickname };
            await user.UpdateUserProfileAsync(profile);

            // 3. Firestore에 닉네임과 discriminator 업데이트
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            var updateData = new Dictionary<string, object>
            {
                { "nickname", nickname },
                { "discriminator", discriminator }
            };
            await userDoc.UpdateAsync(updateData);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("닉네임 업데이트 실패: " + e.Message);
            return false;
        }
    }

    private void ListenForSessionChanges(string uid, string currentSeccsionID)
    {
        FirebaseManager.Instance.DB.Collection("users").Document(uid).Listen(snapshot =>
        {
            if (snapshot.Exists && snapshot.TryGetValue("activeSession", out string activeSession))
            {
                if (activeSession != currentSeccsionID)
                {
                    Debug.LogWarning("다른 기기에서 로그인됨. 로그아웃");
                    FirebaseManager.Instance.Auth.SignOut();

                    ClientManager.Quit();
                }
            }
        });
    }

    public async Task<bool> DeleteAccount()
    {
        var user = FirebaseManager.Instance.Auth.CurrentUser;
        if (user == null) return false;
        try
        {
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            await userDoc.DeleteAsync();
            await user.DeleteAsync();

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("계정 삭제 실패: " + e.Message);
            return false;
        }
    }

}