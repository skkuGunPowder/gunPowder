using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuffSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _fallbackImage;
    [SerializeField] private Image _timerMask;
    [SerializeField] private TextMeshProUGUI _timerText;

    private Buff _buff;
    private float _buffTimer;
    private Coroutine _cooldownCoroutine;

    public Buff GetBuff()
    {
        return _buff;
    }

    public void StartBuffUI(Buff newBuff)
    {
        Debug.LogWarning("UI_BuffSlot - StartBuffUI 호출됨");
        _buff = newBuff;

        if (newBuff.Icon != null)
        {
            _iconImage.sprite = newBuff.Icon;
            _timerMask.sprite = newBuff.Icon;
        }
        else
        {
            Debug.LogError("Buff 아이콘이 설정되지 않았습니다.");
            _iconImage.sprite = _fallbackImage;
            _timerMask.sprite = _fallbackImage;
        }
        _cooldownCoroutine = StartCoroutine(StartCooldown(newBuff.Stat.Duration));
    }

    public void StopBuffUI()
    {
        Debug.LogWarning("UI_BuffSlot - StopBuffUI 호출됨");
        StopCoroutine(_cooldownCoroutine);
        _timerMask.fillAmount = 0f;
        _buff = null;
        gameObject.SetActive(false);
    }

    private IEnumerator StartCooldown(float duration)
    {
        _buffTimer = duration;
        _timerMask.fillAmount = 0f;

        while (_buffTimer > 0f)
        {
            _buffTimer -= Time.deltaTime;
            _timerMask.fillAmount = 1f - _buffTimer / duration;
            _timerText.text = $"{_buffTimer:F1}s";
            yield return null;
        }

        _timerMask.fillAmount = 1f;

        StopBuffUI();
    }
}
