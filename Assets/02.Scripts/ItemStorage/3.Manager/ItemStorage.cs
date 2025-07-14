using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemStorage : MonoBehaviour
{
    public static ItemStorage Instance { get; private set; }

    public Sprite TestIcon;

    public EEquipmentSlot CurrentCategory { get; private set; }
    public Dictionary<EEquipmentSlot, List<Item>> _storedItemDict { get; private set; }
    private ItemStorageRepo _repo;


    public event Action<List<ItemDTO>> OnDataChange;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Init();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ItemDTO newItem = new ItemDTO(
                TestIcon,
                UnityEngine.Random.Range(0, 11).ToString(),
                "Test Item",
                (EEquipmentSlot)UnityEngine.Random.Range(1, (int)EEquipmentSlot.None),
                false
            );

            AddItem(newItem);

            Debug.Log($"{newItem}");
        }
    }

    private void Init()
    {
        // 현재 카테고리 초기화
        CurrentCategory = EEquipmentSlot.Head;

        // 저장된 데이터 로드
        _repo = new ItemStorageRepo();
        _storedItemDict = _repo.LoadItemStorage();

        // 저장된 데이터 없을 시 빈 딕셔너리 할당
        if (_storedItemDict == null)
        {
            _storedItemDict = new Dictionary<EEquipmentSlot, List<Item>>();
            
            for (int i = 0; i < (int)EEquipmentSlot.None; i++)
            {
                if (!_storedItemDict.ContainsKey((EEquipmentSlot)i))
                {
                    _storedItemDict.Add((EEquipmentSlot)i, new List<Item>());
                }
            }
        }
    }

    public void ChangeCategory(EEquipmentSlot nextCategory)
    {
        CurrentCategory = nextCategory;

        // UI 업데이트
        OnDataChange?.Invoke(_storedItemDict[CurrentCategory].ConvertAll(x => new ItemDTO(x)));
    }

    public void AddItem(ItemDTO newItem)
    {
        // null 검사
        if (newItem == null)
        {
            throw new Exception($"유효하지 않은 아이템입니다!");
        }

        // Item 객체 생성 및 컨테이너에 추가
        Item item = new Item(newItem);
        _storedItemDict[item.EquipmentSlot].Add(item);

        // 데이터 저장
        _repo.SaveItemStorage(_storedItemDict);

        // UI 업데이트
        OnDataChange?.Invoke(_storedItemDict[item.EquipmentSlot].ConvertAll(x => new ItemDTO(x)));
    }

    public void SelectItem(ItemDTO item)
    {
        // 선택된 아이템 검색
        Item selectedItem = _storedItemDict[item.EquipmentSlot].Find(x => x.ID == item.ID);

        // null 검사
        if (selectedItem == null)
        {
            throw new Exception("선택된 아이템이 없습니다!");
        }

        // 선택된 아이템 상태 변경
        selectedItem.Select();
        if (selectedItem.IsEquipped == true)
        {
            // TODO
            // 인벤토리에서 장착 해제
        }
        else
        {
            // TODO
            // 인벤토리에 장착
        }

        // 데이터 저장
        _repo.SaveItemStorage(_storedItemDict);
    }
}
