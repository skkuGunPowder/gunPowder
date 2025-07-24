using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class PlayerFallDeadState : PlayerBaseState
{
    private bool _isLeft = false;

    private Vector3 _middlePoint;
    private Vector3 _endPoint;
    private Vector3 _startPoint;
    private float _curveHeight = 1f;

    private bool _isGoaled = false;
    private float _totalDuration = 2.0f; // 전체 이동 시간
    private float _waitDuration = 2.0f; // 대기 시간
    private float _wailTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("PlayerFallDeadState");

        _wailTime = 0f;
        _owner.PlayerStat.IsFallingDead = true;
        _isGoaled = false;

        // 낙사 판정 구간에 들어가면 부활지점으로 이동해야 한다. 
        // 좌측기준으로 하면 좌측 최하단 -> 좌측 상단 중단점 -> 맵 중앙 상단으로 이동
        if (transform.position.x <= 0)
        {
            // 좌측 최하단으로 이동
            _isLeft = true;
            _startPoint = GameManager.Instance.FallDeadStartPointList[0].position;
        }
        else
        {
            // 우측 최하단으로 이동
            _isLeft = false;
            _startPoint = GameManager.Instance.FallDeadStartPointList[1].position;
        }
        transform.position = _startPoint;

        // 중단점 설정
        _middlePoint = GameManager.Instance.FallDeadPathList[_isLeft ? 0 : 1].position;
        // 도착점 설정
        _endPoint = GameManager.Instance.ResurrectPoint.position;

        // 경로 설정 (좌우 반전 적용)
        Vector3[] path = new Vector3[] { _startPoint, _middlePoint, _endPoint };

        // DOTween 곡선 이동
        _owner.transform.DOPath(path, _totalDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                _isGoaled = true;
                _owner.transform.position = _endPoint;
            });
    }

    public override void OnExit()
    {
        base.OnExit();
        _owner.PlayerStat.IsFallingDead = false;
    }

    public override void MineUpdate()
    {
        if (_isGoaled)
        {
            transform.position = _endPoint;
            // 2초 대기
            _wailTime += Time.deltaTime;
            if (_wailTime >= _waitDuration)
            {
                // 사망 폭발 발생
                // PhotonNetwork.Instantiate("DieExplosion", transform.position, Quaternion.identity);
                
                // 15의 데미지를 받는다.
                _owner.TakeDamage(15, _owner.transform.position, _owner.GetComponent<PhotonView>().ViewID, true);
                
                // 피격 상태로 전환
                _playerFSM.ChangeState<PlayerDamagedState>();
            }
        }
    }
}
