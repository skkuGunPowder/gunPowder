using Assets.SimpleSignIn.Google.Scripts;
using UnityEngine;
using System.IO;

public class GoogleAuthManager : MonoBehaviour
{
    public GoogleAuthSettings GoogleAuthSettings;

    public void Awake()
    {
        string envPath;
        
        #if UNITY_EDITOR
        // 에디터에서는 Assets 폴더의 .env 파일 사용
        envPath = Application.dataPath + "/.env";
        #else
        // 빌드에서는 실행 파일과 같은 폴더의 .env 파일 사용
        string executablePath = Application.dataPath;
        string buildDirectory = Path.GetDirectoryName(executablePath);
        envPath = Path.Combine(buildDirectory, ".env");
        #endif
        
        EnvLoader.LoadEnv(envPath);
        
        string clientId = EnvLoader.Get("WINDOW_CLIENT_ID");
        string clientSecret = EnvLoader.Get("WINDOW_CLIENT_SECRET");
        string customUriScheme = EnvLoader.Get("GENERIC_CUSTOM_URI_SCHEME");
        string genericClientId = EnvLoader.Get("GENERIC_CLIENT_ID");

        #if UNITY_EDITOR
        // 에디터에서는 Desktop 설정 사용
        GoogleAuthSettings.SetDesktopCredentials(clientId, clientSecret);
        #elif UNITY_STANDALONE_WIN
        // Windows 빌드에서는 Windows 설정 사용
        GoogleAuthSettings.SetWindowsCredentials(clientId, clientSecret);
        #else
        // 기타 플랫폼에서는 Generic 설정 사용
        GoogleAuthSettings.SetGenericCredentials(genericClientId, customUriScheme);
        #endif
    }
}
