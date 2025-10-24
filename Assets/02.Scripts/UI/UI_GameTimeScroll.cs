using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameTimeScroll : UI_Toast
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform viewport;      // 뷰포트
    public RectTransform targetContent; // 흘러갈 실제 오브젝트(텍스트/패널)
    public TextMeshProUGUI messageText;

    [Header("Timing")]
    [Tooltip("스크롤 시작 전 대기 시간(초)")]
    public float startDelay = 0.0f;

    [Tooltip("스크롤 종료 후 대기 시간(초)")]
    public float endHoldTime = 0.6f;

    [Header("Movement")]
    [Tooltip("오른쪽 → 왼쪽 스크롤 속도 (px/sec)")]
    public float scrollSpeed = 150f;

    [Tooltip("시작 시 뷰포트 오른쪽 밖 오프셋(양수)")]
    public float startGap = 16f;

    [Tooltip("왼쪽 밖으로 완전히 빠져나가는 여유(양수)")]
    public float endGap = 16f;

    [Header("Options")]
    public bool playOnStart = true;
    public bool deactivateOwnerOnFinish = true; // 끝나면 이 오브젝트 비활성화

    private bool _isScrolling;
    private float _timer;
    private float _totalTravel;   // 총 이동해야 할 거리
    private float _moved;         // 누적 이동 거리
    private Vector2 _startAnchoredPos; // 기준 시작 위치 (초기 상태 저장)

    private void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        if (!viewport && scrollRect) viewport = scrollRect.viewport;
        if (!targetContent && scrollRect) targetContent = scrollRect.content;

        if (scrollRect)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.inertia = false;
        }

        gameObject.SetActive(false);
    }

    public override void Open(float duration, string message, Action callback = null)
    {
        messageText.text = $"게임을 이용한 지 {message}시간이 지났습니다. 과도한 게임이용은 정상적인 일상생활에 지장을 줄 수 있습니다.";
        base.Open(duration, message, callback);
    }

    private void OnEnable()
    {
        if (playOnStart)
            StartCoroutine(BootstrapAndStart());
    }

    private IEnumerator BootstrapAndStart()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (targetContent)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetContent);
        }
        Canvas.ForceUpdateCanvases();
        StartScroll();
    }

    public void StartScroll()
    {
        // 기준 위치 저장
        _startAnchoredPos = targetContent.anchoredPosition;

        // 크기 계산
        float viewW = viewport.rect.width;
        float itemW = targetContent.rect.width;

        // 시작: 오른쪽 밖으로 이동
        var p = _startAnchoredPos;
        p.x += (viewW * 0.5f + itemW * 0.5f + startGap);
        targetContent.anchoredPosition = p;

        // 이동 거리 계산
        _totalTravel = startGap + viewW + itemW + endGap;

        _moved = 0f;
        _timer = -startDelay;
        _isScrolling = true;
    }

    private void Update()
    {
        if (!_isScrolling || !viewport || !targetContent) return;

        _timer += Time.deltaTime;
        if (_timer < 0f) return;

        float delta = scrollSpeed * Time.deltaTime;
        _moved += delta;

        Vector2 p = targetContent.anchoredPosition;
        p.x -= delta;
        targetContent.anchoredPosition = p;

        if (_moved >= _totalTravel)
        {
            _isScrolling = false;
            StartCoroutine(HoldThenReset());
        }
    }

    /// <summary>
    /// 스크롤 완료 후 잠깐 정지 후 초기 위치로 복귀
    /// </summary>
    private IEnumerator HoldThenReset()
    {
        if (endHoldTime > 0f)
            yield return new WaitForSeconds(endHoldTime);

        // 스크롤 중단 및 초기 위치 복귀
        _isScrolling = false;
        if (targetContent)
        {
            targetContent.anchoredPosition = _startAnchoredPos;
        }

        // 오브젝트 비활성화 (선택)
        if (deactivateOwnerOnFinish)
        {
            StopAndReset();
            gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 외부에서 텍스트 바꾼 후 재시작할 때 사용
    /// </summary>
    public void ResetAndPlay()
    {
        StopAllCoroutines();
        _isScrolling = false;
        StartCoroutine(BootstrapAndStart());
    }

    /// <summary>
    /// 즉시 종료 후 초기 위치 복귀
    /// </summary>
    public void StopAndReset()
    {
        StopAllCoroutines();
        _isScrolling = false;

        if (targetContent)
            targetContent.anchoredPosition = _startAnchoredPos;
    }
}
