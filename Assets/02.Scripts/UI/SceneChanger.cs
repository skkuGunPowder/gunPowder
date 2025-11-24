using System.Collections;
using UnityEngine;
using MaskTransitions;
public class SceneChanger : MonoBehaviour
{
    public ESceneList Scene;
    [SerializeField] private float _clickInterval = 0.5f;
    private bool _isClickInterval = false; // 쿨타임
    public void OnClickSceneChange()
    {
        if (_isClickInterval)
        {
            return;
        }
        
        _isClickInterval = true; // 즉시 쿨타임 활성화

        StartCoroutine(ClickInterval_Coroutine());
        
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
    
    private IEnumerator ClickInterval_Coroutine()
    {
        yield return new WaitForSeconds(_clickInterval);
        _isClickInterval = false;
    }

    private void OnDestroy()
    {
        StopCoroutine(ClickInterval_Coroutine());
    }
}
