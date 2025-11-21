using UnityEngine;
using MaskTransitions;
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
        
        TransitionManager.Instance.LoadLevel(Scene);
    }

    public void Tutorial()
    {
        Debug.Log("OnclickTutorial");
        PhotonServerManager.Instance.TutorialMode();
    }
}
