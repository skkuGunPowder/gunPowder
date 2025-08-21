using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class AccountManager : DontDestroySingleton<AccountManager>
{
    private Account _myAccount;
    public AccountDTO CurrencAccount => _myAccount.ToDTO();

    private AccountRepository _accountRepository;
    private BackendLogin _backendLogin;
    private const string SALT = "12315";

    private string _sessoinID;


    protected override void Awake()
    {
        base.Awake();
        Init();
        DontDestroyOnLoad(gameObject);
    }

    private void Init()
    {
        _accountRepository = new AccountRepository();
        _backendLogin = new BackendLogin();
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
            Debug.LogError("로그인 실패");
            return new Result(false, "로그인에 실패하였습니다");
        }


        // 뒤끝 로그인
        _backendLogin.CustomLogin(loginId, encryptedPassword);

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
        return new Result(true, "로그인 성공!");
    }

    // 1. 이메일 인증 메일 발송 (임시 계정 생성)
    public async Task<Result> RequestEmailVerification(string email)
    {
        // 이메일 중복 체크
        if (await _accountRepository.IsEmailExists(email))
        {
            return new Result(false, "이미 존재하는 이메일입니다.");
        }

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
    public async Task<Result> CompleteRegister(string email, string password)
    {
        string encryptedPassword = CryptoUtil.Encryption(password, SALT);
        bool updated = await _accountRepository.UpdatePassword(encryptedPassword);
        if (!updated)
        {
            return new Result(false, "비밀번호 설정에 실패하였습니다.");
        }

        // 뒤끝 회원가입
        _backendLogin.CustomSignUp(email, encryptedPassword);

        return new Result(true, "회원가입이 완료되었습니다.");
    }

    // 닉네임 설정
    public async Task<Result> SetNickname(string nickname)
    {
        // 닉네임 중복 체크 및 discriminator 생성
        string discriminator = await GenerateDiscriminatorForNickname(nickname);
        
        bool updated = await _accountRepository.UpdateNickname(nickname);
        _myAccount.SetNickname(nickname, discriminator);

        // 뒤끝 닉네임 업데이트
        _backendLogin.UpdateNickName(nickname);
        
        if (updated)
            return new Result(true, "닉네임이 저장되었습니다.");
        else
            return new Result(false, "닉네임 저장에 실패하였습니다.");
    }

    /// <summary>
    /// 닉네임이 설정되어 있는지 확인
    /// </summary>
    public bool HasNickname()
    {
        return _myAccount != null && !string.IsNullOrEmpty(_myAccount.Nickname);
    }

    /// <summary>
    /// 닉네임에 대한 Discriminator 생성
    /// </summary>
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

    /// <summary>
    /// 구글 로그인 후 현재 계정 정보 설정
    /// </summary>
    public Result SetCurrentAccount(AccountDTO accountDTO)
    {
        try
        {
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

            return new Result(true, "구글 로그인 성공!");
        }
        catch (Exception e)
        {
            Debug.LogError($"구글 계정 정보 설정 실패: {e.Message}");
            return new Result(false, $"구글 계정 정보 설정 실패: {e.Message}");
        }
    }
}
