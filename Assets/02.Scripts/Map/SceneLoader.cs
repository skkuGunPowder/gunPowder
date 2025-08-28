using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public ESceneList AdditiveScene;
    
    private void Awake()
    {
        SceneManager.LoadScene(AdditiveScene.ToString(), LoadSceneMode.Additive);
    }
}