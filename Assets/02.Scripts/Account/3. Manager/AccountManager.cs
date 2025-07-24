using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using Firebase.Auth;

public class AccountManager : Singleton<AccountManager>
{
    private Account _myAccount;
    public AccountDTO CurrencAccount => _myAccount.ToDTO();

    private AccountRepository _accountRepository;
    private const string SALT = "12315";

    private void Awake()
    {
        Init();
        DontDestroyOnLoad(gameObject);
    }

    private void Init()
    {
        _accountRepository = new AccountRepository();
    }

    public async Task<Result> TryRegister(AccountDTO dto)
    {
        string encryptedPassword = CryptoUtil.Encryption(dto.Password_Hash, dto.SALT);
        AccountDTO account = new AccountDTO(
            "", // Account_ID는 회원가입 후 Firebase UID로 할당
            dto.Login_ID,
            encryptedPassword,
            dto.SALT,
            dto.Auth_Provider,
            dto.Nickname,
            dto.Discriminator,
            false,
            AccountFlags.None
        );

        AccountDTO accountDTO = await _accountRepository.GetAccount(account);
        if(accountDTO != null)
        {
            return new Result(false, "이미 가입한 이메일입니다.");
        }

        if(await _accountRepository.TryAddAccount(account))
        {
            Debug.Log("회원가입에 성공하였습니다.");
            return new Result(true, "회원가입에 성공하였습니다.");
        }
        else
        {
            return new Result(false, "회원가입에 실패하였습니다");
        }
    }

    public async Task<Result> TryLogin(string loginId, string password)
    {
        string encryptedPassword = CryptoUtil.Encryption(password, SALT);
        AccountDTO accountDTO = await _accountRepository.GetAccount(new AccountDTO(
            "",
            loginId,
            encryptedPassword,
            SALT,
            AuthProvider.Local,
            "",
            "",
            false,
            AccountFlags.None
        ));
        if (accountDTO == null)
        {
            Debug.Log("로그인 실패");
            return new Result(false, "로그인에 실패하였습니다");
        }
        _myAccount = new Account(
            accountDTO.Account_ID,
            accountDTO.Login_ID,
            accountDTO.Password_Hash,
            accountDTO.SALT,
            accountDTO.Auth_Provider,
            accountDTO.Nickname,
            accountDTO.Discriminator,
            accountDTO.Email_Verified,
            accountDTO.Account_Flags
        );
        Debug.Log("로그인 성공");
        Debug.Log($"[Account Info] Account_ID: {_myAccount.Account_ID}\n" +
                  $"Login_ID: {_myAccount.Login_ID}\n" +
                  $"Password_Hash: {_myAccount.Password_Hash}\n" +
                  $"SALT: {_myAccount.SALT}\n" +
                  $"Auth_Provider: {_myAccount.Auth_Provider}\n" +
                  $"Nickname: {_myAccount.Nickname}\n" +
                  $"Discriminator: {_myAccount.Discriminator}\n" +
                  $"User_Name: {_myAccount.User_Name}\n" +
                  $"Email_Verified: {_myAccount.Email_Verified}\n" +
                  $"Account_Flags: {_myAccount.Account_Flags}");
        return new Result(true, "로그인 성공!");
    }

    // 1. 이메일 인증 메일 발송 (임시 계정 생성)
    public async Task<Result> RequestEmailVerification(string email)
    {
        // 임시 비밀번호 생성 (보안상 실제 서비스에서는 더 복잡하게)
        string tempPassword = System.Guid.NewGuid().ToString().Substring(0, 8);
        bool sent = await _accountRepository.SendEmailVerification(email, tempPassword);
        if (sent)
            return new Result(true, "인증 메일이 발송되었습니다. 이메일을 확인하세요.");
        else
            return new Result(false, "인증 메일 발송에 실패하였습니다.");
    }

    // 2. 이메일 인증 여부 확인
    public async Task<Result> CheckEmailVerified()
    {
        bool verified = await _accountRepository.IsEmailVerified();
        if (verified)
            return new Result(true, "이메일 인증이 완료되었습니다.");
        else
            return new Result(false, "아직 이메일 인증이 완료되지 않았습니다.");
    }

    // 3. 비밀번호/닉네임 설정 및 회원가입 완료
    public async Task<Result> CompleteRegister(string password)
    {
        string encryptedPassword = CryptoUtil.Encryption(password, SALT);
        bool updated = await _accountRepository.UpdatePassword(encryptedPassword);
        if (!updated)
            return new Result(false, "비밀번호 설정에 실패하였습니다.");

        return new Result(true, "회원가입이 완료되었습니다.");
    }

    // 닉네임 설정
    public async Task<Result> SetNickname(string nickname)
    {
        bool updated = await _accountRepository.UpdateNickname(nickname);
        _myAccount.SetNickname(nickname);
        if (updated)
            return new Result(true, "닉네임이 저장되었습니다.");
        else
            return new Result(false, "닉네임 저장에 실패하였습니다.");
    }
}
