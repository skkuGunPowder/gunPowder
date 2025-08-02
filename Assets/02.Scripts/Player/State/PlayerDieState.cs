using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;
    private bool _hasStartedResurrection = false; // 부활 시작 플래그
    private bool _hasRequestedDestroy = false; // 파괴 요청 플래그
    private bool _hasInitialized = false; // 초기화 완료 플래그

    public override void OnEnter()
    {
        // 이미 초기화되었다면 중복 실행 방지
        if (_hasInitialized)
        {
            return;
        }

        // base.OnEnter()를 먼저 호출하여 _owner 초기화
        base.OnEnter();
        
        // null 체크
        if (_owner == null)
        {
            return;
        }

        if (_owner.PhotonView == null)
        {
            Debug.LogError("[PlayerDieState] PhotonView is null in OnEnter");
            return;
        }


        // 타이머 및 플래그 초기화
        _timer = 0f;
        _hasStartedResurrection = false;
        _hasRequestedDestroy = false;
        _hasInitialized = true; // 초기화 완료 표시

        // 무적
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;

        // 플레이어가 사망할 떄, 사망 폭발이 발생
        Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
        dieExplosion.transform.position = _owner.transform.position;
        dieExplosion.Explode(true, _owner.PhotonView);

        // 모습 안보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
        {
            spriteRenderer.enabled = false;
        }
        
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;
    }

    public override void OnExit()
    {
        // null 체크
        if (_owner == null || _owner.PhotonView == null)
        {
            return;
        }
        
        base.OnExit();

        // 무적 해제
        if(_owner.PhotonView.IsMine)
        {
            _owner.gameObject.tag = "Player";
        }
        else
        {
            _owner.gameObject.tag = "Enemy";
        }
        _owner.PlayerStat.IsImmune = false;
        
        // 초기화 플래그 리셋
        _hasInitialized = false;
    }

    public override void Update()
    {   
        // null 체크
        if (_owner == null || _owner.PhotonView == null)
        {
            return;
        }

        // 초기화되지 않았다면 실행하지 않음
        if (!_hasInitialized)
        {
            return;
        }

        
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;
        
        if(_owner.PlayerStat.CurrentPlayerLife <= 0)
        {
            
            // 진짜 죽음
            // 파괴 요청
            if (_hasRequestedDestroy)
            {
                return;
            }
            _hasRequestedDestroy = true;

            
            // 플레이어가 자신의 GameObject를 제거하거나, MasterClient에게 요청
            if (_owner.PhotonView.IsMine)
            {
                
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
                {
                    {EProperties.IsDead.ToString(), true},
                    {EProperties.Kill.ToString(), _owner.PlayerStat.TotalKillCount},
                    {EProperties.Damage.ToString(), _owner.PlayerStat.TotalDamage}
                    
                });
                
                // 다른 플레이어의 GameObject는 MasterClient에게 요청
                PhotonNetwork.Destroy(_owner.gameObject);
            }
            else
            {
            }
        }
        else
        {
            
            // 부활 지점에서 몇초 후 부활
            _timer += Time.deltaTime;
            
            if(_timer < 2f)
            {
                return;
            }

            // 부활 로직은 한 번만 실행
            if (!_hasStartedResurrection)
            {
                _hasStartedResurrection = true;
                StartResurrection();
            }

        }
    }

    /// <summary>
    /// 부활 처리
    /// </summary>
    private void StartResurrection()
    {
        // null 체크
        if (_owner == null || _owner.PhotonView == null)
        {
            Debug.LogError("[PlayerDieState] _owner or PhotonView is null in StartResurrection");
            return;
        }

        
        // 부활 위치로 이동
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;

        // 모습 보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
        {
            spriteRenderer.enabled = true;
        }
        
        // 플레이어 부활
        _owner.ResurrectPlayer();
        
        // 무적 코루틴 시작
        StartCoroutine(ImmuneCoroutine());
        
        // 상태 전환
        _playerFSM.ChangeState<PlayerIdleState>();
    }

    /// <summary>
    /// 3초 동안 무적
    /// </summary>
    /// <returns></returns>
    private IEnumerator ImmuneCoroutine()
    {
        // null 체크
        if (_owner == null || _owner.PhotonView == null)
        {
            yield break;
        }

        
        // 이미 무적 상태이므로 추가 설정 불필요
        yield return new WaitForSeconds(3f);
        
        _owner.PlayerStat.IsImmune = false;
    }
}
