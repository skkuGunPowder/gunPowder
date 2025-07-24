using UnityEngine;

public class AccountDTO
{
    public readonly string Account_ID;
    public readonly string Login_ID;
    public readonly string Password_Hash;
    public readonly string SALT;
    public readonly AuthProvider Auth_Provider;
    public readonly string Nickname;
    public readonly string Discriminator;
    public readonly string User_Name;
    public readonly bool Email_Verified;
    public readonly AccountFlags Account_Flags;

    public AccountDTO(string accountId, string loginId, string passwordHash, string salt, AuthProvider authProvider, string nickname, string discriminator, bool emailVerified, AccountFlags accountFlags)
    {
        Account_ID = accountId;
        Login_ID = loginId;
        Password_Hash = passwordHash;
        SALT = salt;
        Auth_Provider = authProvider;
        Nickname = nickname;
        Discriminator = discriminator;
        User_Name = $"{nickname}#{discriminator}";
        Email_Verified = emailVerified;
        Account_Flags = accountFlags;
    }

    public AccountDTO(Account account)
    {
        Account_ID = account.Account_ID;
        Login_ID = account.Login_ID;
        Password_Hash = account.Password_Hash;
        SALT = account.SALT;
        Auth_Provider = account.Auth_Provider;
        Nickname = account.Nickname;
        Discriminator = account.Discriminator;
        User_Name = account.User_Name;
        Email_Verified = account.Email_Verified;
        Account_Flags = account.Account_Flags;
    }
}
