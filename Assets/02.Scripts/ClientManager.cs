using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : DontDestroySingleton<ClientManager>
{
    private const float PLAYTIME_UNIT = 3600f;
    private int _playHour = 0;
    private float _playTime = 0f;

    private void Start()
    {
        PlayBGM("Photon");
        ColorPalette.Init();
        _playHour = 0;
    }
    
    private void Update()
    {
        _playTime += Time.unscaledDeltaTime;
        if (_playTime >= PLAYTIME_UNIT)
        {
            _playHour++;
            _playTime = 0f;
            ToastMessageManager.Instance.Open(EToastType.UI_GameTimeScroll, $"{_playHour}");
        }        
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public static void GotoShop()
    {
        PhotonNetwork.LoadLevel("Shop");
    }

    public static void PlayBGM(string sceneName)
    {
        switch (sceneName)
        {
            case "Photon":
                SoundManager.Instance.PlayGlobalSound("WatingRoom1", SoundType.BGM, 0, true);
                break;

            case "Lobby":
                SoundManager.Instance.PlayGlobalSound("Lobby1", SoundType.BGM, 0, true);
                break;

            case "Shop":
                SoundManager.Instance.PlayGlobalSound("Shop1", SoundType.BGM, 0, true);
                break;

            case "WaitingRoom":
                SoundManager.Instance.PlayGlobalSound("WatingRoom2", SoundType.BGM, 0, true);
                break;

            case "Forest1":
                SoundManager.Instance.PlayGlobalSound("Forest1", SoundType.BGM, 0, true);
                break;

            case "Dock1":
                SoundManager.Instance.PlayGlobalSound("Dock1", SoundType.BGM, 0, true);
                break;

            case "Beach1":
                SoundManager.Instance.PlayGlobalSound("Beach1", SoundType.BGM, 0, true);
                break;

            case "Tutorial":
                SoundManager.Instance.PlayGlobalSound("Tutorial1", SoundType.BGM, 0, true);
                break;

            case "ResultScene":
                SoundManager.Instance.PlayGlobalSound("Victory_1");
                SoundManager.Instance.PlayGlobalSound("VictoryBGM_1", SoundType.BGM, 0, true);
                break;

            default:
                Debug.LogWarning($"{sceneName} 씬 BGM은 없습니다");
                break;
        }
    }

    
}
