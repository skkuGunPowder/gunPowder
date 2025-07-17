using System.Collections.Generic;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class GunPowder : MonoBehaviour
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


    private void Start()
    {
        _start = transform;
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
        // TODO: 폭탄 타입(공격 타입?) 에 따라서 달라질 수 있음
        // 기본 공격은 공격을 한 플레이어에게 흡수되는 형태
        // 폭탄 타입에 따라 바로 흡수되는게 아니라 바닥에 흩뿌려지고 일정 범위 안에 오면 흡수되는 형태
        if(_timerCurrent > _timerMax)
        {
            return;
        }

        _points[3] = GameObject.FindGameObjectWithTag("Player").transform.position;

        // 경과 시간 계산
        _timerCurrent += Time.deltaTime * _speed;

        // 베지어 곡선으로 X,Y,Z 좌표 얻기
        transform.position = BezierCurve.BezierCurve3D(_points[0], _points[3], _points[1], _points[2], _timerCurrent/_timerMax);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어에게 흡수되기
        if(other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
