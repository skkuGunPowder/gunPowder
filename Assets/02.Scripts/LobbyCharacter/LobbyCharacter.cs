using System;
using UnityEngine;
using DG.Tweening;
public class LobbyCharacter : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector2 _startPos;
    [SerializeField] private Vector2 _endPos;
    [SerializeField] private float _duration;
    
    private void Start()
    {
        Move();
    }

    private void Move()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_rectTransform.DOAnchorPos(_endPos, _duration));
        sequence.Append(_rectTransform.DOAnchorPos(_startPos, _duration));
        sequence.SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
