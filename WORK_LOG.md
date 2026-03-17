# 플레이어 레이어 시스템 버그 수정 작업 내역서

> 작업일: 2026-03-04
> 브랜치: KKH_Test

---

## 작업 1: 스킨 스프라이트 레이어/소팅 전파 수정

**문제**: 기본 이미지에는 layer/sortingLayerID가 정상 적용되지만, 스킨 이미지(자식 오브젝트 포함)에는 전파되지 않음.

### PlayerSkinManager.cs (`Assets/02.Scripts/Player/Skin/`)
- [x] `SetLayerRecursive(GameObject, int)` 유틸 메서드 추가 — 모든 자식에 GameObject.layer 재귀 설정
- [x] `CopySortingLayer(GameObject, GameObject)` 유틸 메서드 추가 — source의 sortingLayerID를 target의 모든 SpriteRenderer에 적용
- [x] `ReplacePrefabInSlot` — `instance.layer = originalObject.layer` (루트만) → `SetLayerRecursive` + `CopySortingLayer` (전체 자식 포함)
- [x] `AdditiveEquip` — layer/sortingLayerID 설정 코드 추가 (기존엔 아예 없었음)
- [x] `ReplaceDiePrefabInSlot` — layer/sortingLayerID 복사 코드 추가 (기존엔 없었음)
- [x] `AdditiveEquipDie` — layer/sortingLayerID 설정 코드 추가 (기존엔 없었음)

### HeadBomb.cs (`Assets/02.Scripts/Player/`)
- [x] `SetLayerRecursive`, `CopySortingLayer` 유틸 메서드 추가
- [x] `ReplacePrefabInSlot` — layer/sortingLayerID 복사 추가 (기존엔 전혀 없었음)

### Player.cs (`Assets/02.Scripts/Player/`)
- [x] `SetDownJump` — `foreach (Transform child in transform)` → `GetComponentsInChildren<Transform>(true)` 로 변경 (직계 자식만 → 전체 하위 계층)
- [x] `ResetDownJump` — 동일하게 전체 하위 계층 포함으로 수정

---

## 작업 2: SortingOrder 슬롯 시스템 수정 (ActorNr → 슬롯 인덱스)

**문제**: `SetPlayerOrderInLayer`에서 `PhotonView.OwnerActorNr`을 직접 사용하여 sortingOrder 오프셋을 계산. Photon ActorNr은 순차 증가하며 재사용되지 않으므로, 유저가 나갔다 들어오면 500, 600, 700번대로 계속 증가. 기획서 요구사항은 1~4 슬롯을 빈 곳부터 재할당.

### Player.cs (`Assets/02.Scripts/Player/`)
- [x] `GetPlayerSlotIndex()` 메서드 추가 — Room CustomProperties의 `PlayerList` 배열에서 자신의 ActorNumber 위치(0~3) 조회, 못 찾으면 `OwnerActorNr - 1` fallback
- [x] `SetPlayerOrderInLayer` — `PhotonView.OwnerActorNr` → `GetPlayerSlotIndex() + 1` 로 변경 (항상 1~4 범위)

### PlayerVisualController.cs (`Assets/02.Scripts/Player/`)
- [x] `using Photon.Pun` 추가
- [x] `GetPlayerSlotIndex()` 메서드 추가 — Player.cs와 동일 로직
- [x] `SetPlayerOrderInLayer` — `_player.PhotonView.OwnerActorNr` → `GetPlayerSlotIndex() + 1` 로 변경

---

## 작업 3: 스킨 sortingOrder 이중 오프셋 버그 수정

**문제**: `RPC_LoadItems`가 재호출될 때(두번째 유저 입장 시), `ReplacePrefabInSlot`이 원본 Face의 `sortingOrder`를 복사하는데, 원본의 sortingOrder는 이전 `SetPlayerOrderInLayer` 호출로 이미 오프셋(+100)이 적용된 상태. 그 위에 다시 +100이 적용되어 275가 됨.

```
1차: 원본 75 → 스킨 75 → +100 = 175 ✓ (이때 원본도 75→175로 변경됨)
2차: 원본 175 복사 → 스킨 175 → +100 = 275 ✗
```

