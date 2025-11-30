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
    public TextMeshProUGUI GunpowderTextUGUI;
    public GameObject FirstPlace;
    
    public Image ProfileImage;
    public Image BombImage;
    
    public ProfileSkin PlayerProfileSkin;
    public UI_EmotionSlot Emotion;
    
    public List<GameObject> LifeList;
   
    [Header("Color")] 
    public int GunpowderMiddle = 50;
    public int GunpowderLow = 20;
    public Color32 LeftoverColor;
    
    [Header("Shaker")]
    public float Strength = 20f;
    public float Duration = 1f;
    public int Vibrato = 10;
    public Ease EaseType;
    public float ScaleStrength = 1.2f;
   
    public ChatBubbleListener ChatListener;
    
    public void Init(Sprite bombImage, EInGameTeam taem, PhotonPlayer player, int gunpowder, int life)
    {
        // PlayerActorNumber = player.ActorNumber;
        NicknameTextUGUI.text = player.NickName;
        BombImage.sprite = bombImage;
        ProfileImage.color = TeamColorSet(taem);
        PlayerProfileSkin.Init(player);
        GunpowderTextUGUI.text = gunpowder.ToString();
        LifeRefresh(life);
        if(ChatListener == null) 
            ChatListener = GetComponent<ChatBubbleListener>();
        if (ChatListener != null) 
            ChatListener.SetOwner(player.NickName);
    }
    public void Refresh(int gunpowder, int life, int attacker)
    {
        ColorSet(gunpowder);
        Shake(attacker);
        GunpowderTextUGUI.text = gunpowder.ToString();
        LifeRefresh(life);

    }

    private void LifeRefresh(int life)
    {
        for (int i = 0; i < LifeList.Count; i++)
        {
            if(i < life)
            {
                LifeList[i].SetActive(true); 
            }
            else
            {
                LifeList[i].SetActive(false);
            }
        }
    }
   
    public void LeftOverRefresh()
    {
        PlayerProfileSkin.PlayerLeft();
        GunpowderTextUGUI.text = "0";
        GunpowderTextUGUI.color = LeftoverColor;
        LifeRefresh(0);
    }
    private void ColorSet(int gunpowder)
    {
        if (gunpowder >= GunpowderMiddle)
        {
            GunpowderTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthDefault];
        }
        else if (gunpowder > GunpowderLow)
        {
            GunpowderTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthMiddle];
        }
        else
        {
            GunpowderTextUGUI.color = ColorPalette.ColorDictionary[EColorType.HealthLow];
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
        
        GunpowderTextUGUI.rectTransform.DOScale(ScaleStrength, Duration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            GunpowderTextUGUI.rectTransform.DOScale(1f, Duration).SetEase(Ease.InCubic);
        });
        GunpowderTextUGUI.rectTransform.DOShakeAnchorPos(Duration, Strength, Vibrato).SetEase(EaseType).OnComplete(() =>
        {
            GunpowderTextUGUI.rectTransform.DOAnchorPos(_gunpowderTextOriginalRectTransform, Duration).SetEase(EaseType);
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
