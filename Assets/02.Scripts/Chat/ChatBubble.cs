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

    public void Setup(string text)
    {
        messageText.text = text;
        
        // 1. 초기 크기 설정
        transform.localScale = Vector3.one * START_SCALE; 
        
        // 2. 등장 애니메이션 (0.1초)
        transform.DOScale(END_SCALE, ANIMATION_DURATION)
            .SetEase(Ease.OutBack);
                 
        // 3초 뒤 DestroyBubble 호출
        // [수정] Invoke 대신 Coroutine 시작을 위해 StartCoroutine 사용
        StartCoroutine(DestroyAfterDelay(lifeTime));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // [추가] 파괴 애니메이션 시작
        yield return StartCoroutine(DestroyAnimation());
        
        // [수정] 애니메이션 완료 후 오브젝트 파괴
        Destroy(gameObject);
    }
    private IEnumerator DestroyAnimation()
    {
        // DOTween 애니메이션 실행
        // 크기를 0으로 줄이는 Tween
        transform.DOScale(0f, ANIMATION_DURATION)
            .SetEase(Ease.InSine); // 부드럽게 사라지는 Ease 설정
        
        // 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(ANIMATION_DURATION);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}