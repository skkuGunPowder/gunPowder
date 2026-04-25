using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class CartridgeAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 마우스 포인터가 enter, exit, 
    // 클릭의 경우 off되도록
    
    [SerializeField] private RectTransform _rectTransform;
    
    [Header("Hit Action")]
    [SerializeField] private int _slotNumber;
    [SerializeField] private int _rotateAmount;
    [SerializeField] private float _sideMoveAmount;
    [SerializeField] private float _sideDuration;
    [SerializeField] private Ease _sideMoveEase;
    
    [Header("Pointer Enter")] 
    [SerializeField] private float _EnterScale;
    [SerializeField] private float _originScale;
    [SerializeField] private float _EnterScaleTime;
    [SerializeField] private float _originScaleTime;
    [SerializeField] private Ease _EnterScaleEase;
    [SerializeField] private Ease _originScaleEase;

    [Header("Select Action")] 
    [SerializeField] private float _turnCount;
    [SerializeField] private float _turnDuration;
    [SerializeField] private Ease _turnEase;
    
    [Header("Move Action")]
    [SerializeField] private Vector3 _scaleVector;
    [SerializeField] private float _scaleDuration;
    [SerializeField] private Ease _scaleEase;
    [SerializeField] private float _moveDuration;
    [SerializeField] private float _moveHeight;
    [SerializeField] private Ease _moveEase;
    [SerializeField] private float _fallDuration;
    [SerializeField] private float _fallHeight;
    [SerializeField] private Ease _fallEase;

    private bool _selected = false;
    private void Awake()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_selected)
        {
            return;
        }
        
        _rectTransform.DOScale(_EnterScale, _EnterScaleTime).SetEase(_EnterScaleEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_selected)
        {
            return;
        }

        _rectTransform.DOScale(_originScale, _originScaleTime).SetEase(_originScaleEase);
    }

    public void OnClickButton()
    {
        _selected = true;
        _rectTransform.localScale = new Vector3(_EnterScale, _EnterScale, _EnterScale);
        
        Sequence seq = DOTween.Sequence();
        seq.Append(_rectTransform.DOAnchorPosY(_moveHeight, _moveDuration).SetEase(_moveEase));
        seq.Append(_rectTransform.DOAnchorPosY(_fallHeight, _fallDuration).SetEase(_fallEase));
        seq.Join(_rectTransform.DOAnchorPosX(_sideMoveAmount * _slotNumber, _sideDuration).SetEase(_sideMoveEase));
        seq.Join(_rectTransform.DOScale(_scaleVector,  _scaleDuration).SetEase(_scaleEase));
        seq.JoinCallback(TurnCartridge);
        
    }

    private void TurnCartridge()
    {
        float turn = _turnCount * 360;
        Vector3 turnVector = new Vector3(0, turn, 0);
        _rectTransform.DORotate(turnVector, _turnDuration,RotateMode.LocalAxisAdd).SetEase(_turnEase);
    }   
    private void OnDisable()
    {
        _selected = false;
        _rectTransform.transform.localPosition = new Vector3(0, 0, 0);
        _rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        _rectTransform.localScale = new Vector3(_originScale,_originScale,_originScale);
    }

}
