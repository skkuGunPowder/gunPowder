using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class SceneChanger : MonoBehaviour
{
    public ESceneList Scene;

    public void OnClickSceneChange()
    {
        if (Scene == ESceneList.Tutorial)
        {
            Tutorial();
            return;
        }
        
        SceneManager.LoadScene(Scene.ToString());
    }

    public void Tutorial()
    {
        Debug.Log("OnclickTutorial");
        PhotonServerManager.Instance.TutorialMode();
    }
}
