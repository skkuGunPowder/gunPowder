using System;
using UnityEngine;
using DG.Tweening;
public class UI_ResultSlotDotween : MonoBehaviour
{
    [Header("Dotween 적용할 오브젝트")]
    [Tooltip("이 게임 오브젝트")] public RectTransform MyRectTransform;
    
    [Header("For Dotween")]
    public float MoveSpeed = 0.2f;
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public Ease EaseType;
    // Dotween 시작
    private void OnEnable()
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(MyRectTransform.DOAnchorPos(EndPosition, MoveSpeed)).SetEase(EaseType);
    }
    
    private void OnDisable()
    {
        MyRectTransform.anchoredPosition = StartPosition;
    }
}
