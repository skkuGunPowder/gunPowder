using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_TextBlinkMove : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private float blinkDuration = 0.5f;
    [SerializeField] private float moveDistance = 20f;
    [SerializeField] private float moveDuration = 1f;

    private Sequence _sequence;

    private void Start()
    {
        if (targetText == null) targetText = GetComponent<TMP_Text>();

        RectTransform rect = targetText.rectTransform;

        _sequence = DOTween.Sequence();

        // 깜빡임 효과 (알파값 1 → 0)
        _sequence.Append(targetText.DOFade(0f, blinkDuration).SetLoops(-1, LoopType.Yoyo));

        // 텍스트 위아래 움직임 (localPosition.y 기준)
        rect.DOLocalMoveY(rect.localPosition.y + moveDistance, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        _sequence.Kill();
    }
}