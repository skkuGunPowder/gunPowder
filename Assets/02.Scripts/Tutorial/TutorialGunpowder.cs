using System.Runtime.InteropServices;
using UnityEngine;
using Photon.Pun;

public class TutorialGunpowder : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private Transform _target;
    private int _targetViewId = 0;
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
    private bool _hasPlayedDestroyVFX;

    public GameObject VFXPrefab;
    public GameObject HealVFXPrefab;
    
    private void Start()
    {
        Init();
    }

    /// <summary>
    /// ViewID를 통해 타겟 플레이어를 설정
    /// </summary>
    public void SetTargetByViewId(int viewId)
    {
        _targetViewId = viewId;
        FindTargetByViewId();
    }

    private void FindTargetByViewId()
    {
        if (_targetViewId == 0)
        {
            Destroy(gameObject);
            return;
        }

        PhotonView targetView = PhotonView.Find(_targetViewId);
        if (targetView != null && targetView.gameObject != null)
        {
            _target = targetView.transform;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Init()
    {
        // ViewID가 설정되어 있으면 ViewID로 찾기, 없으면 태그로 찾기 (하위 호환성)
        if (_targetViewId != 0)
        {
            FindTargetByViewId();
            if (_target == null)
            {
                return; // FindTargetByViewId에서 이미 Destroy 호출됨
            }
        }
        else
        {
            // 1. Player 오브젝트 찾기 (하위 호환성)
            GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (player == null)
            {
                Destroy(gameObject);
                return;
            }

            // 4. 타겟 설정
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
        // _target이 null이면 다시 찾기 시도
        if (_target == null)
        {
            if (_targetViewId != 0)
            {
                // ViewID로 다시 찾기 시도
                FindTargetByViewId();
                if (_target == null)
                {
                    return; // FindTargetByViewId에서 이미 Destroy 호출됨
                }
            }
            else
            {
                // 하위 호환성: 태그로 찾기
                GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
                if (player != null)
                {
                    _target = player.transform;
                }
                else
                {
                    // 플레이어를 찾을 수 없으면 오브젝트 파괴
                    Destroy(gameObject);
                    return;
                }
            }
        }

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

    private void OnDisable()
    {
        // 파괴 시 로컬에서 VFX 재생
        if (_hasPlayedDestroyVFX)
        {
            return;
        }
        _hasPlayedDestroyVFX = true;

        if (VFXPrefab != null)
        {
            FollowVFX vfx = VFXPool.Instance.Get(VFXPrefab.name) as FollowVFX;
            if (vfx != null)
            {
                if (_target != null && _target.gameObject.activeInHierarchy)
                {
                    vfx.PlayAttached(_target);
                }
            }
        }
        
        if (HealVFXPrefab != null)
        {
            FollowVFX vfx = VFXPool.Instance.Get(HealVFXPrefab.name) as FollowVFX;
            if (vfx != null)
            {
                if (_target != null && _target.gameObject.activeInHierarchy)
                {
                    // 타겟 위쪽으로 호를 그려서 랜덤 위치에 생성
                    vfx.PlayAttachedWithArcOffset(_target, arcRadius: 1.5f, arcAngleRange: 90f);
                }
            }
        }
    }
}
