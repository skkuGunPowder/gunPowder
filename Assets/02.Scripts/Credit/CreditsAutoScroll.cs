using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class CreditsAutoScroll : MonoBehaviour
{
[Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;

    [Header("Timing")]
    [Tooltip("스크롤 시작 전 대기 시간 (초 단위)")]
    public float startDelay = 0.25f;

    [Tooltip("끝에 도달한 후 정지 유지 시간 (초 단위)")]
    public float endHoldTime = 0.8f;

    [Header("Movement")]
    [Tooltip("스크롤 속도 (픽셀/초 단위)")]
    public float scrollSpeed = 120f;

    [Tooltip("내용이 짧아도 최소 이동(px)을 강제로 부여")]
    public float minTravel = 0f;

    [Tooltip("timeScale=0 상황에서도 진행하려면 켜기")]
    public bool useUnscaledTime = false;

    [Header("Options")]
    [Tooltip("활성화될 때마다 자동으로 스크롤 시작")]
    public bool playOnEnable = true;

    private float _maxScrollY;
    private bool _isScrolling;
    private float _timer;

    private void Awake()
    {
        TryResolveReferences();
    }

    private void OnEnable()
    {
        // if (playOnEnable)
        //     StartCoroutine(BootstrapAndStart());
    }
    
    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        if (content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
        
        StartScroll();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isScrolling = false;
    }
    
    // private IEnumerator BootstrapAndStart()
    // {
    //     // 오토사이즈/레이아웃 반영 대기(1~2 프레임)
    //     yield return null;
    //     yield return null;
    //
    //     // 강제 리빌드 (동적 텍스트/콘텐츠 대비)
    //     Canvas.ForceUpdateCanvases();
    //     if (content != null)
    //         LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    //     Canvas.ForceUpdateCanvases();
    //
    //     StartScroll();
    // }

    private void TryResolveReferences()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (viewport == null && scrollRect != null) viewport = scrollRect.viewport;
        if (content == null && scrollRect != null) content = scrollRect.content;
    }

    private void StartScroll()
    {
        if (content == null || viewport == null) return;

        // 높이 재계산
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        _maxScrollY = Mathf.Max(contentHeight - viewportHeight, minTravel);

        // // 시작 위치 초기화 (맨 아래에서 시작하여 위로 이동)
        // Vector2 anchored = content.anchoredPosition;
        // anchored.y = 0f;
        // content.anchoredPosition = anchored;

        _timer = -startDelay;
        _isScrolling = true;

        // 스크롤할 내용이 없으면 즉시 종료 처리
        // if (_maxScrollY <= 0f)
        // {
        //     _isScrolling = false;
        //     StartCoroutine(HoldAtEnd());
        // }
    }

    private void Update()
    {
        if (InputHandler.GetKeyDown(KeyCode.Escape))
        {
            OnScrollFinished();
        }
        
        if (!_isScrolling || content == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        _timer += dt;
        if (_timer < 0f) return;
        
        float newY = content.anchoredPosition.y + scrollSpeed * dt;

        if (newY >= _maxScrollY)
        {
            newY = _maxScrollY;
            var anchored = content.anchoredPosition;
            anchored.y = newY;
            content.anchoredPosition = anchored;

            _isScrolling = false;
            HoldAtEnd().Forget();
            return;
        }

        var updated = content.anchoredPosition;
        updated.y = newY;
        content.anchoredPosition = updated;
    }

    private async UniTaskVoid HoldAtEnd()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = 0f;

        var ct = this.GetCancellationTokenOnDestroy();
        
        while (t < endHoldTime)
        {
            t += dt;
            await UniTask.Yield(cancellationToken: ct).SuppressCancellationThrow();
        }
     
        OnScrollFinished();
    }

    private void OnScrollFinished()
    {
        // TODO: 씬 전환/페이드 등
        PhotonNetwork.LoadLevel(ESceneList.Lobby.ToString());
    }
}