### PlayerVisualController.cs (`Assets/02.Scripts/Player/`)
- [x] `GetOriginalSortingOrder(SpriteRenderer)` 메서드 추가 — `_originalSortingOrderMap`에서 오프셋 적용 전 원본 값 반환

### Player.cs (`Assets/02.Scripts/Player/`)
- [x] `GetOriginalSortingOrder(SpriteRenderer)` 래퍼 메서드 추가 — VisualController에 위임

### PlayerSkinManager.cs (`Assets/02.Scripts/Player/Skin/`)
- [x] `ReplacePrefabInSlot` — `srcSr.sortingOrder` → `_player.GetOriginalSortingOrder(srcSr)` 로 변경 (항상 오프셋 적용 전 원본 값을 복사)

---

## 코드 변경 상세 (Diff)

### PlayerSkinManager.cs

**유틸 메서드 추가** (Awake 뒤에 추가)
```csharp
// 모든 자식 포함 GameObject.layer 일괄 설정
private void SetLayerRecursive(GameObject obj, int layer)
{
    if (obj == null) { return; }
    obj.layer = layer;
    foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
    {
        child.gameObject.layer = layer;
    }
}

// source의 sortingLayerID를 target의 모든 SpriteRenderer에 복사
private void CopySortingLayer(GameObject source, GameObject target)
{
    if (source == null || target == null) { return; }
    SpriteRenderer srcSr = source.GetComponent<SpriteRenderer>();
    if (srcSr == null) { return; }
    int sortingLayerID = srcSr.sortingLayerID;
    foreach (SpriteRenderer sr in target.GetComponentsInChildren<SpriteRenderer>(true))
    {
        sr.sortingLayerID = sortingLayerID;
    }
}
```

**ReplacePrefabInSlot** — 루트만 복사 → 전체 자식 포함 + 이중 오프셋 방지
```diff
- instance.layer = originalObject.layer;
+ SetLayerRecursive(instance, originalObject.layer);
+ CopySortingLayer(originalObject, instance);
  SpriteRenderer srcSr = originalObject.GetComponent<SpriteRenderer>();
  SpriteRenderer dstSr = instance.GetComponent<SpriteRenderer>();
  if (srcSr != null && dstSr != null)
  {
-     dstSr.sortingLayerID = srcSr.sortingLayerID;
-     dstSr.sortingOrder = srcSr.sortingOrder;
+     dstSr.sortingOrder = _player != null ? _player.GetOriginalSortingOrder(srcSr) : srcSr.sortingOrder;
  }
```

**AdditiveEquip** — layer/sortingLayer 설정 추가 (기존엔 없었음)
```diff
  instance.transform.localScale = Vector3.one;
+ SetLayerRecursive(instance, slotParent.gameObject.layer);
+ CopySortingLayer(slotParent.gameObject, instance);
  if (manageLists) { AddInstanceComponentsToLists(instance); }
```

**ReplaceDiePrefabInSlot** — layer/sortingLayer 복사 추가
```diff
  instance.transform.localScale = localScale;
+ SetLayerRecursive(instance, originalObject.layer);
+ CopySortingLayer(originalObject, instance);
  SetDieOriginalVisibility(originalObject, false);
```

**AdditiveEquipDie** — layer/sortingLayer 설정 추가
```diff
  instance.transform.localScale = Vector3.one;
+ SetLayerRecursive(instance, slotParent.gameObject.layer);
  if (originalObject != null)
  {
+     CopySortingLayer(originalObject, instance);
      SetDieOriginalVisibility(originalObject, false);
  }
```

---

### Player.cs

**SetPlayerOrderInLayer** — ActorNr → 슬롯 인덱스
```diff
- int playerOrderInLayerPlus = PhotonView.OwnerActorNr;
+ int playerOrderInLayerPlus = GetPlayerSlotIndex() + 1;
```

