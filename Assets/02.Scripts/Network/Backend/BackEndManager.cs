using UnityEngine;
using BackEnd;

public class BackendManager : DontDestroySingleton<BackendManager>
{
#if DEV_MODE
    //Dev 폴더
    public const int FOLDER_ID = 3124;
#else
    //Build 폴더
    public const int FOLDER_ID = 3125;
#endif
    protected override void Awake()
    {
        base.Awake();
        
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (!bro.IsSuccess())
        {
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }
    }
}