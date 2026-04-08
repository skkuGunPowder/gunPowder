using System;
using UnityEngine;

// 플레이어 이벤트 정의 클래스
// 모든 플레이어 이벤트 선언 + Invoke 래퍼 + Clear()를 포함한다.
public class PlayerEvents
{
    // 스폰됨 (첫 스폰)
    public event Action OnSpawned;
    // 부활 (카트리지로 인한 부활)
    public event Action OnResurrected;
    // 사망 (마지막 킬인지, 그냥 킬인지, 상대의 킬 이펙트를 판단하거나, 관전 모드로 보내기 위해)
    public event Action<DeathContext> OnDeath;
    // 디스폰됨 (사망하고 오브젝트 비활성화를 위해)
    public event Action OnDespawned;

    // 공격 입력 (반동 등을 위해)
    public event Action OnAttack;
    // Z 공격 (무슨 공격이 나갈건지)
    public event Action OnNormalAttack;
    // X 공격 (무슨 공격이 나갈건지)
    public event Action OnSpecialAttack;
    // 공격 적중 (사운드 재생, hit 파티클 등을 소환하기 위해)
    public event Action<AttackHitContext> OnAttackHit;
    // 처치 성공 (킬 사운드, 킬로그 등을 사용하기 위해)
    public event Action<KillContext> OnKillConfirmed;

    // 피격 (상태 전환용)
    public event Action OnHit;
    // 공격 받음 (데미지가 닳고, 건파우더를 뿌릴지 흡수당할지 등을 정하기 위해)
    public event Action<DamagedContext> OnDamaged;

    // 궁극기 게이지 충전 (충전되는 UI 표시하기 위함)
    public event Action OnUltimateChanceActivated;
    // 궁극기 게이지 미충전 (UI 비활성화)
    public event Action OnUltimateChanceDeactivated;
    // 궁극기 사용
    public event Action OnUltimateUsed;

    // 공격 정체 패널티 카운트 시작 (공격을 하지 않고 대기할 때)
    public event Action OnNoAttackPenaltyStart;
    // 공격 정체 패널티 카운트 발동 (위 카운트가 모두 차 발동될 때)
    public event Action OnNoAttackPenaltyTriggered;
    // 공격 정체 패널티 카운트 종료 (공격을 하여 카운트를 초기화 할 때)
    public event Action OnNoAttackPenaltyReset;

    // 공중 진입 (공중 대쉬를 판단 등을 하기 위해)
    public event Action OnAirborne;
    // 착지 (대쉬 카운트 등을 초기화하기 위해)
    public event Action OnLanded;
    // 다른 플레이어와의 스침 (자폭탄 궁극기, 화상 카트리지를 위해)
    public event Action<int> OnPlayerContact;
    // 다른 오브젝트와의 스침 (게, 대포 등 맵 기믹들과 아이템 획득 등을 위해)
    public event Action<Collider2D> OnObjectContact;

    // GP 변경 (빚 등을 졌을 때 강제로 조정하기 위해)
    public event Action<int> OnGPSet;
    // GP 획득 (GP를 외부 요인으로 획득하여 UI 및 수치를 변경하기 위해)
    public event Action<int> OnGPGained;
    // GP 손실 (GP를 외부 요인으로 손실하여 UI 및 수치를 변경하기 위해)
    public event Action<int> OnGPLost;

    // 아이템 획득 (아이템 상자에 닿아 아이템을 획득하였을 때 룰렛을 돌리기 위해)
    public event Action<string> OnItemPickedUp;
    // 아이템 사용 (아이템 룰렛을 돌린 후 아이템을 사용하기 위해)
    public event Action<string> OnItemUsed;

    // Invoke 래퍼 메서드

    // 스폰됨 (첫 스폰)
    public void InvokeOnSpawned() { Debug.Log("[PlayerEvents] 스폰됨"); OnSpawned?.Invoke(); }
    // 부활 (카트리지로 인한 부활)
    public void InvokeOnResurrected() { Debug.Log("[PlayerEvents] 부활"); OnResurrected?.Invoke(); }
    // 사망
    public void InvokeOnDeath(DeathContext ctx) { Debug.Log($"[PlayerEvents] 사망 (킬러: {ctx.KillerActorNumber}, 마지막킬: {ctx.IsLastKill}, 일반공격: {ctx.IsNormalAttack})"); OnDeath?.Invoke(ctx); }
    // 디스폰됨
    public void InvokeOnDespawned() { Debug.Log("[PlayerEvents] 디스폰됨"); OnDespawned?.Invoke(); }

    // 공격 입력 (반동 등을 위해)
    public void InvokeOnAttack() { Debug.Log("[PlayerEvents] 공격 입력"); OnAttack?.Invoke(); }
    // Z 공격
    public void InvokeOnNormalAttack() { Debug.Log("[PlayerEvents] Z 공격"); OnNormalAttack?.Invoke(); }
    // X 공격
    public void InvokeOnSpecialAttack() { Debug.Log("[PlayerEvents] X 공격"); OnSpecialAttack?.Invoke(); }
    // 공격 적중
    public void InvokeOnAttackHit(AttackHitContext ctx) { Debug.Log($"[PlayerEvents] 공격 적중 (피격자: {ctx.VictimActorNumber}, 데미지: {ctx.Damage}, 크리티컬: {ctx.IsCritical})"); OnAttackHit?.Invoke(ctx); }
    // 처치 성공
    public void InvokeOnKillConfirmed(KillContext ctx) { Debug.Log($"[PlayerEvents] 처치 성공 (피격자: {ctx.VictimActorNumber}, 마지막킬: {ctx.IsLastKill}, 일반공격: {ctx.IsNormalAttack})"); OnKillConfirmed?.Invoke(ctx); }

