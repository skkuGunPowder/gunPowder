using Photon.Pun.UtilityScripts;
using UnityEngine;

public class GunPowderRelease : MonoBehaviour
{
    public int maxBounce = 3;      // 최대 튕김 횟수
    public float xzForce = 3f;     // XZ 평면 힘 (멀리 튀게)
    public float yForce = 5f;      // Y축 힘 (높이 튕게)
    public float gravity = 9.8f;   // 중력

    private Vector3 direction;     // 이동 방향
    private int currentBounce = 0;
    private bool isGrounded = true;

    private float maxHeight;
    private float currentHeight;

    private BoxCollider _collider;

    public float GroundCheckDistance; // 바닥 체크용 Raycast 거리
    public LayerMask GroundLayer;            // 바닥 레이어 지정(Inspector에서 할당)

    public float GroundCheckOffset;
    public float YForceOffset;

    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        // XZ 평면 랜덤 방향
        Vector2 randXZ = Random.insideUnitCircle.normalized * xzForce;
        direction = new Vector3(randXZ.x, 0, 0);
        GroundCheckDistance = _collider.size.y * 0.5f * transform.localScale.y + 0.05f;

        currentHeight = Random.Range(yForce - YForceOffset, yForce + YForceOffset);
        maxHeight = currentHeight;
        Initialize(direction);
    }

    void Update()
    {
        if (!isGrounded)
        {
            // Y축(상하) 이동
            currentHeight += -gravity * Time.deltaTime;
            transform.position += new Vector3(direction.x, currentHeight, direction.z) * Time.deltaTime;

            CheckGroundHit();
        }
    }

    void Initialize(Vector3 _direction)
    {
        isGrounded = false;
        maxHeight /= 1.5f;
        direction = _direction;
        currentHeight = maxHeight;
        currentBounce++;
    }

    void CheckGroundHit()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, GroundCheckDistance, GroundLayer))
        {
            if (transform.position.y - hit.point.y < transform.localScale.y + GroundCheckOffset && currentHeight < 0)
            {
                // 콜라이더 절반 높이만큼 위로 올려서 바닥에 닿게
                BoxCollider col = GetComponent<BoxCollider>();
                float halfHeight = col != null ? GroundCheckDistance : 0.5f;

                Vector3 pos = transform.position;
                pos.y = hit.point.y + halfHeight;
                transform.position = pos;

                if (currentBounce < maxBounce)
                {
                    Initialize(direction / 1.1f);
                }
                else
                {
                    isGrounded = true;
                }
            }
        }
    }
}
