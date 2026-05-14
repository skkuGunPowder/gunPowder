using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using MaskTransitions;

public class LoadingScene : MonoBehaviour

{
    // public List<RawImage> _loadingImages;
    [SerializeField] private Slider _progressBar; // 진행도 표시용 (옵션)
    [SerializeField] private float _minimumLoadTime = 3f; // 최소 로딩 시간
    [SerializeField] private float _transitionTime = 2f;
    private bool _isTransitioning = false;
    private int _currentImageIndex = 0;
    private float _imageChangeInterval = 0.5f;
    
    private bool _isSceneLoaded = false;
    
    public void Start()
    {
        _isTransitioning = false;
        Task_LoadNextScene().Forget();
    }
    
    private IEnumerator LoadNextScene_Coroutine()
    {
        float startTime = Time.time;
        
        AsyncOperation ao = SceneManager.LoadSceneAsync(TransitionManager.Instance.GetTargetScene());
        ao.allowSceneActivation = false; // 비동기로 로드되는 씬의 모습이 화면에 보이지 않게 한다.

        while (ao.isDone == false)
        {
            // 비동기로 실행할 코드를 입력한다.
            _progressBar.value = ao.progress; // => 0~1의 값을 나타낸다.
            
            if (ao.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;
                
                if (elapsedTime >= _transitionTime && !_isTransitioning)
                {
                    _isTransitioning = true;
                    TransitionManager.Instance.PlayTransition(3.3f);
                }
                else if (elapsedTime >= _minimumLoadTime)
                {
                    // ProgressText.text = "지옥의 문이 열렸다. 이제 네 차례야.";
                    ao.allowSceneActivation = true;
                }
                
            }
            yield return null;
        }
    }

    private async UniTaskVoid Task_LoadNextScene()
    {
        float startTime = Time.time;
        
        AsyncOperation ao = SceneManager.LoadSceneAsync(TransitionManager.Instance.GetTargetScene());
        ao.allowSceneActivation = false; // 비동기로 로드되는 씬의 모습이 화면에 보이지 않게 한다.

        while (ao.isDone == false)
        {
            // 비동기로 실행할 코드를 입력한다.
            _progressBar.value = ao.progress; // => 0~1의 값을 나타낸다.
            
            if (ao.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;
                
                if (elapsedTime >= _transitionTime && !_isTransitioning)
                {
                    _isTransitioning = true;
                    TransitionManager.Instance.PlayTransition(3.3f);
                }
                else if (elapsedTime >= _minimumLoadTime)
                {
                    // ProgressText.text = "지옥의 문이 열렸다. 이제 네 차례야.";
                    ao.allowSceneActivation = true;
                }
                
            }
            
            await UniTask.Yield();
        }
        
        
    }
}
