using System.Collections;
using UnityEngine;
using System.Text;

using static Stove.PCSDK.Base;
// using static Stove.PCSDK.IAP;
using System;
using System.Threading.Tasks;


public class STOVEManager : MonoBehaviour
{
    private bool _isInitialized;

    private float _runCallbackInternval = 1.0f;
    private Coroutine _runCallbackCoroutine;

    private StovePCInitializeParam _initParam;

    private static STOVEManager _instance;
    private static object _lockObject = new object();

    public Action<Result> OnLoginResult;
    public Action OnLoginSuccess;


    public static STOVEManager Instance
    {
        get
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<STOVEManager>();

                    if (_instance == null)
                    {
                        _instance = new GameObject().AddComponent<STOVEManager>();
                        _instance.name = "STOVEPCSDK3Manager";
                    }
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _initParam = new StovePCInitializeParam
        {
            environment = "LIVE",
            gameId = EnvLoader.Get("GAME_ID"),
            applicationKey = EnvLoader.Get("APPLICATION_KEY")
        };

        // TODO : Deprecated 된거라 나중에 수정해야할 가능서 높음!
        bool restartAppIfNecessary = Base_RestartAppIfNecessary(_initParam);

        // TODO : 지금은 무한루프만 돌고 콜백이 안옴 -> Base_RestartAppIfNecessary가 완전히 Deprecated 되어서 지원안하면 바꿔야할 듯
        // Base_RestartAppIfNecessaryAsync(_initParam, 60_000, (CallbackResult callbackResult, bool restartAppIfNecessary) =>
        // {
        //     PrintCallbackResult(callbackResult);

            if (restartAppIfNecessary)
            {
                Debug.LogError("Please run the game through the stove launcher.");
            }
            else
            {
                Debug.Log("Success to run through stove launcher.");
                Initialize(EnvLoader.Get("SHOP_KEY"));
            }
        // });
    }

    private void OnDestroy()
    {
        if (_isInitialized)
        {
           UnInitialize();
        }
    }

    private IEnumerator RunCallbackCoroutine()
    {
        var wfs = new WaitForSeconds(_runCallbackInternval);

        while (true)
        {        		
            Base_RunCallback();

            yield return wfs;
        }
    }

    public void StartRunCallbackLoop()
    {
        if (_runCallbackCoroutine == null)
        {
            Debug.Log("Start RunCallbackLoop");

            _runCallbackCoroutine = StartCoroutine(RunCallbackCoroutine());
        }
    }

    public void StopRunCallbackLoop()
    {
        if (_runCallbackCoroutine != null)
        {
            Debug.Log("Stop RunCallbackLoop");

            StopCoroutine(_runCallbackCoroutine);
            _runCallbackCoroutine = null;
        }
    }

    public void PrintResult(Stove.PCSDK.Base.Result r)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# Result");
        sb.AppendLine($" - Result.sdkName : {r.sdkName}");
        sb.AppendLine($" - Result.methodCode : {r.methodCode}");
        sb.AppendLine($" - Result.resultCode : {r.resultCode}");
        sb.AppendLine($" - Result.exceptionMessage : {r.exceptionMessage}");

        Debug.Log(sb.ToString());
    }

    public void PrintCallbackResult(CallbackResult cr)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# CallbackResult");
        sb.AppendLine($" - CallbackResult.Result.sdkName : {cr.result.sdkName}");
        sb.AppendLine($" - CallbackResult.Result.methodCode : {cr.result.methodCode}");
        sb.AppendLine($" - CallbackResult.Result.resultCode : {cr.result.resultCode}");
        sb.AppendLine($" - CallbackResult.Result.exceptionMessage : {cr.result.exceptionMessage}");
        sb.AppendLine($" - CallbackResult.message : {cr.errorMessage}");
        sb.AppendLine($" - CallbackResult.externalError : {cr.externalError}");

        Debug.Log(sb.ToString());
    }

    public void Initialize(string shopKey)
    {
        StartRunCallbackLoop();

        Base_Initialize(_initParam, (CallbackResult callbackResult) =>
        {
            PrintCallbackResult(callbackResult);

            if (callbackResult.result.IsSuccessful())
            {
                Stove.PCSDK.Base.Result result = default;

                Debug.Log("Success to initialize Base SDK");
                // TODO
                // result = IAP_Initialize(shopKey);
                PrintResult(result);

                _isInitialized = true;
            }
            else
            {
                Debug.LogError("Fail to initialize Base SDK");
            }
        });
    }

    public void UnInitialize()
    {
        Stove.PCSDK.Base.Result result;

        StopRunCallbackLoop();

        result = Base_UnInitialize();
        PrintResult(result);

        // TODO
        // result = IAP_UnInitialize();
        // PrintResult(result);

        _isInitialized = false;
    }

    public void OnSTOVELogin()
    {
        STOVELogin();
    }

    public async void STOVELogin()
    {
        StovePCUser user = default;

        var result = Base_GetUser(ref user);
        if(result.IsSuccessful())
        {
            string id = $"{user.gameUserId}@stovelogin.com";
            string pw = $"{user.gameUserId}";

            var loginResult  = await AccountManager.Instance.TryLogin(id, pw);
            if (loginResult.IsSuccess)
            {
                Debug.Log("STOVE 로그인 성공");
                OnLoginResult?.Invoke(loginResult);
                OnLoginSuccess?.Invoke();
            }
            else
            {
                Debug.LogWarning($"STOVE 로그인 실패: {loginResult.Message}");
                STOVERegister(id, pw);
            }
        }
        else
        {
            Debug.LogWarning("STOVE 로그인 실패");
            OnLoginResult?.Invoke(new Result(false, "STOVE 로그인 실패"));
        }
    }

    private async void STOVERegister(string id, string pw)
    {
        string encryptedPassword = CryptoUtil.Encryption(pw, AccountManager.SALT);
        AccountDTO newAccountDTO = new AccountDTO(
            "",
            id,
            pw,
            AccountManager.SALT,
            AuthProvider.STOVE,
            "",
            "",
            false,
            AccountFlags.None
        );

        var fbRegisterResult = await AccountManager.Instance.TryRegister(newAccountDTO);
        var beRegisterResult = AccountManager.Instance._backendLogin.CustomSignUp(id, encryptedPassword);
        if(fbRegisterResult.IsSuccess)
        {
            Debug.Log("STOVE 회원가입 성공");
            var loginResult  = await AccountManager.Instance.TryLogin(id, pw);
            if (loginResult.IsSuccess)
            {
                Debug.Log("STOVE 로그인 성공");
                OnLoginResult?.Invoke(loginResult);
                OnLoginSuccess?.Invoke();
            }
            else
            {
                Debug.LogWarning($"STOVE 로그인 실패: {loginResult.Message}");
                OnLoginResult?.Invoke(loginResult);
            }
        }
        else
        {
            Debug.LogWarning($"STOVE Firebase: {fbRegisterResult.IsSuccess} | {fbRegisterResult.Message}");
            Debug.LogWarning($"STOVE Backend: {beRegisterResult.IsSuccess} | {beRegisterResult.Message}");
            OnLoginResult?.Invoke(fbRegisterResult);
        }
    }
}
