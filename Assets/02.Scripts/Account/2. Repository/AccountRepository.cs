using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic; // Added for HashSet

public class AccountRepository
{
    public async Task<bool> TryAddAccount(AccountDTO account)
    {
        try
        {
            // 1. Discriminator 할당
            string discriminator = await GenerateDiscriminatorForNickname(account.Nickname);

            // 2. Firebase Auth 계정 생성
            FirebaseUser user = (await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(account.Login_ID, account.Password_Hash)).User;
            UserProfile profile = new UserProfile { DisplayName = account.Nickname };
            await user.UpdateUserProfileAsync(profile);

            // 3. Firestore에 사용자 정보 저장 (닉네임, discriminator)
            var userDoc = FirebaseManager.Instance.DB.Collection("users").Document(user.UserId);
            await userDoc.SetAsync(new {
                nickname = account.Nickname,
                discriminator = discriminator,
                loginId = account.Login_ID,
                emailVerified = user.IsEmailVerified
            });

            Debug.LogFormat("회원가입 성공: {0} ({1}) #{2}", user.DisplayName, user.UserId, discriminator);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("회원가입 실패: " + e.Message);
            return false;
        }
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
            AccountDTO result = new AccountDTO(
                user.UserId,
                user.Email,
                account.Password_Hash,
                account.SALT,
                AuthProvider.Local,
                user.DisplayName,
                "0000", // Discriminator 임시
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
            Debug.LogError("예외 발생: " + e.Message);
            return null;
        }
    }

    // 이메일 인증 메일 발송 (임시 계정 생성)
    public async Task<bool> SendEmailVerification(string email, string password)
    {
        try
        {
            FirebaseUser user = (await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password)).User;
            await user.SendEmailVerificationAsync();
            Debug.Log("인증 메일 발송 완료");
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
            Debug.Log("비밀번호 업데이트 성공");
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
            UserProfile profile = new UserProfile { DisplayName = nickname };
            await user.UpdateUserProfileAsync(profile);
            Debug.Log("닉네임 업데이트 성공");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("닉네임 업데이트 실패: " + e.Message);
            return false;
        }
    }
}