    // 피격
    public void InvokeOnHit() { Debug.Log("[PlayerEvents] 피격"); OnHit?.Invoke(); }
    // 공격 받음
    public void InvokeOnDamaged(DamagedContext ctx) { Debug.Log($"[PlayerEvents] 공격 받음 (데미지: {ctx.Damage}, 공격자: {ctx.AttackerActorNumber}, 낙사: {ctx.IsFallingOut})"); OnDamaged?.Invoke(ctx); }

    // 궁극기 게이지 충전
    public void InvokeOnUltimateChanceActivated() { Debug.Log("[PlayerEvents] 궁극기 게이지 충전"); OnUltimateChanceActivated?.Invoke(); }
    // 궁극기 게이지 미충전
    public void InvokeOnUltimateChanceDeactivated() { Debug.Log("[PlayerEvents] 궁극기 게이지 미충전"); OnUltimateChanceDeactivated?.Invoke(); }
    // 궁극기 사용
    public void InvokeOnUltimateUsed() { Debug.Log("[PlayerEvents] 궁극기 사용"); OnUltimateUsed?.Invoke(); }

    // 공격 정체 패널티 카운트 시작
    public void InvokeOnNoAttackPenaltyStart() { Debug.Log("[PlayerEvents] 공격 정체 패널티 카운트 시작"); OnNoAttackPenaltyStart?.Invoke(); }
    // 공격 정체 패널티 카운트 발동
    public void InvokeOnNoAttackPenaltyTriggered() { Debug.Log("[PlayerEvents] 공격 정체 패널티 카운트 발동"); OnNoAttackPenaltyTriggered?.Invoke(); }
    // 공격 정체 패널티 카운트 종료
    public void InvokeOnNoAttackPenaltyReset() { Debug.Log("[PlayerEvents] 공격 정체 패널티 카운트 종료"); OnNoAttackPenaltyReset?.Invoke(); }

    // 공중 진입
    public void InvokeOnAirborne() { Debug.Log("[PlayerEvents] 공중 진입"); OnAirborne?.Invoke(); }
    // 착지
    public void InvokeOnLanded() { Debug.Log("[PlayerEvents] 착지"); OnLanded?.Invoke(); }
    // 다른 플레이어와의 스침
    public void InvokeOnPlayerContact(int actorNumber) { Debug.Log($"[PlayerEvents] 플레이어 스침 (상대: {actorNumber})"); OnPlayerContact?.Invoke(actorNumber); }
    // 다른 오브젝트와의 스침
    public void InvokeOnObjectContact(Collider2D collider) { Debug.Log($"[PlayerEvents] 오브젝트 스침 ({collider.gameObject.name})"); OnObjectContact?.Invoke(collider); }

    // GP 변경
    public void InvokeOnGPSet(int value) { Debug.Log($"[PlayerEvents] GP 변경 (값: {value})"); OnGPSet?.Invoke(value); }
    // GP 획득
    public void InvokeOnGPGained(int amount) { Debug.Log($"[PlayerEvents] GP 획득 (+{amount})"); OnGPGained?.Invoke(amount); }
    // GP 손실
    public void InvokeOnGPLost(int amount) { Debug.Log($"[PlayerEvents] GP 손실 (-{amount})"); OnGPLost?.Invoke(amount); }

    // 아이템 획득
    public void InvokeOnItemPickedUp(string itemId) { Debug.Log($"[PlayerEvents] 아이템 획득 ({itemId})"); OnItemPickedUp?.Invoke(itemId); }
    // 아이템 사용
    public void InvokeOnItemUsed(string itemId) { Debug.Log($"[PlayerEvents] 아이템 사용 ({itemId})"); OnItemUsed?.Invoke(itemId); }

    // 모든 이벤트 구독 해제
    public void Clear()
    {
        OnSpawned = null;
        OnResurrected = null;
        OnDeath = null;
        OnDespawned = null;

        OnAttack = null;
        OnNormalAttack = null;
        OnSpecialAttack = null;
        OnAttackHit = null;
        OnKillConfirmed = null;

        OnHit = null;
        OnDamaged = null;

        OnUltimateChanceActivated = null;
        OnUltimateChanceDeactivated = null;
        OnUltimateUsed = null;

        OnNoAttackPenaltyStart = null;
        OnNoAttackPenaltyTriggered = null;
        OnNoAttackPenaltyReset = null;

        OnAirborne = null;
        OnLanded = null;
        OnPlayerContact = null;
        OnObjectContact = null;

        OnGPSet = null;
        OnGPGained = null;
        OnGPLost = null;

        OnItemPickedUp = null;
        OnItemUsed = null;
    }
}
