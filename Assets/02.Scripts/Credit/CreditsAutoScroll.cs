using System.Collections;
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

    [Tooltip("스크롤 속도 (픽셀/초 단위)")]
    public float scrollSpeed = 120f;

    [Tooltip("끝에 도달한 후 정지 유지 시간 (초 단위)")]
    public float endHoldTime = 0.8f;

    [Header("Options")]
    [Tooltip("실행 시 자동으로 스크롤 시작")]
    public bool playOnStart = true;

    [Tooltip("진단 로그 출력")]
    public bool debugLogs = false;

    private float _maxScrollY;
    private bool _isScrolling;
    private float _timer;

    private void Awake()
    {
        TryResolveReferences();
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(BootstrapAndStart());
        }
    }

    private IEnumerator BootstrapAndStart()
    {
        // 한 프레임 기다려 오토사이즈/레이아웃 반영
        yield return null;

        //ForceRebuildLayouts();
        StartScroll();
    }

    private void TryResolveReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        if (viewport == null && scrollRect != null)
        {
            viewport = scrollRect.viewport;
        }

        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }
    }

    /*private void ForceRebuildLayouts()
    {
        Canvas.ForceUpdateCanvases();
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
        Canvas.ForceUpdateCanvases();
    }*/

    private void StartScroll()
    {
        if (scrollRect == null || content == null || viewport == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[CreditsAutoScroll] 참조가 비어 있습니다. ScrollRect/Content/Viewport를 확인하세요.", this);
            }
            return;
        }

        // 높이 재계산
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight > viewportHeight)
        {
            _maxScrollY = contentHeight - viewportHeight;
        }
        else
        {
            _maxScrollY = 0f;
        }

        // 시작 위치 초기화 (맨 아래에서 시작하여 위로 이동)
        Vector2 anchored = content.anchoredPosition;
        anchored.y = 0f;
        content.anchoredPosition = anchored;

        _timer = -startDelay;
        _isScrolling = true;

        if (debugLogs)
        {
            Debug.Log($"[CreditsAutoScroll] Start: contentH={contentHeight:F1}, viewH={viewportHeight:F1}, maxY={_maxScrollY:F1}", this);
        }

        // 스크롤할 내용이 없으면 즉시 종료 처리
        if (_maxScrollY <= 0f)
        {
            _isScrolling = false;
            StartCoroutine(HoldAtEnd());
        }
    }

    private void Update()
    {
        if (!_isScrolling)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < 0f)
        {
            return;
        }

        float newY = content.anchoredPosition.y + scrollSpeed * Time.deltaTime;

        if (newY >= _maxScrollY)
        {
            newY = _maxScrollY;

            Vector2 anchored = content.anchoredPosition;
            anchored.y = newY;
            content.anchoredPosition = anchored;

            _isScrolling = false;
            StartCoroutine(HoldAtEnd());
            return;
        }

        Vector2 updated = content.anchoredPosition;
        updated.y = newY;
        content.anchoredPosition = updated;
    }

    private IEnumerator HoldAtEnd()
    {
        if (endHoldTime > 0f)
        {
            yield return new WaitForSeconds(endHoldTime);
        }
        OnScrollFinished();
    }

    // 스크롤 완료 시 수행할 후처리(비워둠)
    private void OnScrollFinished()
    {
        // TODO: 필요 시 씬 전환/페이드 등 후처리
    }
}
