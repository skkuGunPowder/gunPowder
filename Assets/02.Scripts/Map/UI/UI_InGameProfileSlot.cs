using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PhotonPlayer = Photon.Realtime.Player;
public class UI_InGameProfileSlot : MonoBehaviour
{
    public TextMeshProUGUI NicknameTextUGUI;
    public TextMeshProUGUI GunpowderTextUGUI;
    public GameObject FirstPlace;
    
    public Image ProfileImage;
    public Image BombImage;
    
    public ProfileSkin PlayerProfileSkin;
    public UI_EmotionSlot Emotion;
    
    public List<GameObject> LifeList;
    public List<Color32> GunPowderColorCodeList;
    public List<Color32> TeamColorCodeList;

    [Header("Shaker")]
    public float Strength = 20f;
    public float Duration = 1f;
    public Ease EaseType;
    public void Init(string playerName, Sprite bombImage, EInGameTeam taem, PhotonPlayer player)
    {
        NicknameTextUGUI.text = playerName;
        BombImage.sprite = bombImage;
        ProfileImage.color = TeamColorSet(taem);
        PlayerProfileSkin.Init(player);
    }
    public void Refresh(int gunpowder, int life)
    {
        if (gunpowder > 50)
        {
            GunpowderTextUGUI.color = GunPowderColorCodeList[0];
        }
        else if (gunpowder > 20)
        {
            GunpowderTextUGUI.color = GunPowderColorCodeList[1];
        }
        else
        {
            GunpowderTextUGUI.color = GunPowderColorCodeList[2];
        }

        Shake();
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

    private Color32 TeamColorSet(EInGameTeam team)
    {
        switch (team)
        {
            case EInGameTeam.Red:
                return TeamColorCodeList[0];
            case EInGameTeam.Blue:
                return TeamColorCodeList[1];
            case EInGameTeam.Green:
                return TeamColorCodeList[2];
            case EInGameTeam.Yellow:
                return TeamColorCodeList[3];
            default:
                return TeamColorCodeList[0];
        }
    }

    private void Shake()
    {
        Debug.Log("Shake");
        GunpowderTextUGUI.rectTransform.DOShakeAnchorPos(Duration, Strength).SetEase(EaseType);
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
