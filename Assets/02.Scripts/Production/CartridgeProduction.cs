using System;
using DG.Tweening;
using SpriteTrail;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer =  Photon.Realtime.Player;
using Sequence = DG.Tweening.Sequence;

[Serializable]
public class ProductionStat
{
    public float Duration;
    
    public Vector3 StartVector;
    public float StartValue;
    
    public Vector3 EndVector;
    public float EndValue;
    
    public Ease ProductionEase;
}
public class CartridgeProduction : MonoBehaviour
{
    [Header("Production Object")]
    [SerializeField] private Image _background;
    [SerializeField] private RectTransform _cartridgePivot;
    [SerializeField] private RectTransform _profileSlotRectTransform;
    [SerializeField] private GameObject _buttonPivot;
    
    [Header("Production Variables")]
    public ProductionStat BackgroundVariables;
    public ProductionStat ProfileVariables;
    public ProductionStat CartridgeVariables;
    
    [Header("Color Change")]
    [SerializeField] private float _colorChangeTime = 0.5f;
    [SerializeField] private Ease _colorChangeEase = Ease.Linear;
    
    
    private void Awake()
    {
        EventManager.Instance.OnCartridgeStateEnter += SubscribePlay;
        EventManager.Instance.OnCartridgeStart += PlayerChange;
        EventManager.Instance.OnCartridgeEnd += Set;
    }

    private void SubscribePlay()
    {
        Play();
    }

    private void Play()
    {
        Sequence seq = DOTween.Sequence();
        // 1. 뒷배경 등장
        seq.Append(_profileSlotRectTransform.DOAnchorPosY(ProfileVariables.EndValue, ProfileVariables.Duration).SetEase(ProfileVariables.ProductionEase));
        seq.Join(_background.rectTransform.DOAnchorPosX(BackgroundVariables.EndValue,BackgroundVariables.Duration).SetEase(BackgroundVariables.ProductionEase));
        // 2. 카트리지 떨어지기
        seq.Join(_cartridgePivot.DOAnchorPosY(CartridgeVariables.EndValue, CartridgeVariables.Duration).SetEase(CartridgeVariables.ProductionEase));
        // 3. 프로필 등장하기
        // 4. 등장 애니메이션 구독 종료
        seq.OnComplete(() =>
        {
            _buttonPivot.gameObject.SetActive(true);
        });
    }
    
    // 뒷 배경 색 설정하기
    private void SetBackGroundColor(EInGameTeam team, bool isDotween = false)
    {
        Color32 color = ColorPalette.GetTeamColor(team);

        if (!isDotween)
        {
            _background.color = color;
            return;
        }
        
        // 두트윈
        _background.DOColor(color, _colorChangeTime).SetEase(_colorChangeEase);
    }

    private void PlayerChange(PhotonPlayer player)
    {
        //플레이어만 바꿔주기
        //1. 팀 배경색
        EInGameTeam team = EInGameTeam.Red;
        
        if (player != null && player.CustomProperties.ContainsKey(EProperties.Team.ToString()))
        {
            team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];    
        }
        
        SetBackGroundColor(team, isDotween: true);
        //2. 플레이어 슬롯 크기 변경 (건파우더 변경)
    }

    private void Set()
    {
        _background.rectTransform.anchoredPosition = new Vector2(BackgroundVariables.StartValue,0);
        _cartridgePivot.anchoredPosition = new Vector2(0,CartridgeVariables.StartValue);
        _profileSlotRectTransform.anchoredPosition = new Vector2(0,ProfileVariables.StartValue);
        _buttonPivot.gameObject.SetActive(false);
        
        UI_CartridgeShopPopup popup = (UI_CartridgeShopPopup)PopupManager.Instance.GetPopup(EPopupType.UI_CartridgeShopPopup);
        popup?.Close();

    }
    private void OnDestroy()
    {
        EventManager.Instance.OnCartridgeEnd -= Set;
        EventManager.Instance.OnCartridgeStateEnter -= SubscribePlay;
        EventManager.Instance.OnCartridgeStart -= PlayerChange;
    }
}
