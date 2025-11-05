using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuffSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _timerMask;
    [SerializeField] private TextMeshProUGUI _timerText;

    private float _buffTimer;

    public void StartBuffUI(Buff newBuff)
    {
        Debug.LogWarning("UI_BuffSlot - StartBuffUI 호출됨");
        if (newBuff.Icon != null)
        {
            _iconImage.sprite = newBuff.Icon;
        }
        else
        {
            Debug.LogError("Buff 아이콘이 설정되지 않았습니다.");
        }
        StartCoroutine(StartCooldown(newBuff.Stat.Duration));
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

        gameObject.SetActive(false);
    }
}
