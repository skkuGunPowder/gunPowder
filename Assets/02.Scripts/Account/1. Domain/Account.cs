using UnityEngine;
using System;
using System.Text.RegularExpressions;
using Unity.Collections;

public enum AuthProvider
{
    Local,
    Google,
    Steam
}

public enum AccountFlags
{
    None = 0,
    Banned = 1 << 0,
    Restricted = 1 << 1,
    // ... 기타 상태 추가 가능
}

public class Account
{
    public readonly string Account_ID; // 내부 고유 식별자 (UUID)
    public readonly string Login_ID;   // 이메일 또는 사용자 ID
    public readonly string Password_Hash; // 솔팅된 해시 비밀번호
    public readonly string SALT;       // 해시 계산용 salt 값
    public readonly AuthProvider Auth_Provider; // local/google/steam
    public string Nickname { get; private set; } // 사용자 닉네임 (중복 허용)
    public string Discriminator { get; private set; } // 닉네임 식별용 4자리 숫자
    public string User_Name => $"{Nickname}#{Discriminator}"; // 닉네임+식별자
    public bool Email_Verified { get; private set; } // 이메일 인증 여부
    public AccountFlags Account_Flags { get; private set; } // 계정 상태

    // 생성자
    public Account(string accountId, string loginId, string passwordHash, string salt, AuthProvider provider, string nickname, string discriminator = null, bool emailVerified = false, AccountFlags flags = AccountFlags.None)
    {
        if (string.IsNullOrEmpty(accountId))
            throw new ArgumentException("Account_ID(Firebase UID)는 반드시 지정되어야 합니다.");
        Account_ID = accountId;
        Login_ID = loginId;
        Password_Hash = passwordHash;
        SALT = salt;
        Auth_Provider = provider;
        Nickname = nickname;
        Discriminator = string.IsNullOrEmpty(discriminator) ? GenerateDiscriminator() : discriminator;
        Email_Verified = emailVerified;
        Account_Flags = flags;
    }

    // Discriminator 4자리 숫자 생성 (중복 체크는 외부에서)
    private string GenerateDiscriminator()
    {
        var rand = new System.Random();
        return rand.Next(0, 10000).ToString("D4");
    }

    // 닉네임 변경
    public void SetNickname(string newNickname, string newDiscriminator = null)
    {
        Nickname = newNickname;
        if (!string.IsNullOrEmpty(newDiscriminator))
            Discriminator = newDiscriminator;
    }

    // 이메일 인증 상태 변경
    public void SetEmailVerified(bool verified)
    {
        Email_Verified = verified;
    }

    // 계정 상태 플래그 변경
    public void SetAccountFlags(AccountFlags flags)
    {
        Account_Flags = flags;
    }

    // DTO 변환 (필요시 확장)
    public AccountDTO ToDTO()
    {
        return new AccountDTO(Account_ID, Login_ID, Password_Hash, SALT, Auth_Provider, Nickname, Discriminator, Email_Verified, Account_Flags);
    }
}