using System.Collections.Generic;
using UnityEngine;

public class UsedAssetSlotSpawner : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UsedAssetSO _data;

    [Header("UI")]
    [SerializeField] private UsedAssetSlot _slotPrefab;

    private readonly List<UsedAssetSlot> _slots = new();

    private void Awake()
    {
        BuildCredits();
    }

    [ContextMenu("Build Credits")]
    public void BuildCredits()
    {
        if (_data == null || _slotPrefab == null)
        {
            Debug.LogWarning("⚠️ 필수 참조가 누락됨");
            return;
        }

        Transform parent = transform;
        _slots.Clear();

        for (int i = 0; i < _data.Credits.Count; i++)
        {
            var credit = _data.Credits[i];
            var slot = Instantiate(_slotPrefab, parent);
            _slots.Add(slot);
            slot.SetData(credit);
        }

        Debug.Log($"✅ AssetCreditSlotSpawner: {_data.Credits.Count}개의 에셋 크레딧을 표시했습니다.");
    }
}
