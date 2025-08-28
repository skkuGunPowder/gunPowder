using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_KillLogSlot : MonoBehaviour
{
    public RectTransform KillLogPivot;
    
    public TextMeshProUGUI KillPlayerNickname;
    public TextMeshProUGUI DeathPlayerNickname;
    public Image KillIcon;

    [Header("색상")] 
    [Tooltip("킬한 사람의 배경화면")]public Image KillBackground;
    [Tooltip("데스한 사람의 배경화면")]public Image DeathBackground;
    public ColorPalette ColorPalette;
    
    [Header("시간")] 
    [Tooltip("킬로그 등장 퇴장에 관련된 시간")] public float MoveSpeed;
    [Tooltip("킬로그가 머무르는 시간")] public float StayTime;
    [Tooltip("킬로그가 움직이는 Eaze 타입")] public Ease EaseType;
    
    [Header("킬로그가 움직이는 위치")]
    [SerializeField]private Vector2 _startPosition;
    private Vector2 _midlePosition = Vector2.zero;
    [SerializeField] private Vector2 _endPosition;
    
    
    
    [Header("아이콘")]
    public Sprite Icon;

    private void OnEnable()
    {
        KillLogPivot.anchoredPosition = _startPosition;

    }

    public void Refresh(string kill,string death, bool killTeam, bool deathTeam)
    {
        KillPlayerNickname.text = kill;
        DeathPlayerNickname.text = death;
        KillIcon.sprite = Icon;

        TeamCheck(killTeam, deathTeam);
        
        Tween_KillLog();
    }

    private void Tween_KillLog()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(KillLogPivot.DOAnchorPos(_midlePosition, MoveSpeed).SetEase(EaseType));
        sequence.AppendInterval(StayTime);
        sequence.Append(KillLogPivot.DOAnchorPos(_endPosition, MoveSpeed).SetEase(EaseType)).OnComplete(()=>
        {
            this.gameObject.SetActive(false);
        });
    }

    // 킬로그에 나온 사람이 누구 팀인가?
    private void TeamCheck(bool killTeam, bool deathTeam)
    {
        if (killTeam)
        {
            KillBackground.color = ColorPalette.ColorDictionary[EColorType.KillLogMyTeam];
        }
        else
        {
            KillBackground.color = ColorPalette.ColorDictionary[EColorType.KillLogEnemy];
        }

        if (deathTeam)
        {
            DeathBackground.color = ColorPalette.ColorDictionary[EColorType.KillLogMyTeam];
        }
        else
        {
            
            DeathBackground.color = ColorPalette.ColorDictionary[EColorType.KillLogEnemy];
        }
    }

    private void OnDisable()
    {
        DOTween.Kill(KillLogPivot);
    }
}
