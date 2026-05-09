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
   
    [Header("카트리지 아이콘 슬롯")]
    [SerializeField] private List<Image> _cartridgeIconList = new List<Image>();
    [SerializeField] private List<Image> _cartridgeGradeIconList = new List<Image>();
    public ChatBubbleListener ChatListener;

    [Header("궁극기 게이지")]
    [SerializeField] private Image _ultimateGaugeBarFill; // 궁극기 게이지 바 Fill 이미지 (Image Type: Filled)
    public Image UltimateGaugeBarFill => _ultimateGaugeBarFill;
    
    public void Init(Sprite bombImage,Sprite subImage, EInGameTeam team, PhotonPlayer player, int hp, int life, int gp)
    {
        NicknameTextUGUI.text = player.NickName;
        BombImage.sprite = bombImage;
        SubBombImage.sprite = subImage;
        ProfileImage.color = ColorPalette.GetTeamColor(team);
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

    public void RefreshCartridges(PhotonPlayer player)
    {
        foreach (Image grade in _cartridgeGradeIconList)
        {
            grade.gameObject.SetActive(false);
        }

        if (!player.CustomProperties.TryGetValue(EProperties.Cartridges.ToString(), out object value))
        {
            return;
        }

        if (!(value is string[] entries))
        {
            return;
        }

        int slotIndex = 0;
        foreach (string entry in entries)
        {
            if (slotIndex >= _cartridgeIconList.Count)
            {
                break;
            }

            if (string.IsNullOrEmpty(entry))
            {
                continue;
            }

            string[] tokens = entry.Split('|');
            if (tokens.Length < 2)
            {
                continue;
            }

            string id = tokens[1];
            CartridgeData data = CartridgeFactory.Instance.GetCartridgeData(id);
            if (data == null)
            {
                continue;
            }
            _cartridgeGradeIconList[slotIndex].gameObject.SetActive(true);
            EColorType color = ColorPalette.GetColorTypeByName(data.Rarity.ToString());
            Color32[] colors = ColorPalette.GetGradationColors(color, 2);
            _cartridgeGradeIconList[slotIndex].color = colors[1];
            _cartridgeIconList[slotIndex].sprite = data.ImageSprite;
            slotIndex++;
        }
    }
}
