using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class CustomPlayerTransformView : MonoBehaviourPun, IPunObservable
{
    [Header("Synchronization Settings")]
    public bool synchronizePosition = true;
    public bool synchronizeRotation = false; // 2D 게임이므로 회전은 불필요
    public bool synchronizeScale = false;

    [Header("Interpolation Settings")]
    public float interpolationSpeed = 15f; // 더 빠른 보간
    public float extrapolationSpeed = 10f; // 외삽 속도
    public bool useExtrapolation = true; // 외삽 사용
    public float teleportDistance = 5f; // 텔레포트 거리

    [Header("Network Settings")]
    public float networkUpdateRate = 20f; // 네트워크 업데이트 빈도

    private Vector3 networkPosition;
    private Vector3 lastPosition;
    private float lastNetworkTime;
    private bool firstTake = true;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            // 네트워크 위치로 부드럽게 보간
            Vector3 targetPosition = networkPosition;
            
            if (useExtrapolation)
            {
                // 외삽 적용 (네트워크 지연 보정)
                float timeSinceUpdate = Time.time - lastNetworkTime;
                Vector3 velocity = (networkPosition - lastPosition) / Time.deltaTime;
                targetPosition += velocity * timeSinceUpdate;
            }

            // 텔레포트 체크
            if (Vector3.Distance(transform.position, targetPosition) > teleportDistance)
            {
                transform.position = targetPosition;
            }
            else
            {
                // 부드러운 보간
                transform.position = Vector3.Lerp(transform.position, targetPosition, 
                    interpolationSpeed * Time.deltaTime);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내 플레이어: 위치 전송
            if (synchronizePosition)
            {
                stream.SendNext(transform.position);
            }
        }
        else
        {
            // 다른 플레이어: 위치 수신
            if (synchronizePosition)
            {
                lastPosition = networkPosition;
                networkPosition = (Vector3)stream.ReceiveNext();
                lastNetworkTime = Time.time;

                if (firstTake)
                {
                    transform.position = networkPosition;
                    firstTake = false;
                }
            }
        }
    }

    // 속도 정보를 외부에서 설정할 수 있도록
    public void SetNetworkVelocity(Vector3 velocity)
    {
        if (!photonView.IsMine)
        {
            // 속도 기반 외삽 개선
            networkPosition += velocity * Time.deltaTime;
        }
    }
} 