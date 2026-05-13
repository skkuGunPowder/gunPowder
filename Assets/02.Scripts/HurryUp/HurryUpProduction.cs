using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HurryUpProduction : MonoBehaviour
{
    [Header("Hurry Up 사운드")]
    [SerializeField] private string _hurryUpSound;
    
    [Header("Hurry Up 텍스트")]
    [SerializeField] private RectTransform _hurryUpText;
    [SerializeField] private TextMeshProUGUI _hurryUpTextComponent;
    
    [Header("Hurry Up 연출")]
    [SerializeField] private float _totalDuration = 4.5f;
    [SerializeField] private float _blinkInterval = 0.5f;

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
        _sequence?.Kill();
        SoundManager.Instance.PauseBGM();
        //SoundManager.Instance.PlayGlobalSound(_hurryUpSound);

        _hurryUpText.anchoredPosition = new Vector2(
            _hurryUpText.anchoredPosition.x, _bottomY);
        _hurryUpText.gameObject.SetActive(true);

        // 시작 색상
        _hurryUpTextComponent.color = _redColor;
        
        _sequence = DOTween.Sequence().SetUpdate(true);
        

        // [0.0s~1.0s] 하단→중앙 이동
        _sequence.Append(
            _hurryUpText.DOAnchorPosY(_centerY, 1f).SetEase(Ease.OutBack));

        // [1.0s~2.5s] 중앙에서 대기
        _sequence.AppendInterval(1.5f);
        
        // 처음부터 끝까지 0.5초마다 색상 변경
        for (int i = 1; i * _blinkInterval < _totalDuration; i++)
        {
            float time = i * _blinkInterval;
            Color blinkColor = i % 2 == 0 ? _redColor : _yellowColor;

            _sequence.InsertCallback(time, () =>
            {
                _hurryUpTextComponent.color = blinkColor;
            });
        }

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

        bool wasSequenceAlive = _sequence != null && _sequence.IsActive();

        _sequence?.Kill();
        _hurryUpText.gameObject.SetActive(false);
        if (wasSequenceAlive && SoundManager.Instance != null)
        {
            SoundManager.Instance.ResumeBGM();
            SoundManager.Instance.ResetBGMPitch();
        }

    }
}
