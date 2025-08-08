using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

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
    private bool _hasTriggeredDeathEvents = false; // 사망 이벤트가 한 번만 발생하도록 하는 플래그

    private const float MAX_FALL_SPEED = -20f;

    // DOTween 저장용
    private Tween _moveTween;

    // 안전성 체크용
    private bool _isInitialized = false;

    public override void OnEnter()
    {
        base.OnEnter();

        // 안전성 체크
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null in PlayerFallDeadState");
            return;
        }

        // 무적
        _owner.gameObject.tag = "Immune";
        if(_owner.PhotonView.IsMine)
        {
            _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, true);
        }

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

        // DOTween 곡선 이동 - Tween 저장
        _moveTween = _owner.transform.DOPath(path, _totalDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _isGoaled = true;
                _owner.transform.position = _endPoint;
                
                // DOTween 완료 후 Rigidbody2D 속도 초기화
                if (_owner.Rigidbody2D != null)
                {
                    Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
                    velocity.x = 0f;
                    velocity.y = 0f;
                    _owner.Rigidbody2D.linearVelocity = velocity;
                }
            });

        // 모습 보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        if (playerSpriteRendererList != null)
        {
            foreach (SpriteRenderer spriteRenderer in playerSpriteRendererList)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }

        _isInitialized = true;
    }

    public override void OnExit()
    {
        base.OnExit();

        // DOTween 중단
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
            _moveTween = null;
        }

        _owner.PlayerStat.IsImmune = false;
        _owner.PlayerStat.IsFallingDead = false;
        _hasTriggeredDeathEvents = false; // 플래그 리셋

        // 무적 해제
        if (_owner.PhotonView.IsMine)
        {
            _owner.gameObject.tag = "Player";
        }
        else
        {
            _owner.gameObject.tag = "Enemy";
        }
        
        // 상태 전환 시 Rigidbody2D 속도 초기화
        if (_owner.Rigidbody2D != null)
        {
            Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
            velocity.x = 0f;
            velocity.y = 0f;
            _owner.Rigidbody2D.linearVelocity = velocity;
        }
    }

    public override void Update()
    {
        // 초기화되지 않았으면 실행하지 않음
        if (!_isInitialized)
        {
            return;
        }

        if (_isGoaled)
        {
            transform.position = _endPoint;
            
            // _isGoaled 상태에서도 속도 제한 적용
            LimitYVelocity();
            
            // 2초 대기 후 한 번만 사망 이벤트 발생
            if (!_hasTriggeredDeathEvents)
            {
                _wailTime += Time.deltaTime;
                if (_wailTime >= _waitDuration)
                {
                    _hasTriggeredDeathEvents = true; // 플래그 설정으로 중복 실행 방지
                    
                    Debug.Log($"[PlayerFallDeadState] Player {_owner.PhotonView.Owner.ActorNumber} 사망 폭발 발생 - IsMine: {_owner.PhotonView.IsMine}, IsMasterClient: {PhotonNetwork.IsMasterClient}");

                    // 안전성 체크
                    if (ExplosionPool.Instance == null || _owner.DieExplosionPrefab == null)
                    {
                        Debug.LogError("ExplosionPool or DieExplosionPrefab is null");
                        return;
                    }

                    Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
                    if (dieExplosion != null)
                    {
                        dieExplosion.transform.position = _owner.transform.position;
                        dieExplosion.Explode(true, _owner.PhotonView);
                    }

                    // 15의 데미지를 받는다.
                    _owner.PlayerStat.IsImmune = false;
                    Debug.Log($"[PlayerFallDeadState] Player {_owner.PhotonView.Owner.ActorNumber} - Calling TakeDamage, IsMine: {_owner.PhotonView.IsMine}");
                    if(_owner.PhotonView.IsMine)
                    {
                        // IsImmune을 네트워크로 동기화
                        _owner.PhotonView.RPC(nameof(_owner.RPC_SetIsImmune), RpcTarget.All, false);
                        _owner.TakeDamage(15, _owner.transform.position, _owner.GetComponent<PhotonView>().ViewID, _owner.GetComponent<PhotonView>().OwnerActorNr ,true);
                    }
                }
            }
        }
    }
    
    private void LimitYVelocity()
    {
        if (_owner.Rigidbody2D == null) return;
        
        Vector2 velocity = _owner.Rigidbody2D.linearVelocity;
        
        // 낙하 속도 제한 (음수)
        if (velocity.y < MAX_FALL_SPEED)
        {
            velocity.y = MAX_FALL_SPEED;
        }
        
        _owner.Rigidbody2D.linearVelocity = velocity;
    }
}
