using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UltimateProductionSlot : MonoBehaviour
{
    
    public List<GameObject> UltimateEffectList;
    [Header("궁극기 연출 시간")]
    public float UltimateTime = 1f;
    [Header("궁극기 애니메이션 오브젝트 관련")]
    public RectTransform UltimateEffectUp;
    public RectTransform UltimateEffectDown;
    public float UltimateEffectSpeed = 1f;
    public Ease UltimateEffectEase = Ease.Linear;
    public Vector2 UltimateEndPosition;
    public Vector2 UltimateEndPosition2;
    
    [Header("위치 초기화")]
    public Vector2 UltimateOriginPosition;
    public Vector2 UltimateOriginPosition2;
    private void Awake()
    {
        EventManager.Instance.OnUltimate += Play;
    }

    private void Play(string bomb)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(UltimateEffectUp.DOAnchorPos(UltimateEndPosition, UltimateEffectSpeed).SetEase(UltimateEffectEase));
        sequence.Join(UltimateEffectDown.DOAnchorPos(UltimateEndPosition2, UltimateEffectSpeed).SetEase(UltimateEffectEase));
        sequence.AppendCallback(()=>BombEffect(bomb));
        sequence.AppendInterval(UltimateTime);
        sequence.Append(UltimateEffectUp.DOAnchorPos(UltimateOriginPosition, UltimateEffectSpeed).SetEase(UltimateEffectEase));
        sequence.Join(UltimateEffectDown.DOAnchorPos(UltimateEndPosition2, UltimateEffectSpeed).SetEase(UltimateEffectEase));
        sequence.OnComplete(() =>
        {
            UltimateEffectUp.gameObject.SetActive(false);
            UltimateEffectDown.gameObject.SetActive(false);
        });
    }

    private void BombEffect(string bomb)
    {
        switch (bomb)
        {
            case "BO0005": 
                break;
            case "BO0007":
                UltimateEffectList[0].SetActive(true);
                UltimateEffectList[1].SetActive(true);
                break;
            case "BO00011":
                break;
            default:
                break;
        }
    }
    private void OnDisable()
    {
        DOTween.Kill(this);
        UltimateEffectUp.anchoredPosition = UltimateOriginPosition;
        UltimateEffectDown.anchoredPosition = UltimateOriginPosition2;
    }
}
