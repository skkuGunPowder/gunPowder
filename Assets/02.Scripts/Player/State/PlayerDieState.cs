using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// 플레이어 사망 상태 클래스
/// 
/// 역할:
/// - 플레이어 사망 시 처리 (시각적 효과, 사운드, 폭발)
/// - 생명 수에 따른 부활 또는 완전 사망 처리
/// - 사망 시 신체 부위별 물리 효과 적용
/// 
/// 동작 방식:
/// 1. 사망 시 무적 상태 전환 및 시각적 숨김
/// 2. 신체 부위별 물리 효과 적용 (흩어지는 효과)
/// 3. 사망 폭발 효과 및 사운드 재생
/// 4. 생명 수 확인 후 부활 또는 관전 상태로 전환
/// 5. 부활 시 일정 시간 무적 상태 유지
/// </summary>
public class PlayerDieState : PlayerBaseState
{
    // 시간 관련 상수
    private const float RESURRECTION_DELAY_TIME = 3f;      // 부활 대기 시간 (초)
    private const float IMMUNE_DURATION_AFTER_RESURRECTION = 3f; // 부활 후 무적 시간 (초)
    
    // // 사망 효과 관련 상수
    // private const float DIE_EFFECT_FORCE = 30f;            // 사망 시 신체 부위에 가해지는 힘
    //
    // // 방향 벡터 상수들
    // private static readonly Vector2 HEAD_DIRECTION = new Vector2(0, 1).normalized;       // 머리 부위 방향 (위)
    // private static readonly Vector2 BODY_DIRECTION = new Vector2(0, -1).normalized;     // 몸통 부위 방향 (아래)
    // private static readonly Vector2 LEFT_ARM_DIRECTION = new Vector2(-1, 1).normalized; // 왼팔 방향 (왼쪽 위)
    // private static readonly Vector2 LEFT_LEG_DIRECTION = new Vector2(-1, -1).normalized;// 왼다리 방향 (왼쪽 아래)
    // private static readonly Vector2 RIGHT_ARM_DIRECTION = new Vector2(1, 1).normalized; // 오른팔 방향 (오른쪽 위)
    // private static readonly Vector2 RIGHT_LEG_DIRECTION = new Vector2(1, -1).normalized;// 오른다리 방향 (오른쪽 아래)
    //
    // 상태 변수들
    private float _dieTimer = 0f;                          // 사망 후 경과 시간 타이머
    private bool _hasStartedResurrection = false;          // 부활 시작 플래그
    private bool _hasRequestedDestroy = false;             // 파괴 요청 플래그
    private bool _effectInitial = false;                   // 파괴 이펙트 한번만
    /// <summary>
    /// 사망 상태 진입 시 초기화
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        
        // 안전성 검사
        if (!ValidateOwnerAndComponents())
        {
            return;
        }
        
        // 상태 초기화
        InitializeDeathState();

        // 무적 상태 설정
        SetImmuneState();
        
