using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;
    private bool _hasStartedResurrection = false; // 부활 시작 플래그

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log($"PlayerDieState {_owner.PhotonView.Owner.ActorNumber}");

        // 타이머 및 플래그 초기화
        _timer = 0f;
        _hasStartedResurrection = false;

        // 무적
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;

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
    }

    public override void Update()
    {   
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;
        
        if(_owner.PlayerStat.CurrentPlayerLife <= 0)
        {
            // 진짜 죽음
            // 파괴 요청
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() {{EProperties.IsDead.ToString(), true}});
            InstantiateDestroyManager.Instance.RequestDestroy(gameObject.GetComponent<PhotonView>().ViewID);
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
        // 모습 보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
        {
            spriteRenderer.enabled = true;
        }

        // 플레이어가 사망할 떄, 사망 폭발이 발생
        Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
        dieExplosion.transform.position = _owner.transform.position;
        dieExplosion.Explode(true, _owner.transform);
        
        Debug.Log("죽음 폭발 발생");

        // 부활 위치로 이동
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;
        
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
        // 이미 무적 상태이므로 추가 설정 불필요
        yield return new WaitForSeconds(3f);
        _owner.PlayerStat.IsImmune = false;
    }
}
