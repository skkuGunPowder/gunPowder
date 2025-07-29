using Photon.Pun;
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
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            GunPowder gunPowder = gameObject.GetComponentInParent<GunPowder>();
            if (gunPowder != null)
            {
                PhotonView targetView = collision.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    gunPowder.photonView.RPC(nameof(GunPowder.SetTarget), RpcTarget.All, targetView.ViewID);
                }
            }

            // 이 콜라이더는 더 이상 감지하지 않게 비활성화
            _collider.enabled = false;

            // 1. 베지어 곡선 이동 활성화
            gameObject.GetComponentInParent<GunPowderBezierCurve>().enabled = true;

            // 2. 상위 콜라이더를 트리거로 전환
            gameObject.GetComponentInParent<BoxCollider2D>().isTrigger = true;

            gameObject.SetActive(false);
        
        }
    }
    
}
