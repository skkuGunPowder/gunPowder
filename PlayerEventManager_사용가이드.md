# PlayerEventManager 사용가이드

## 기본 사용법

```csharp
// 로컬 플레이어 이벤트 구독
PlayerEventManager.Instance.Local.OnDeath += ctx => { /* 처리 */ };

// 특정 플레이어 이벤트 구독 (actorNumber 기준)
PlayerEventManager.Instance.GetEvents(actorNumber).OnHit += () => { /* 처리 */ };
```

**구독 해제는 반드시 OnDestroy에서 처리할 것.**

```csharp
void OnDestroy()
{
    PlayerEventManager.Instance.GetEvents(_actorNumber).OnHit -= HandleHit;
}
```

---

## 이벤트 발행 (내부 시스템용)

```csharp
PlayerEventManager.Instance.GetEvents(ActorNumber).InvokeOnDeath(new DeathContext { ... });
```

---

## 이벤트 목록

### 생명주기

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnSpawned` | `Action` | 첫 스폰 완료 |
| `OnResurrected` | `Action` | 카트리지 부활 완료 |
| `OnDeath` | `Action<DeathContext>` | 사망 확정 |
| `OnDespawned` | `Action` | 오브젝트 비활성화 직전 |

### 공격

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnAttack` | `Action` | 공격 입력 (Z/X 공통) |
| `OnNormalAttack` | `Action` | Z 공격 발사 |
| `OnSpecialAttack` | `Action` | X 공격 발사 |
| `OnAttackHit` | `Action<AttackHitContext>` | 공격 적중 |
| `OnKillConfirmed` | `Action<KillContext>` | 적 처치 성공 |

### 피격

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnHit` | `Action` | 피격 시 |
| `OnDamaged` | `Action<DamagedContext>` | 데미지 받음 (상세 정보 포함) |

### 궁극기

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnUltimateChanceActivated` | `Action` | 궁극기 사용 가능 |
| `OnUltimateChanceDeactivated` | `Action` | 궁극기 사용 불가 |
| `OnUltimateUsed` | `Action` | 궁극기 사용 확정 |

### 패널티

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnNoAttackPenaltyStart` | `Action` | 경고 카운트 시작 |
| `OnNoAttackPenaltyTriggered` | `Action` | 패널티 발동 |
| `OnNoAttackPenaltyReset` | `Action` | 카운트 초기화 |

### 물리/이동

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnAirborne` | `Action` | 지상 -> 공중 전환 |
| `OnLanded` | `Action` | 착지 |
| `OnPlayerContact` | `Action<int>` | 플레이어 스침 (매개변수: 상대 actorNumber) |
| `OnObjectContact` | `Action<Collider2D>` | 오브젝트 스침 (매개변수: 충돌 콜라이더) |

### GP

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnGPSet` | `Action<int>` | GP 강제 조정 (매개변수: 설정된 GP 값) |
| `OnGPGained` | `Action<int>` | GP 획득 (매개변수: 획득량) |
| `OnGPLost` | `Action<int>` | GP 손실 (매개변수: 손실량) |

### 아이템

| 이벤트 | 시그니처 | 설명 |
|--------|----------|------|
| `OnItemPickedUp` | `Action<string>` | 아이템 획득 (매개변수: 아이템 ID) |
| `OnItemUsed` | `Action<string>` | 아이템 사용 (매개변수: 아이템 ID) |

---

## Context 구조체

### DeathContext

| 필드 | 타입 | 설명 |
|------|------|------|
| `KillerActorNumber` | `int` | 킬러의 actorNumber (-1 = 자살/환경 사망) |
| `IsLastKill` | `bool` | 마지막 킬 여부 |
| `IsNormalAttack` | `bool` | Z 공격에 의한 사망 여부 |

### AttackHitContext

| 필드 | 타입 | 설명 |
|------|------|------|
| `VictimActorNumber` | `int` | 피격자 actorNumber |
| `Damage` | `int` | 실제 데미지 |
| `MaxDamage` | `int` | 최대 데미지 |
| `IsCritical` | `bool` | 크리티컬 여부 (damage == maxDamage) |
| `VictimPosition` | `Vector3` | 피격자 위치 |

### KillContext

| 필드 | 타입 | 설명 |
|------|------|------|
| `VictimActorNumber` | `int` | 처치된 플레이어 actorNumber |
| `IsLastKill` | `bool` | 마지막 킬 여부 |
| `IsNormalAttack` | `bool` | Z 공격에 의한 처치 여부 |

### DamagedContext

| 필드 | 타입 | 설명 |
|------|------|------|
| `Damage` | `int` | 받은 데미지 |
| `MaxDamage` | `int` | 최대 데미지 |
| `AttackerActorNumber` | `int` | 공격자 actorNumber |
| `AttackerViewId` | `int` | 공격자 PhotonView ID |
| `AttackerBombPosition` | `Vector3` | 폭탄 위치 |
| `StealPercent` | `int` | GP 탈취 비율 |
| `IsFallingOut` | `bool` | 낙사 여부 |
| `IsNormalAttack` | `bool` | Z 공격에 의한 피격 여부 |

---

## 사용 예시

```csharp
// 킬 피드 UI에서 처치 이벤트 구독
PlayerEventManager.Instance.GetEvents(targetActorNumber).OnKillConfirmed += ctx =>
{
    ShowKillFeed(ctx.VictimActorNumber, ctx.IsLastKill);
};

// 카메라 흔들림
PlayerEventManager.Instance.Local.OnDamaged += ctx =>
{
    ShakeCamera(ctx.Damage);
};

// 플레이어 퇴장 시 정리
PlayerEventManager.Instance.RemoveEvents(actorNumber);
```
