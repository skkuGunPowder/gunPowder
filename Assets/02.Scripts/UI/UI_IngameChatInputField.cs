using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 범용 채팅 InputField 컴포넌트
/// 인게임, 파티, 귓속말 등 다양한 채팅에서 재사용 가능
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class UI_IngameChatInputField : MonoBehaviour
{
    private TMP_InputField _inputField;
    private TMP_Text _placeholderText;

    [Header("Settings")]
    [SerializeField] private string defaultPlaceholder = "ENTER MESSAGE...";
    [SerializeField] private int characterLimit = 20;
    [SerializeField] private bool autoFocusOnEnable = false;
    [SerializeField] private bool clearOnSubmit = true;
    [SerializeField] private bool focusAfterSubmit = true;

    /// <summary>
    /// 텍스트가 제출되었을 때 호출되는 이벤트 (비어있지 않은 텍스트만)
    /// </summary>
    public event Action<string> OnSubmit;
    // 비어있는텍스트 이벤트
    public event Action OnEmptySubmit;

    // 자동 포커스를 위한 변수
    private bool _shouldAutoFocus = false;
    private float _focusAttemptTime = 0f;
    private const float FOCUS_ATTEMPT_DURATION = 0.5f; // 0.5초 동안 계속 시도

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();

        if (_inputField != null && _inputField.placeholder != null)
        {
            _placeholderText = _inputField.placeholder as TMP_Text;
            if (_placeholderText != null)
            {
                _placeholderText.text = defaultPlaceholder; 
            }
        }

        if (_inputField != null)
        {
            _inputField.onEndEdit.AddListener(OnEndEdit);
            _inputField.onValueChanged.AddListener(OnValueChanged);
        }
    }

    private void OnEnable()
    {
        if (autoFocusOnEnable)
        {
            _shouldAutoFocus = true;
            _focusAttemptTime = Time.unscaledTime;
        }
    }

    private void Update()
    {
        // 자동 포커스가 활성화되어 있고, 아직 포커스되지 않았다면 계속 시도
        if (_shouldAutoFocus)
        {
            // 0.5초 동안 계속 포커스 시도
            if (Time.unscaledTime - _focusAttemptTime < FOCUS_ATTEMPT_DURATION)
            {
                // 강제로 계속 포커스 시도
                if (_inputField != null && !_inputField.isFocused)
                {
                    _inputField.ActivateInputField();
                    _inputField.Select();
                }
            }
            else
            {
                // 0.5초 경과하면 포커스 시도 중단
                _shouldAutoFocus = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (_inputField != null)
        {
            _inputField.onEndEdit.RemoveListener(OnEndEdit);
            _inputField.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    /// <summary>
    /// InputField의 onEndEdit 콜백
    /// </summary>
    private void OnEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitText();
        }
    }

    /// <summary>
    /// InputField의 onValueChanged 콜백 - 글자 수 제한
    /// </summary>
    private void OnValueChanged(string text)
    {
        if (text.Length > characterLimit)
        {
            _inputField.text = text.Substring(0, characterLimit);
        }
    }

    /// <summary>
    /// 텍스트 제출 처리
    /// </summary>
    private void SubmitText()
    {
        if (_inputField == null) return;

        string text = _inputField.text;

        // 빈 텍스트나 공백만 있는 경우 무시
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
        {
            if (clearOnSubmit)
            {
                _inputField.text = string.Empty;
            }
            // [추가] 빈 상태에서 엔터 입력 시 이벤트 발생
            OnEmptySubmit?.Invoke();
            if (focusAfterSubmit)
            {
                Focus();
            }
            return;
        }

        // 이벤트 발생
        OnSubmit?.Invoke(text);

        // 자동 클리어
        if (clearOnSubmit)
        {
            _inputField.text = string.Empty;
        }

        // 자동 포커스 유지
        if (focusAfterSubmit)
        {
            Focus();
        }
    }

    /// <summary>
    /// 현재 InputField가 활성화되어 있는지 확인
    /// </summary>
    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    /// <summary>
    /// InputField가 포커스되어 있는지 확인
    /// </summary>
    public bool IsFocused()
    {
        return _inputField != null && _inputField.isFocused;
    }

    /// <summary>
    /// InputField에 포커스 설정
    /// </summary>
    public void Focus()
    {
        if (_inputField != null)
        {
            _inputField.ActivateInputField();
            _inputField.Select();
        }
    }

    /// <summary>
    /// 포커스 설정 (지연 처리)
    /// </summary>
    public void FocusDelayed()
    {
        _shouldAutoFocus = true;
        _focusAttemptTime = Time.unscaledTime;
    }

    /// <summary>
    /// InputField의 텍스트 가져오기
    /// </summary>
    public string GetText()
    {
        return _inputField != null ? _inputField.text : string.Empty;
    }

    /// <summary>
    /// InputField의 텍스트 설정
    /// </summary>
    public void SetText(string text)
    {
        if (_inputField != null)
        {
            _inputField.text = text;
        }
    }

    /// <summary>
    /// InputField의 텍스트 비우기
    /// </summary>
    public void ClearText()
    {
        SetText(string.Empty);
    }

    /// <summary>
    /// 글자 수 제한 설정 (동적 변경)
    /// </summary>
    public void SetCharacterLimit(int limit)
    {
        characterLimit = limit;

        // 현재 텍스트가 제한을 초과하면 자르기
        if (_inputField != null && _inputField.text.Length > limit)
        {
            _inputField.text = _inputField.text.Substring(0, limit);
        }
    }

    /// <summary>
    /// Placeholder 텍스트 업데이트
    /// </summary>
    public void UpdatePlaceholder(string message, float restoreDelay)
    {
        if (_placeholderText != null)
        {
            string originalText = _placeholderText.text;
            _placeholderText.text = message;

            // 지연 후 원래 placeholder로 복구
            StartCoroutine(RestorePlaceholder(originalText, restoreDelay));
        }
    }

    /// <summary>
    /// Placeholder 복구 Coroutine
    /// </summary>
    private IEnumerator RestorePlaceholder(string originalText, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_placeholderText != null)
        {
            _placeholderText.text = originalText;
        }
    }

    /// <summary>
    /// Placeholder를 기본값으로 복구
    /// </summary>
    public void RestorePlaceholderToDefault()
    {
        if (_placeholderText != null)
        {
            _placeholderText.text = defaultPlaceholder;
        }
    }

    /// <summary>
    /// InputField 참조 가져오기 (읽기 전용)
    /// </summary>
    public TMP_InputField GetInputField() => _inputField;
}
