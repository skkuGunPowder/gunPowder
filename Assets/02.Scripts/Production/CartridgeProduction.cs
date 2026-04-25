using System;
using DG.Tweening;
using SpriteTrail;
using UnityEngine;
using UnityEngine.UI;
using PhotonPlayer =  Photon.Realtime.Player;
public class CartridgeProduction : MonoBehaviour
{
    [Header("Production Object")]
    [SerializeField] private Image _background;
    
    
    [Header("Color Change")]
    [SerializeField] private float _colorChangeTime = 0.5f;
    [SerializeField] private Ease _colorChangeEase = Ease.Linear;
    
    
    private void Awake()
    {
        EventManager.Instance.OnCartridgeStateEnter += SubscribePlay;
    }

    private void SubscribePlay()
    {
        EventManager.Instance.OnCartridgeStart -= PlayerChange;
        EventManager.Instance.OnCartridgeStart += Play;
    }

    private void Play(PhotonPlayer player)
    {
        EInGameTeam team = EInGameTeam.Red;
        
        if (player != null && player.CustomProperties.ContainsKey(EProperties.Team.ToString()))
        {
            team = (EInGameTeam)player.CustomProperties[EProperties.Team.ToString()];    
        }
        
        SetBackGroundColor(team);

        Sequence seq = DOTween.Sequence();
        // 1. 뒷배경 등장
        // 2. 카트리지 떨어지기
        // 3. 프로필 등장하기
        // 4. 등장 애니메이션 구독 종료
        seq.OnComplete(() =>
        {
            EventManager.Instance.OnCartridgeStart -= Play;
            EventManager.Instance.OnCartridgeStart += PlayerChange;
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

    private void OnDestroy()
    {
        EventManager.Instance.OnCartridgeStateEnter -= SubscribePlay;
        EventManager.Instance.OnCartridgeStart -= Play;
        EventManager.Instance.OnCartridgeStart -= PlayerChange;
    }
}
