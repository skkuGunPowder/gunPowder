using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class CartridgeAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 마우스 포인터가 enter, exit,
    // 클릭의 경우 off되도록

    [SerializeField] private RectTransform _rectTransform;
    private UI_CartridgeShopPopup _shopPopup;
    private UI_CartridgeGoods uiCartridgeGoods;
    
    [Header("Hit Action")]
    [SerializeField] private int _slotCount;
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
    public bool Selected => _selected;
    private int _slotNumber;
    private void Awake()
    {
        if (_rectTransform == null)
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        if (_shopPopup == null)
        {
            _shopPopup = (UI_CartridgeShopPopup)PopupManager.Instance.GetPopup(EPopupType.UI_CartridgeShopPopup);
        }

        if (uiCartridgeGoods == null)
        {
            uiCartridgeGoods = GetComponentInParent<UI_CartridgeGoods>();
        }
    }

    public void SetSlotNumber(int number)
    {
        _slotNumber = number;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_shopPopup.MyTurn == false) // 내 턴에만 작동
        {
            return;
        }
        
        if (_selected)
        {
            return;
        }

        _shopPopup.RequestHoverCartridge(_slotNumber, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_shopPopup.MyTurn == false) // 내 턴에만 작동
        {
            return;
        }
        
        if (_selected)
        {
            return;
        }

        _shopPopup.RequestHoverCartridge(_slotNumber, false);
    }

    public void PlayEnterAnimation()
    {
        _rectTransform.DOScale(_EnterScale, _EnterScaleTime).SetEase(_EnterScaleEase);
    }

    public void PlayExitAnimation()
    {
        _rectTransform.DOScale(_originScale, _originScaleTime).SetEase(_originScaleEase);
    }

    public void OnClickButton()
    {
        if (_shopPopup.MyTurn == false) // 내 턴에만 작동
        {
            return;
        }

        if (_selected) // 선택된 것만 작동
        {
            return;
        }

        // GP 체크
        if (!RoomStatManager.Instance.CanChangeGP(-uiCartridgeGoods.Price))
        {
            return;
        }

        string newId = uiCartridgeGoods.CartridgeData.ID;

        // 이미 보유 중인 영구형 → 수리 경로 (ChangePopup 없이)
        // 내구도 꽉 참 or 이번 턴 이미 수리한 경우 CanBuy가 false를 반환해 차단
        if (CartridgeInventoryManager.Instance.GetPermanentCartridges().ContainsKey(newId))
        {
            if (uiCartridgeGoods.CanBuy() == false)
            {
                return;
            }
            _selected = true;
            _shopPopup.RequestSelectCartridge(_slotNumber);
            return;
        }

        // 이미 보유 중인 소모형 → 중복 구매 불가
        if (CartridgeInventoryManager.Instance.GetConsumableCartridges().ContainsKey(newId))
        {
            return;
        }

        // 슬롯이 가득 찬 경우 교체 팝업
        if (CartridgeInventoryManager.Instance.GetTotalCount() >= 3)
        {
            UI_ChangePopup changePopup = (UI_ChangePopup)PopupManager.Instance.Open(EPopupType.UI_ChangePopup);
            changePopup.Setup(uiCartridgeGoods.CartridgeData, uiCartridgeGoods.Price, OnExchangeCompleted);
            return;
        }

        // 정상 구매
        if (uiCartridgeGoods.CanBuy() == false)
        {
            return;
        }

        _selected = true; // 구매 직후 즉시 잠금
        _shopPopup.RequestSelectCartridge(_slotNumber);
    }

    private void OnExchangeCompleted()
    {
        _selected = true;
        _shopPopup.RequestSelectCartridge(_slotNumber);
    }

    public void PlaySelectAnimation()
    {
        _shopPopup.SetClickLock(false); // 중복 클릭 방지

        _selected = true;
        _rectTransform.localScale = new Vector3(_EnterScale, _EnterScale, _EnterScale);

        Sequence seq = DOTween.Sequence();
        seq.Append(_rectTransform.DOAnchorPosY(_moveHeight, _moveDuration).SetEase(_moveEase));
        seq.Append(_rectTransform.DOAnchorPosY(_fallHeight, _fallDuration).SetEase(_fallEase));
        seq.Join(_rectTransform.DOAnchorPosX(_sideMoveAmount * _slotCount, _sideDuration).SetEase(_sideMoveEase));
        seq.Join(_rectTransform.DOScale(_scaleVector, _scaleDuration).SetEase(_scaleEase));
        seq.JoinCallback(TurnCartridge).OnComplete(() =>
        {
            EventManager.Instance.ScreenClick();
        });
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
        DOTween.Kill(_rectTransform);
        _rectTransform.transform.localPosition = new Vector3(0, 0, 0);
        _rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        _rectTransform.localScale = new Vector3(_originScale,_originScale,_originScale);
    }

}