        if (!HasNoMoreLives())
        {
            ExecuteDeath();
        }
        // 히트 이벤트 해제 (사망 중 추가 피격 방지)
        _owner.OnHit -= HandleHit;
        
    }

    /// <summary>
    /// 사망 상태 종료 시 정리 작업
    /// </summary>
    public override void OnExit()
    {
        // 안전성 검사
        if (!ValidateOwnerAndComponents())
        {
            return;
        }
        
        base.OnExit();

        // 태그 복원 (무적 상태는 ImmuneCoroutine에서 별도 관리)
        RestorePlayerTag();

        // 플레이어 모습 다시 보이게 설정
        SetSpriteRenderersVisibility(true);

    }

    /// <summary>
    /// 사망 상태의 메인 업데이트 로직
    /// </summary>
    public override void MineUpdate()
    {   
        // 안전성 검사
        if (!ValidateOwnerAndComponents())
        {
            return;
        }

        // 생명 수에 따른 처리 분기
        if (HasNoMoreLives())
        {
            HandlePermanentDeath();
        }
        else
        {
            HandleResurrectionProcess();
        }
    }

    /// <summary>
    /// 부활 처리
    /// </summary>
    private void StartResurrection()
    {
        // 안전성 검사
        if (!ValidateOwnerAndComponents())
        {
            return;
        }

        // 부활 위치로 이동
        MoveToResurrectionPoint();

        // 플레이어 모습 다시 보이게 설정
        SetSpriteRenderersVisibility(true);
        
        // 플레이어 상태 부활
        _owner.ResurrectPlayer();
        
        // 부활 후 무적 시간 시작
        PostResurrectionImmuneCoroutine().Forget();
        
        // Idle 상태로 전환
        SyncStateChange<PlayerIdleState>();
    }

    /// <summary>
    /// 부활 후 무적 시간 관리
    /// </summary>
    private async UniTask PostResurrectionImmuneCoroutine()
    {
        // 안전성 검사
        if (!ValidateOwnerAndComponents())
        {
            return;
        }

        // 부활 후 무적 시간 대기
        await UniTask.WaitForSeconds(IMMUNE_DURATION_AFTER_RESURRECTION);
        
        // 무적 상태 해제
        if (_owner != null && _owner.PlayerStat != null)
        {
            _owner.PlayerStat.IsImmune = false;
        }
    }

    // /// <summary>
    // /// 신체 부위별 사망 효과 적용 (각 부위를 다른 방향으로 흩어뜨림)
    // /// </summary>
    // private void ApplyBodyPartsDeathEffect()
    // {
    //     // 각 신체 부위별로 지정된 방향으로 힘 적용
    //     ApplyForceToBodyParts(_owner.HeadPartList, HEAD_DIRECTION);
    //     ApplyForceToBodyParts(_owner.BodyPartList, BODY_DIRECTION);
    //     ApplyForceToBodyParts(_owner.LeftArmPartList, LEFT_ARM_DIRECTION);
    //     ApplyForceToBodyParts(_owner.LeftLegPartList, LEFT_LEG_DIRECTION);
    //     ApplyForceToBodyParts(_owner.RightArmPartList, RIGHT_ARM_DIRECTION);
    //     ApplyForceToBodyParts(_owner.RightLegPartList, RIGHT_LEG_DIRECTION);
    // }
    //
    // /// <summary>
    // /// 특정 신체 부위 리스트에 힘 적용
    // /// </summary>
    // private void ApplyForceToBodyParts(List<GameObject> bodyParts, Vector2 direction)
    // {
    //     if (bodyParts == null) return;
    //
    //     foreach (GameObject bodyPart in bodyParts)
    //     {
    //         ApplyForceToSingleBodyPart(bodyPart, direction);
    //     }
    // }
    //
    // /// <summary>
    // /// 개별 신체 부위에 힘 적용
    // /// </summary>
    // private void ApplyForceToSingleBodyPart(GameObject bodyPart, Vector2 direction)
    // {
    //     if (bodyPart == null) return;
    //
    //     Rigidbody2D rigidbody2D = bodyPart.GetComponent<Rigidbody2D>();
    //     if (rigidbody2D != null)
    //     {
    //         bodyPart.SetActive(true);
    //         rigidbody2D.AddForce(direction * DIE_EFFECT_FORCE, ForceMode2D.Impulse);
    //     }
    // }

    // ====== 새로 추가된 헬퍼 메서드들 ======

    /// <summary>
    /// Owner와 핵심 컴포넌트들의 유효성 검사
    /// </summary>
    private bool ValidateOwnerAndComponents()
    {
        if (_owner == null)
        {
            Debug.LogWarning("[PlayerDieState] Owner is null");
            return false;
        }

        if (_owner.PhotonView == null)
        {
            Debug.LogError("[PlayerDieState] PhotonView is null");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 사망 상태 초기화
    /// </summary>
    private void InitializeDeathState()
    {
        _dieTimer = 0f;
        _hasStartedResurrection = false;
        _hasRequestedDestroy = false;
    }

    /// <summary>
    /// 무적 상태 설정
    /// </summary>
    private void SetImmuneState()
    {
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;
    }

    /// <summary>
    /// 모든 사망 효과 실행
    /// </summary>
    private void ExecuteDeath()
    {
        ExecuteDeathEffects();
        // ApplyBodyPartsDeathEffect();
        // // 사망 폭발 효과
        // CreateDeathExplosion();

        // 플레이어 모습 숨기기
        // SetSpriteRenderersVisibility(false);
        //
        // // 사망 사운드 재생
        // PlayDeathSound();
    }

    // /// <summary>
    // /// 사망 폭발 효과 생성
    // /// </summary>
    // private void CreateDeathExplosion()
    // {
    //     Explosion dieExplosion = ExplosionPool.Instance.Get(_owner.DieExplosionPrefab.name);
    //     dieExplosion.transform.position = _owner.transform.position;
    //     dieExplosion.Explode(true, _owner.PhotonView);
    // }

    // /// <summary>
    // /// 스프라이트 렌더러들의 가시성 설정
    // /// </summary>
    // private void SetSpriteRenderersVisibility(bool isVisible)
    // {
    //     List<SpriteRenderer> spriteRenderers = _owner.PlayerStat.MySpriteREndererList;
    //     if (spriteRenderers == null) return;
    //
    //     foreach (SpriteRenderer spriteRenderer in spriteRenderers)
    //     {
    //         if (spriteRenderer != null)
    //         {
    //             spriteRenderer.enabled = isVisible;
    //         }
    //     }
    // }

    // /// <summary>
    // /// 사망 사운드 재생
    // /// </summary>
    // private void PlayDeathSound()
    // {
    //     SoundManager.Instance.PlayLocalRandomSound("PlayerDeath", transform, 1, 2);
    // }

    /// <summary>
    /// 플레이어 태그 복원
    /// </summary>
    private void RestorePlayerTag()
    {
        _owner.gameObject.tag = _owner.PhotonView.IsMine ? "Player" : "Enemy";
    }

    /// <summary>
    /// 생명 수가 남아있는지 확인
    /// </summary>
    private bool HasNoMoreLives()
    {
        return _owner.PlayerStat.CurrentPlayerLife <= 0;
    }

    /// <summary>
    /// 영구 사망 처리
    /// </summary>
    private void HandlePermanentDeath()
    {
        // 중복 처리 방지
        if (_hasRequestedDestroy) return;
       _hasRequestedDestroy = true;
       
       // 죽었음을 먼저 알려주기 
       if (_owner.photonView.IsMine)
       {
           TransitionToObserveState();
           EventManager.Instance.PlayObserve();   
       }
    }

    // public void LastDieCheck(bool isLastPlayer)
    // {
    //     Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
    //     
    //     if (isLastPlayer)
    //     {
    //         
    //         Debug.Log($"{player.NickName}player Die = LastDie");
    //         SyncStateChange<PlayerLastDieState>();
    //         return;
    //     }
    //     
    //     Debug.Log("player Die");
    //     ExecuteDeath();
    //
    //     // 본인의 클라이언트에서만 관전 상태로 전환 및 통계 업데이트
    //     if (_owner.PhotonView.IsMine)
    //     {
    //         TransitionToObserveState();
    //         EventManager.Instance.PlayObserve();   
    //     }
    //     
    // }

    /// <summary>
    /// 부활 프로세스 처리
    /// </summary>
    private void HandleResurrectionProcess()
    {
        _dieTimer += Time.deltaTime;
        
        // 부활 대기 시간이 지나지 않았으면 대기
        if (_dieTimer < RESURRECTION_DELAY_TIME)
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

    /// <summary>
    /// 관전 상태로 전환
    /// </summary>
    private void TransitionToObserveState()
    {
        Debug.Log("Observe");
        SyncStateChange<PlayerObserveState>();
    }

    /// <summary>
    /// 플레이어 통계 업데이트
    /// </summary>
    private void UpdatePlayerStatistics()
    {
        // PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        // {
        //     {EProperties.IsDead.ToString(), true},
        //     {EProperties.Kill.ToString(), _owner.PlayerStat.TotalKillCount},
        //     {EProperties.Damage.ToString(), _owner.PlayerStat.TotalDamage}
        // });
    }

    /// <summary>
    /// 부활 위치로 이동
    /// </summary>
    private void MoveToResurrectionPoint()
    {
        DOTween.Kill(_owner.transform);
        _owner.transform.position = GameManager.Instance.GetResurrectPoint().position;
    }
}
