using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UltimateProductionSlot : MonoBehaviour
{
    public List<UltimateEffectBase> EffectList;
    
    public List<GameObject> UltimateEffectList;
    [Header("플레이어 이미지")] 
    public GameObject PlayerImage;
    
    [Header("궁극기 연출 시간")]
    public float UltimateTime = 1f;
    [Header("궁극기 애니메이션 오브젝트 관련")]
    public RectTransform UltimateEffectUp;
    public RectTransform UltimateEffectDown;
    public Image UpBackGround;
    public Image DownBackGround;
    public float UltimateEffectSpeed = 1f;
    public Ease UltimateEffectInEase = Ease.Linear;
    public Ease UltimateEffectOutEase = Ease.Linear;
    public Vector2 UltimateEndPosition;
    public Vector2 UltimateEndPosition2;
    
    [Header("위치 초기화")]
    public Vector2 UltimateOriginPosition;
    public Vector2 UltimateOriginPosition2;
    
    [Header("컬러")]
    public ColorPalette ColorPalette;
    public void Play(string bomb, bool isMyTeam)
    {
        
        UltimateEffectUp.gameObject.SetActive(true);
        UltimateEffectDown.gameObject.SetActive(true);
        PlayerImage.gameObject.SetActive(true);
        
        BackGroundColorChange(isMyTeam);
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(UltimateEffectUp.DOAnchorPos(UltimateEndPosition, UltimateEffectSpeed).SetEase(UltimateEffectInEase));
        sequence.Join(UltimateEffectDown.DOAnchorPos(UltimateEndPosition2, UltimateEffectSpeed).SetEase(UltimateEffectInEase));
        sequence.JoinCallback(()=>BombEffectOn(bomb));
        sequence.AppendInterval(UltimateTime);
        sequence.Append(UltimateEffectUp.DOAnchorPos(UltimateOriginPosition, UltimateEffectSpeed).SetEase(UltimateEffectOutEase));
        sequence.Join(UltimateEffectDown.DOAnchorPos(UltimateOriginPosition2, UltimateEffectSpeed).SetEase(UltimateEffectOutEase));
        sequence.OnComplete(() =>
        {
            BombEffectOff(bomb);
            
            Debug.Log("off");
            UltimateEffectUp.gameObject.SetActive(false);
            UltimateEffectDown.gameObject.SetActive(false);
            PlayerImage.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        });
    }

    private void BackGroundColorChange(bool isMyTeam)
    {
        if (isMyTeam)
        {
            UpBackGround.color = ColorPalette.ColorDictionary[EColorType.UltimatePlayer];
            DownBackGround.color = ColorPalette.ColorDictionary[EColorType.UltimatePlayer];
        }
        else
        {
            UpBackGround.color = ColorPalette.ColorDictionary[EColorType.UltimateEnemy];
            DownBackGround.color = ColorPalette.ColorDictionary[EColorType.UltimateEnemy];
        }
    }
    private void BombEffectOn(string bomb)
    {
        foreach (UltimateEffectBase effect in EffectList)
        {
            if (effect.BombName == bomb)
            {
                effect.Play();
                break;
            }
        }
    }
    private void BombEffectOff(string bomb)
    {
        foreach (UltimateEffectBase effect in EffectList)
        {
            if (effect.BombName == bomb)
            {
                effect.Stop();
                break;
            }
        }
    }

    public void AddPlayer(GameObject player)
    {
        PlayerImage = player;
    }
    private void OnDisable()
    {
        UltimateEffectUp.anchoredPosition = UltimateOriginPosition;
        UltimateEffectDown.anchoredPosition = UltimateOriginPosition2;
    }
}
