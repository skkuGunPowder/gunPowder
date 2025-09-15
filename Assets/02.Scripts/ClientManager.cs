using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : DontDestroySingleton<ClientManager>
{
    private void Start()
    {
        PlayBGM("Photon");
        ColorPalette.Init();
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
