using System.Collections.Generic;
using UnityEngine;

public class SpecialThanksSlotSpawner : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private SpecialThanksSO _data;

    [Header("UI")]
    [SerializeField] private SpecialThanksSlot _slotPrefab;

    // 슬롯을 담아둘 리스트 (나중에 접근용)
    private readonly List<SpecialThanksSlot> _slots = new();

    private void Start()
    {
        BuildCredits();
    }
    
    public void BuildCredits()
    {
        // 안전성 체크
        if (_data == null || _slotPrefab == null)
        {
            Debug.LogWarning("⚠️ 필수 참조가 누락됨");
            return;
        }

        // 슬롯의 부모는 자기 자신
        Transform parent = transform;

        _slots.Clear();

        const int perSlot = 3;
        int total = _data.SPTnames.Count;

        for (int i = 0; i < total; i += perSlot)
        {
            // 자기 자신의 자식으로 슬롯 생성
            SpecialThanksSlot slot = Instantiate(_slotPrefab, parent);
            _slots.Add(slot);

            // 3명씩 끊어서 전달
            int count = Mathf.Min(perSlot, total - i);
            string[] group = new string[count];
            for (int j = 0; j < count; j++)
            {
                group[j] = _data.SPTnames[i + j];
            }

            slot.SetNames(group);
        }

        Debug.Log($"✅ SpecialThanksSlotSpawner: {total}명의 이름을 {Mathf.CeilToInt((float)total / perSlot)}개의 슬롯으로 표시했습니다.");
    }
}
