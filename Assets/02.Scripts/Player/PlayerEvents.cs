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
    public void InvokeOnSpawned() { OnSpawned?.Invoke(); }
    // 부활 (카트리지로 인한 부활)
    public void InvokeOnResurrected() { OnResurrected?.Invoke(); }
    // 사망
    public void InvokeOnDeath(DeathContext ctx) { OnDeath?.Invoke(ctx); }
    // 디스폰됨
    public void InvokeOnDespawned() { OnDespawned?.Invoke(); }

    // 공격 입력 (반동 등을 위해)
    public void InvokeOnAttack() { OnAttack?.Invoke(); }
    // Z 공격
    public void InvokeOnNormalAttack() { OnNormalAttack?.Invoke(); }
    // X 공격
    public void InvokeOnSpecialAttack() { OnSpecialAttack?.Invoke(); }
    // 공격 적중
    public void InvokeOnAttackHit(AttackHitContext ctx) { OnAttackHit?.Invoke(ctx); }
    // 처치 성공
    public void InvokeOnKillConfirmed(KillContext ctx) { OnKillConfirmed?.Invoke(ctx); }

    // 피격
    public void InvokeOnHit() { OnHit?.Invoke(); }
    // 공격 받음
    public void InvokeOnDamaged(DamagedContext ctx) { OnDamaged?.Invoke(ctx); }

    // 궁극기 게이지 충전
    public void InvokeOnUltimateChanceActivated() { OnUltimateChanceActivated?.Invoke(); }
    // 궁극기 게이지 미충전
    public void InvokeOnUltimateChanceDeactivated() { OnUltimateChanceDeactivated?.Invoke(); }
    // 궁극기 사용
    public void InvokeOnUltimateUsed() { OnUltimateUsed?.Invoke(); }

    // 공격 정체 패널티 카운트 시작
    public void InvokeOnNoAttackPenaltyStart() { OnNoAttackPenaltyStart?.Invoke(); }
    // 공격 정체 패널티 카운트 발동
    public void InvokeOnNoAttackPenaltyTriggered() { OnNoAttackPenaltyTriggered?.Invoke(); }
    // 공격 정체 패널티 카운트 종료
    public void InvokeOnNoAttackPenaltyReset() { OnNoAttackPenaltyReset?.Invoke(); }

    // 공중 진입
    public void InvokeOnAirborne() { OnAirborne?.Invoke(); }
    // 착지
    public void InvokeOnLanded() { OnLanded?.Invoke(); }
    // 다른 플레이어와의 스침
    public void InvokeOnPlayerContact(int actorNumber) { OnPlayerContact?.Invoke(actorNumber); }
    // 다른 오브젝트와의 스침
    public void InvokeOnObjectContact(Collider2D collider) { OnObjectContact?.Invoke(collider); }

    // GP 변경
    public void InvokeOnGPSet(int value) { OnGPSet?.Invoke(value); }
    // GP 획득
    public void InvokeOnGPGained(int amount) { OnGPGained?.Invoke(amount); }
    // GP 손실
    public void InvokeOnGPLost(int amount) { OnGPLost?.Invoke(amount); }

    // 아이템 획득
    public void InvokeOnItemPickedUp(string itemId) { OnItemPickedUp?.Invoke(itemId); }
    // 아이템 사용
    public void InvokeOnItemUsed(string itemId) { OnItemUsed?.Invoke(itemId); }

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
