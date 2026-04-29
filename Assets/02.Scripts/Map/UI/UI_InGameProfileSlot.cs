using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfileSlot : MonoBehaviour
{
    // public int PlayerActorNumber { get; private set; }
    [SerializeField] private Vector2 _gunpowderTextOriginalRectTransform;
    public TextMeshProUGUI NicknameTextUGUI;
    public TextMeshProUGUI HPTextUGUI;
    public TextMeshProUGUI GPTextUGUI;
    public GameObject FirstPlace;
    
    public Image ProfileImage;
    public Image BombImage;
    public Image SubBombImage;
    
    public ProfileSkin PlayerProfileSkin;
    public UI_EmotionSlot Emotion;

    [Header("Life")]
    public GameObject LifePivot;
    public TextMeshProUGUI LifeText;
   
    [Header("Color")]
    public int HPMiddle = 75;
    public int HPLow = 30;
    public Color32 LeftoverColor;
    
    [Header("Shaker")]
    public float Strength = 20f;
    public float Duration = 1f;
    public int Vibrato = 10;
    public Ease EaseType;
    public float ScaleStrength = 1.2f;
   
    public ChatBubbleListener ChatListener;

    [Header("궁극기 게이지")]
    [SerializeField] private Image _ultimateGaugeBarFill; // 궁극기 게이지 바 Fill 이미지 (Image Type: Filled)
    public Image UltimateGaugeBarFill => _ultimateGaugeBarFill;
    
    public void Init(Sprite bombImage,Sprite subImage, EInGameTeam taem, PhotonPlayer player, int hp, int life, int gp)
    {
        NicknameTextUGUI.text = player.NickName;
        BombImage.sprite = bombImage;
        SubBombImage.sprite = subImage;
        ProfileImage.color = TeamColorSet(taem);
        PlayerProfileSkin.Init(player);
        HPTextUGUI.text = hp.ToString();
        ColorSet(hp);
        if (GPTextUGUI != null)
            GPTextUGUI.text = gp.ToString();
        LifeRefresh(life);
        if(ChatListener == null)
            ChatListener = GetComponent<ChatBubbleListener>();
        if (ChatListener != null)
            ChatListener.SetOwner(player.NickName);
    }

    public void Refresh(int hp, int life, int attacker)
    {
        ColorSet(hp);
        Shake(attacker);
        HPTextUGUI.text = hp.ToString();
        LifeRefresh(life);
    }

    public void RefreshBomb(Sprite main, Sprite sub)
    {
        BombImage.sprite = main;
        SubBombImage.sprite = sub;
    }

    public void RefreshGP(int gp)
    {
        if (GPTextUGUI != null)
            GPTextUGUI.text = gp.ToString();
    }

    private void LifeRefresh(int life)
    {
        LifePivot.SetActive(life >= 1);
        LifeText.text = life.ToString();
    }
   
    public void LeftOverRefresh()
    {
        PlayerProfileSkin.PlayerLeft();
        HPTextUGUI.text = "0";
        HPTextUGUI.color = LeftoverColor;
        if (GPTextUGUI != null)
        {
            GPTextUGUI.text = "0";
            GPTextUGUI.color = LeftoverColor;
        }
        LifeRefresh(0);
    }
    private void ColorSet(int hp)
    {
        if (hp >= HPMiddle)
        {
            HPTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthDefault];
        }
        else if (hp > HPLow)
        {
            HPTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthMiddle];
        }
        else
        {
            HPTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthLow];
        }
    }

    private Color32 TeamColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red:
                return ColorPalette.ColorDictionary[EColorType.Red];
            case EInGameTeam.Blue:
                return ColorPalette.ColorDictionary[EColorType.Blue];
            case EInGameTeam.Green:
                return ColorPalette.ColorDictionary[EColorType.Green];
            case EInGameTeam.Yellow:
                return ColorPalette.ColorDictionary[EColorType.Yellow];
            default:
                return ColorPalette.ColorDictionary[EColorType.Red];
        }
    }

    private void Shake(int attacker)
    {
        if (attacker == 0)
        {
            return;
        }
        
        DOTween.Kill(this);
        
        HPTextUGUI.rectTransform.DOScale(ScaleStrength, Duration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            HPTextUGUI.rectTransform.DOScale(1f, Duration).SetEase(Ease.InCubic);
        });
        HPTextUGUI.rectTransform.DOShakeAnchorPos(Duration, Strength, Vibrato).SetEase(EaseType).OnComplete(() =>
        {
            HPTextUGUI.rectTransform.DOAnchorPos(_gunpowderTextOriginalRectTransform, Duration).SetEase(EaseType);
        });
    }
    public void SetTop(bool isTop)
    {
        FirstPlace.SetActive(isTop);
    }

    public void PlayEmotion(string emotionName)
    {
        Emotion.Play(emotionName);
    }
}
