using Photon.Pun;
using UnityEngine;

public class PlayerDropDeadState : PlayerBaseState
{
    private bool _isLeft = false;

    private Vector3 _middlePoint;
    private Vector3 _endPoint;
    private float _moveSpeed = 5f;
    private Vector3 _startPoint;
    private float _progress1 = 0f;
    private float _progress2 = 0f;
    private float _moveDuration1 = 1.0f; // 시작→중단
    private float _moveDuration2 = 1.0f; // 중단→도착
    private float _curveHeight = 1f;

    private bool _isGoaled = false;
    private bool _isFirstPhase = true;

    private float _totalProgress = 0f;
    private float _totalDuration = 2.0f; // 전체 이동 시간
    private float _waitDuration = 2.0f; // 대기 시간
    private float _wailTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();

        // 낙사 판정 구간에 들어가면 부활지점으로 이동해야 한다. 
        // 좌측기준으로 하면 좌측 최하단 -> 좌측 상단 중단점 -> 맵 중앙 상단으로 이동
        if(transform.position.x <= 0)
        {
            // 좌측 최하단으로 이동
            _isLeft = true;
            _startPoint = new Vector3(-5, -5, 0);
        }
        else
        {
            // 우측 최하단으로 이동
            _isLeft = false;
            _startPoint = new Vector3(5, -5, 0);
        }
        transform.position = _startPoint;

        // 중단점 설정
        _middlePoint = new Vector3(5, 10, 0);
        // 도착점 설정
        _endPoint = new Vector3(0, 5, 0);

        _progress1 = 0f;
        _progress2 = 0f;
        _isFirstPhase = true;
        _totalProgress = 0f;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void MineUpdate()
    {
        if (!_isGoaled)
        {
            _totalProgress += Time.deltaTime / _totalDuration;
            float t = Mathf.Clamp01(_totalProgress);

            if (t < 0.5f)
            {
                // 시작→중단 (0~0.5)
                float localT = t / 0.5f;
                float curve = Mathf.Sin(localT * Mathf.PI);
                float x = Mathf.Lerp(_startPoint.x, _middlePoint.x, localT) + curve * _curveHeight;
                float y = Mathf.Lerp(_startPoint.y, _middlePoint.y, localT) + curve * _curveHeight;
                x = _isLeft ? -x : x;
                _owner.transform.position = new Vector3(x, y, 0);
            }
            else
            {
                // 중단→도착 (0.5~1)
                float localT = (t - 0.5f) / 0.5f;
                float curve = Mathf.Sin(localT * Mathf.PI);
                float x = Mathf.Lerp(_middlePoint.x, _endPoint.x, localT) + curve * _curveHeight;
                float y = Mathf.Lerp(_middlePoint.y, _endPoint.y, localT) + curve * _curveHeight;
                x = _isLeft ? -x : x;
                _owner.transform.position = new Vector3(x, y, 0);

                // 도착
                if (t >= 1f)
                {
                    _isGoaled = true;
                    _owner.transform.position = _endPoint;
                }
            }
        }

        if(_isGoaled)
        {
            transform.position = _endPoint;
            // 2초 대기
            _wailTime += Time.deltaTime;
            if(_wailTime >= _waitDuration)
            {
                // 사망 폭발 발생
                // PhotonNetwork.Instantiate("DieExplosion", transform.position, Quaternion.identity);
                
                // 15의 건파우더 낙출
                _owner.ReleaseGunPowder(transform.position, 15, 30, 1.0f, true);
                
                // 피격 상태로 전환
                _playerFSM.ChangeState<PlayerDamagedState>();
            }
        }
    }
}
