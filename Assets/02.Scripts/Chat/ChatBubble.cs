using System;
using UnityEngine;
using TMPro;
using DG.Tweening; 
using System.Collections; 

public class ChatBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText; 
    [SerializeField] private float lifeTime = 3.0f;
    
    // 애니메이션 상수 정의
    private const float ANIMATION_DURATION = 0.1f;
    private const float START_SCALE = 0f;
    private const float END_SCALE = 1f;

    private Sequence _seq;
    public void Setup(string text)
    {
        messageText.text = text;
        
        // 1. 초기 크기 설정
        transform.localScale = Vector3.one * START_SCALE; 
        
        _seq =  DOTween.Sequence().SetUpdate(true);
        _seq.Append(transform.DOScale(END_SCALE, ANIMATION_DURATION).SetEase(Ease.OutBack));
        _seq.AppendInterval(lifeTime);
        _seq.Append(transform.DOScale(0f, ANIMATION_DURATION).SetEase(Ease.InSine));
        _seq.AppendInterval(ANIMATION_DURATION);
        _seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDisable()
    {
        _seq.Kill();
        _seq = null;
        Destroy(gameObject);
    }
}