using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : DontDestroySingleton<ClientManager>
{
    protected override void Awake()
    {
        base.Awake();

        PlayBGM("Photon");
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
        SceneManager.LoadScene("SShopUIDev");
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

            case "Map1":
                SoundManager.Instance.PlayGlobalSound("Forest1", SoundType.BGM, 0, true);
                break;

            case "Map2":
                SoundManager.Instance.PlayGlobalSound("Dock1", SoundType.BGM, 0, true);
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
