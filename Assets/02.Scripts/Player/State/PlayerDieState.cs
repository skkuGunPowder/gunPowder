using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using DG.Tweening;

public class PlayerDieState : PlayerBaseState
{
    private float _timer = 0f;
    private bool _hasStartedResurrection = false; // 부활 시작 플래그
    private bool _hasRequestedDestroy = false; // 파괴 요청 플래그

    public override void OnEnter()
    {
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

        // 사망 효과
        DieEffect();


        // 타이머 및 플래그 초기화
        _timer = 0f;
        _hasStartedResurrection = false;
        _hasRequestedDestroy = false;

        // 무적
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;

        // 플레이어가 사망할 떄, 사망 폭발이 발생
        Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
        dieExplosion.transform.position = _owner.transform.position;
        dieExplosion.Explode(true, _owner.PhotonView);

        // 모습 안보이게
        
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        if (playerSpriteRendererList != null)
        {
            foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }
        }

        // 플레이어 사망 사운드 재생
        SoundManager.Instance.PlayLocalRandomSound("PlayerDeath", transform, 1, 3);
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

        // 모습 보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        if (playerSpriteRendererList != null)
        {
            foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }
    }

    public override void Update()
    {   
        // null 체크
        if (_owner == null || _owner.PhotonView == null)
        {
            return;
        }

        
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
        }
        else
        {
            // 부활 지점에서 몇초 후 부활
            _timer += Time.deltaTime;
            
            if(_timer < 3f)
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
            return;
        }


        // 부활 위치로 이동
        DOTween.Kill(_owner.transform);
        _owner.transform.position = GameManager.Instance.ResurrectPoint.position;

        // 모습 보이게
        List<SpriteRenderer> playerSpriteRendererList = _owner.PlayerStat.MySpriteREndererList;
        if (playerSpriteRendererList != null)
        {
            foreach(SpriteRenderer spriteRenderer in playerSpriteRendererList)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }
        
        // 플레이어 부활
        _owner.ResurrectPlayer();
        
        // 무적 코루틴 시작
        StartCoroutine(ImmuneCoroutine());
        
        // 상태 전환 - 네트워크 동기화 사용
        if (_owner.PhotonView.IsMine)
        {
            SyncStateChange<PlayerIdleState>();
        }
        else
        {
            // 다른 클라이언트에서는 직접 상태 변경
            _playerFSM.ChangeState<PlayerIdleState>();
        }
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

    private void DieEffect()
    {
        float power = 20f;
        foreach(GameObject diePart in _owner.HeadPartList)
        {
            AddForceToDiePart(diePart, new Vector2(0, 1).normalized, power);
        }

        foreach(GameObject diePart in _owner.BodyPartList)
        {
            AddForceToDiePart(diePart, new Vector2(0, -1).normalized, power);
        }

        foreach(GameObject diePart in _owner.LeftArmPartList)
        {
            AddForceToDiePart(diePart, new Vector2(-1, 1).normalized, power);
        }

        foreach(GameObject diePart in _owner.LeftLegPartList)
        {
            AddForceToDiePart(diePart, new Vector2(-1, -1).normalized, power);
        }

        foreach(GameObject diePart in _owner.RightArmPartList)
        {
            AddForceToDiePart(diePart, new Vector2(1, 1).normalized, power);
        }

        foreach(GameObject diePart in _owner.RightLegPartList)
        {
            AddForceToDiePart(diePart, new Vector2(1, -1).normalized, power);
        }
        
    }

    private void AddForceToDiePart(GameObject diePart, Vector2 direction, float power)
    {
        Rigidbody2D rigidbody2D = diePart.GetComponent<Rigidbody2D>();
        if(rigidbody2D != null)
        {
            diePart.SetActive(true);
            rigidbody2D.AddForce(direction * power, ForceMode2D.Impulse);
        }
    }
}