**GetPlayerSlotIndex 추가** — Room CustomProperties에서 슬롯 조회
```csharp
private int GetPlayerSlotIndex()
{
    if (PhotonNetwork.CurrentRoom == null) { return PhotonView.OwnerActorNr - 1; }

    var props = PhotonNetwork.CurrentRoom.CustomProperties;
    string key = EProperties.PlayerList.ToString();
    if (!props.ContainsKey(key)) { return PhotonView.OwnerActorNr - 1; }

    int[] slotList = props[key] as int[];
    if (slotList == null) { return PhotonView.OwnerActorNr - 1; }

    int actorNr = PhotonView.OwnerActorNr;
    for (int i = 0; i < slotList.Length; i++)
    {
        if (slotList[i] == actorNr) { return i; }
    }
    return PhotonView.OwnerActorNr - 1;
}
```

**GetOriginalSortingOrder 추가** — VisualController에 위임
```csharp
public int GetOriginalSortingOrder(SpriteRenderer renderer)
{
    if (_visualController != null) { return _visualController.GetOriginalSortingOrder(renderer); }
    return renderer != null ? renderer.sortingOrder : 0;
}
```

**SetDownJump / ResetDownJump** — 직계 자식만 → 전체 하위 계층
```diff
- gameObject.layer = LayerMask.NameToLayer("DownJump");
- foreach (Transform child in transform)
- {
-     child.gameObject.layer = LayerMask.NameToLayer("DownJump");
- }
+ int downJumpLayer = LayerMask.NameToLayer("DownJump");
+ gameObject.layer = downJumpLayer;
+ foreach (Transform child in GetComponentsInChildren<Transform>(true))
+ {
+     child.gameObject.layer = downJumpLayer;
+ }
```
```diff
- gameObject.layer = LayerMask.NameToLayer("Player");
- foreach (Transform child in transform)
- {
-     child.gameObject.layer = LayerMask.NameToLayer("Player");
- }
+ int playerLayer = LayerMask.NameToLayer("Player");
+ gameObject.layer = playerLayer;
+ foreach (Transform child in GetComponentsInChildren<Transform>(true))
+ {
+     child.gameObject.layer = playerLayer;
+ }
```

---

### PlayerVisualController.cs

**using 추가**
```diff
+ using Photon.Pun;
```

**GetOriginalSortingOrder 추가** — 오프셋 적용 전 원본 sortingOrder 반환
```csharp
public int GetOriginalSortingOrder(SpriteRenderer renderer)
{
    if (renderer != null && _originalSortingOrderMap != null && _originalSortingOrderMap.ContainsKey(renderer))
    {
        return _originalSortingOrderMap[renderer];
    }
    return renderer != null ? renderer.sortingOrder : 0;
}
```

**SetPlayerOrderInLayer** — ActorNr → 슬롯 인덱스
```diff
- int playerOrderInLayerPlus = _player.PhotonView.OwnerActorNr;
+ int playerOrderInLayerPlus = GetPlayerSlotIndex() + 1;
```

**GetPlayerSlotIndex 추가** — Player.cs와 동일 로직
```csharp
private int GetPlayerSlotIndex()
{
    if (_player == null || _player.PhotonView == null) { return 0; }
    if (PhotonNetwork.CurrentRoom == null) { return _player.PhotonView.OwnerActorNr - 1; }

    var props = PhotonNetwork.CurrentRoom.CustomProperties;
    string key = EProperties.PlayerList.ToString();
    if (!props.ContainsKey(key)) { return _player.PhotonView.OwnerActorNr - 1; }

    int[] slotList = props[key] as int[];
    if (slotList == null) { return _player.PhotonView.OwnerActorNr - 1; }

    int actorNr = _player.PhotonView.OwnerActorNr;
    for (int i = 0; i < slotList.Length; i++)
    {
        if (slotList[i] == actorNr) { return i; }
    }
    return _player.PhotonView.OwnerActorNr - 1;
}
```

---

### HeadBomb.cs

**유틸 메서드 추가** — PlayerSkinManager와 동일
```csharp
private void SetLayerRecursive(GameObject obj, int layer) { /* 동일 */ }
private void CopySortingLayer(GameObject source, GameObject target) { /* 동일 */ }
```

**ReplacePrefabInSlot** — layer/sortingLayer 복사 추가 (기존엔 없었음)
```diff
  instance.transform.localScale = localScale;
+ if (originalObject != null)
+ {
+     SetLayerRecursive(instance, originalObject.layer);
+     CopySortingLayer(originalObject, instance);
+ }
  return instance;
```
