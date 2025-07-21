using UnityEngine;

public class GunPowderBezierCurve : MonoBehaviour
{
    private Vector3[] _points = new Vector3[4];

    [SerializeField]
    private float _timerMax = 0;
    [SerializeField]
    private float _timerCurrent = 0;
    [SerializeField]
    private float _speed;
    public float RandomTimerMax = 1.5f;
    public float RandomTimerMin = 0.8f;

    [Header("Init")]
    [SerializeField]
    private Transform _start;
    [SerializeField]
    private Transform _end;
    [SerializeField]
    private float _newPointDistanceFromStart;
    [SerializeField]
    private float _newPointDistanceFromEnd;
    [SerializeField]
    private float _gunPowderSpeed = 5f;

    private bool _bezierFinished = false;
    private float _followSpeed = 5f;

    private void Start()
    {
        _start = transform;
        _timerCurrent = 0f;
        // TODO: 경우에 따라 달라질듯
        // 날 때린 플레이어, 가까이 있는 플레이어 등 상태에 따라 달라짐
        _end = GameObject.FindGameObjectWithTag("Player").transform;


        _speed = _gunPowderSpeed;
        Init(_start, _end, _speed, _newPointDistanceFromStart, _newPointDistanceFromEnd);
    }

    public void Init(Transform start, Transform end, float speed, float newPointDistanceFromStart, float newPointDistanceFromEnd)
    {
        _speed = speed;

        // 도착할 시간 랜덤
        _timerMax = Random.Range(RandomTimerMin, RandomTimerMax);

        // 시작점
        _points[0] = start.position;

        // 시작점을 기준으로 랜덤 포인트 지정
        _points[1] = start.position + 
            newPointDistanceFromStart * Random.Range(-1.0f, 1.0f) * start.right +       // X (좌, 우 전체)
            newPointDistanceFromStart * Random.Range(-0.15f, 1.0f) * start.up +          // Y (아래쪽 조금, 위쪽 전체)
            newPointDistanceFromStart * Random.Range(-1.0f, -0.8f) * start.forward;      // Z (뒤 쪽만)

        // 끝점을 기준으로 랜덤 포인트 지정
        _points[2] = end.position + 
            newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.right +       // X (좌, 우 전체)
            newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.up +          // Y (아래, 위 전체체)
            newPointDistanceFromEnd * Random.Range(0.8f, 1.0f) * end.forward;      // Z (앞 쪽만)

        // 끝점
        _points[3] = end.position;

        transform.position = start.position;
    }

    private void Update()
    {
        if (!_bezierFinished)
        {
            if (_timerCurrent > _timerMax)
            {
                _bezierFinished = true;
                return;
            }

            _points[3] = GameObject.FindGameObjectWithTag("Player").transform.position;
            _timerCurrent += Time.deltaTime * _speed;
            transform.position = BezierCurve.BezierCurve3D(_points[0], _points[3], _points[1], _points[2], _timerCurrent/_timerMax);
        }
        else
        {
            // 베지어 끝난 후, 플레이어를 향해 직선 이동
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            float followSpeed = _speed * _followSpeed; // 직선 이동 속도 (베지어보다 약간 빠르게)
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * followSpeed * Time.deltaTime;
        }
    }
}
