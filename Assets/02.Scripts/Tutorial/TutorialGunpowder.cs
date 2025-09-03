using System.Runtime.InteropServices;
using UnityEngine;

public class TutorialGunpowder : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private Transform _target;
    private Vector2[] _points = new Vector2[4];
    private float _speed = 5f;
    private float _timerMax = 1.5f;
    private float _randomTimerMin = 1f;
    private float _randomTimerMax = 1.8f;
    private float _newPointDistanceFromStart = 3f;
    private float _newPointDistanceFromEnd = 3f;
    private float _timerCurrent = 0f;
    private bool _bezierFinished = false;
    private float _followSpeed = 10f;
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (player != null)
        {
            _target = player.transform;
        }

        _timerMax = Random.Range(_randomTimerMin, _randomTimerMax);

        Transform start = transform;
        Transform end = _target;

        _points[0] = transform.position;

        // 시작점을 기준으로 랜덤 포인트 지정
        _points[1] = start.position +
            _newPointDistanceFromStart * Random.Range(-1.0f, 1.0f) * start.right +       // X (좌, 우 전체)
            _newPointDistanceFromStart * Random.Range(-0.15f, 1.0f) * start.up;          // Y (아래쪽 조금, 위쪽 전체)

        // 끝점을 기준으로 랜덤 포인트 지정
        _points[2] = end.position +
            _newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.right +       // X (좌, 우 전체)
            _newPointDistanceFromEnd * Random.Range(-1.0f, 1.0f) * end.up;            // Y (아래, 위 전체체)

        // 끝점
        _points[3] = end.position;
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

            _points[3] = _target.position;
            _timerCurrent += Time.deltaTime * _speed;
            transform.position = BezierCurve.BezierCurve2D(_points[0], _points[3], _points[1], _points[2], _timerCurrent / _timerMax);
        }
        else
        {
            // 베지어 끝난 후, 플레이어를 향해 직선 이동
            float followSpeed = _speed * _followSpeed; // 직선 이동 속도 (베지어보다 약간 빠르게)
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * followSpeed * Time.deltaTime;
        }

        if(Vector2.Distance(transform.position, _target.position) < 0.1f)
        {
            _target.GetComponent<Player>().PlayerStat.IncreaseGunPowderCount(1);
            Destroy(gameObject);
        }
    }
}
