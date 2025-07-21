using UnityEngine;

public class GunPowderTrigger : MonoBehaviour
{
    private CircleCollider2D _collider;
    private float _timer = 0f;
    private float _colliderOnTime = 3f;

    void OnEnable()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 베지어 곡선 이동 활성화
            gameObject.GetComponentInParent<GunPowderBezierCurve>().enabled = true;

            // 2. 상위 콜라이더를 트리거로 전환
            if (gameObject.TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCollider))
                boxCollider.isTrigger = true;

            // 3. (선택) 이 콜라이더는 더 이상 감지하지 않게 비활성화
            gameObject.SetActive(false); // 또는 collider.enabled = false;
        }
    }
    
}
