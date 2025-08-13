using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : DontDestroySingleton<EventManager>
{
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
}
