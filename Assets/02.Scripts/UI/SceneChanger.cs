using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public ESceneList Scene;

    public void OnClickSceneChange()
    {
        SceneManager.LoadScene(Scene.ToString());
    }
}
