using DG.Tweening;
using UnityEngine;

public class UI_CartridgeProfileSlot : UI_InGameProfileSlot
{
    [SerializeField] private GameObject _turnIndicator;

    [Header("Turn Animation")]
    [SerializeField] private float _turnScale;
    [SerializeField] private float _idleScale;
    [SerializeField] private float _turnDuration;
    [SerializeField] private Ease _turnEase;
    [SerializeField] private RectTransform _profileSlotRectTransform;
    private RectTransform _rectTransform;


    public void SetMyTurn(bool isTurn)
    {
        _turnIndicator.SetActive(isTurn);
        DOTween.Kill(_profileSlotRectTransform);
        float targetScale = isTurn ? _turnScale : _idleScale;
        _profileSlotRectTransform.DOScale(targetScale, _turnDuration).SetEase(_turnEase);
    }
}
