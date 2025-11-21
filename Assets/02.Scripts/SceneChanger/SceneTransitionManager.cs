using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : DontDestroySingleton<SceneTransitionManager>
{
    
    private const string LOADING_SCENE_NAME = "LoadingScene";
    
    private ESceneList _targetScene;
    
    // 로딩 후 추가 처리가 필요한지 여부
    private bool _waitingForPhotonRoom = false;

    
    public void LoadScene(ESceneList sceneName)
    {
        _targetScene = sceneName;
        
        // 로딩 씬으로 전환
        SceneManager.LoadScene(LOADING_SCENE_NAME);
    }

    /// <summary>
    /// Photon 방 입장 후 씬 로드 (OnJoinedRoom 대기)
    /// </summary>
    public void LoadSceneAfterJoinRoom(ESceneList sceneName)
    {
        _targetScene = sceneName;
        _waitingForPhotonRoom = true;
        
        SceneManager.LoadScene(LOADING_SCENE_NAME);
    }

    public void LoadLevel(ESceneList sceneName)
    {
        _targetScene = sceneName;
        
        PhotonNetwork.LoadLevel(LOADING_SCENE_NAME);
    }
    
    // 넘어갈 씬 받기
    public string GetTargetScene()
    {
        return _targetScene.ToString();
    }
    
    public bool IsWaitingForPhotonRoom()
    {
        return _waitingForPhotonRoom;
    }
}
