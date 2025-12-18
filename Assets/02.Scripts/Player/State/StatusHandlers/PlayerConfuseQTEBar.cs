using UnityEngine;
using Microlight.MicroBar;

/// <summary>
/// 혼란 QTE 강도 표시용 게이지 바
/// - Microlight MicroBar를 사용해서 0 ~ maxIntensity까지 차는 바를 표시
/// - ConfuseStatusHandler의 QTE 이벤트를 구독해서 값/표시를 갱신
/// </summary>
public class PlayerConfuseQTEBar : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private MicroBar _intensityBar;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool _initialized = false;
    private float _maxIntensity = 0f;

    /// <summary>
    /// ConfuseStatusHandler와 연결 및 바 초기화
    /// </summary>
    public void Bind(ConfuseStatusHandler handler, float maxIntensity)
    {
        if (handler == null || _intensityBar == null)
        {
            Debug.LogWarning("[PlayerConfuseQTEBar] handler 또는 MicroBar가 없습니다.");
            return;
        }

        // CanvasGroup 자동 세팅
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        _maxIntensity = maxIntensity;

        // MicroBar 초기화 (0 ~ maxIntensity)
        _intensityBar.Initialize(_maxIntensity);
        _intensityBar.UpdateBar(0f, skipAnimation: true);

        // 이벤트 구독
        handler.OnQTEStarted += HandleQTEStarted;
        handler.OnQTEProgressChanged += HandleQTEProgressChanged;
        handler.OnQTEEnded += HandleQTEEnded;

        // 처음에는 숨김
        _canvasGroup.alpha = 0f;

        _initialized = true;
    }

    /// <summary>
    /// ConfuseStatusHandler에서 더 이상 사용하지 않을 때 이벤트 해제
    /// </summary>
    public void Unbind(ConfuseStatusHandler handler)
    {
        if (!_initialized || handler == null)
            return;

        handler.OnQTEStarted -= HandleQTEStarted;
        handler.OnQTEProgressChanged -= HandleQTEProgressChanged;
        handler.OnQTEEnded -= HandleQTEEnded;

        _initialized = false;
    }

    private void HandleQTEStarted()
    {
        if (!_initialized)
            return;

        // 바 보이게 하고 0으로 리셋
        _canvasGroup.alpha = 1f;
        _intensityBar.UpdateBar(0f, skipAnimation: true);
    }

    private void HandleQTEProgressChanged(float currentScore, float maxIntensity)
    {
        if (!_initialized)
            return;

        // 현재 점수 기준으로 채워지는 연출
        _intensityBar.UpdateBar(currentScore, UpdateAnim.Heal);
    }

    private void HandleQTEEnded()
    {
        if (!_initialized)
            return;

        // 혼란 해제 후 바 숨기기
        _canvasGroup.alpha = 0f;
    }
}
