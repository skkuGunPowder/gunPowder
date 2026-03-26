using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HurryUpProduction : MonoBehaviour
{
    [Header("Hurry Up 텍스트")]
    [SerializeField] private RectTransform _hurryUpText;
    [SerializeField] private TextMeshProUGUI _hurryUpTextComponent;

    [Header("위치 설정")]
    [SerializeField] private float _bottomY;
    [SerializeField] private float _centerY;
    [SerializeField] private float _topY;

    [Header("색상 설정")]
    [SerializeField] private Color _redColor = Color.red;
    [SerializeField] private Color _yellowColor = Color.yellow;

    private Sequence _sequence;

    private void Awake()
    {
        EventManager.Instance.OnHurryUp += PlayHurryUp;
        _hurryUpText.gameObject.SetActive(false);
    }

    private void PlayHurryUp()
    {
        SoundManager.Instance.PauseBGM();

        _hurryUpText.anchoredPosition = new Vector2(
            _hurryUpText.anchoredPosition.x, _bottomY);
        _hurryUpText.gameObject.SetActive(true);

        _sequence = DOTween.Sequence().SetUpdate(true);

        // [0.0s~1.0s] 하단→중앙 이동
        _sequence.Append(
            _hurryUpText.DOAnchorPosY(_centerY, 1f).SetEase(Ease.OutBack));

        // [1.0s~2.5s] 0.5초마다 붉은색↔노란색 점멸
        _sequence.Append(
            _hurryUpTextComponent.DOColor(_redColor, 0f));
        _sequence.AppendInterval(0.5f);
        _sequence.Append(
            _hurryUpTextComponent.DOColor(_yellowColor, 0f));
        _sequence.AppendInterval(0.5f);
        _sequence.Append(
            _hurryUpTextComponent.DOColor(_redColor, 0f));
        _sequence.AppendInterval(0.5f);

        // [2.5s~3.5s] 중앙→상단 이동
        _sequence.Append(
            _hurryUpText.DOAnchorPosY(_topY, 1f).SetEase(Ease.InBack));

        // [3.5s~4.5s] 1초 대기 후 비활성화
        _sequence.AppendInterval(1f);
        _sequence.AppendCallback(() =>
        {
            _hurryUpText.gameObject.SetActive(false);

            SoundManager.Instance.ReplayBGMWithPitch(1.2f);

            GimmickManager.Instance.ActivateRandomByGroup(GimmickGroupType.HurryUp, 1);
        });
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.OnHurryUp -= PlayHurryUp;

        _sequence?.Kill();
        _hurryUpText.gameObject.SetActive(false);
    }
}
