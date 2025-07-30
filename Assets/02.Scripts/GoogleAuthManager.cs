using Assets.SimpleSignIn.Google.Scripts;
using UnityEngine;

public class GoogleAuthManager : MonoBehaviour
{
    public GoogleAuthSettings GoogleAuthSettings;

    public void Awake()
    {
        EnvLoader.LoadEnv(Application.dataPath + "/.env");
        string clientId = EnvLoader.Get("WINDOW_CLIENT_ID");
        string clientSecret = EnvLoader.Get("WINDOW_CLIENT_SECRET");

        GoogleAuthSettings.SetWindowsCredentials(clientId, clientSecret);
        GoogleAuthSettings.SetDesktopCredentials(clientId, clientSecret);
    }
}
