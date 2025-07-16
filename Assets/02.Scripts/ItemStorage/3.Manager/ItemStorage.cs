using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemStorage : MonoBehaviour
{
    public static ItemStorage Instance { get; private set; }

    private Dictionary<EEquipmentSlot, List<Item>> _storedItemDict;
    private Dictionary<EEquipmentSlot, Item> _equippedItemDict;

    public EEquipmentSlot CurrentCategory { get; private set; }
    public int SelectedItemIndex { get; private set; }
    private ItemDTO _selectedItem;

    private ItemStorageRepo _repo;

    public event Action<EEquipmentSlot> OnDataChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Init();
    }

    private async void LoadData()
    {
        _storedItemDict = await _repo.LoadItemStorage();
        OnDataChanged?.Invoke(CurrentCategory);
    }

#if UNITY_EDITOR
    public Sprite TestIcon;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            LoadData();
        }

        if (Input.GetKeyDown(KeyCode.Q))
            {
                ItemDTO newItem = new ItemDTO(
                    UnityEngine.Random.Range(0, 11).ToString(),
                    "Test Item",
                    "Descriptions...",
                    TestIcon,
                    (EEquipmentSlot)UnityEngine.Random.Range(0, (int)EEquipmentSlot.None),
                    false
                );

                AddItem(newItem);
            }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (_selectedItem.IsEquipped)
            {
                UnEquipItem(_selectedItem);
            }
            else
            {
                EquipItem(_selectedItem);
            }
        }
    }
#endif

    private void Init()
    {
        // 현재 카테고리 초기화
        CurrentCategory = EEquipmentSlot.Head;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // 저장된 데이터 로드
        _repo = new ItemStorageRepo();
        // _storedItemDict = _repo.LoadItemStorage().Result;
        _storedItemDict = null;
        _equippedItemDict = _repo.LoadInventory();



        // 저장된 데이터 없을 시 딕셔너리 초기화
        if (_storedItemDict == null)
        {
            _storedItemDict = new Dictionary<EEquipmentSlot, List<Item>>();

            for (int i = 0; i < (int)EEquipmentSlot.None; i++)
            {
                _storedItemDict.Add((EEquipmentSlot)i, new List<Item>());
            }
        }

        if (_equippedItemDict == null)
        {
            _equippedItemDict = new Dictionary<EEquipmentSlot, Item>((int)EEquipmentSlot.None);
            for (int i = 0; i < (int)EEquipmentSlot.None; i++)
            {
                _equippedItemDict.Add((EEquipmentSlot)i, null);
            }
        }
    }

    public List<ItemDTO> GetStoredItemList(EEquipmentSlot equipmentSlot)
    {
        return _storedItemDict[equipmentSlot].ConvertAll(x => new ItemDTO(x));
    }

    public ItemDTO GetEquppedItem(EEquipmentSlot equipmentSlot)
    {
        if (_equippedItemDict[equipmentSlot] == null)
        {
            return null;
        }

        return new ItemDTO(_equippedItemDict[equipmentSlot]);
    }

    public ItemDTO GetSelectedItem()
    {
        if (_selectedItem == null)
        {
            return null;
        }

        return _selectedItem;
    }

    public void ChangeCategory(EEquipmentSlot nextCategory)
    {
        // 현재 카테고리 변경
        CurrentCategory = nextCategory;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
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

        // 현재 카테고리 변경
        CurrentCategory = item.EquipmentSlot;

        // 데이터 저장
        _repo.SaveItemStorage(_storedItemDict);

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
    }

    public void SelectItem(ItemDTO item)
    {
        // 선택된 아이템 인덱스 검색
        int newSelectedItemIndex = _storedItemDict[item.EquipmentSlot].FindIndex(x => x.ID == item.ID);

        if (newSelectedItemIndex == -1)
        {
            throw new Exception("선택된 아이템이 없습니다!");
        }

        _selectedItem = item;
        SelectedItemIndex = newSelectedItemIndex;

        OnDataChanged.Invoke(item.EquipmentSlot);
    }

    public void EquipItem(ItemDTO item)
    {
        if (item == null)
        {
            throw new Exception("장착하려는 아이템이 없습니다!");
        }

        if (!_equippedItemDict.ContainsKey(item.EquipmentSlot))
        {
            throw new Exception($"장착아이템 컨테이너에 해당 키가 없습니다 || {item.EquipmentSlot}");
        }

        if (_equippedItemDict[item.EquipmentSlot] != null)
        {
            _equippedItemDict[item.EquipmentSlot].UnEquip();
        }

        Item desiredItem = _storedItemDict[item.EquipmentSlot].Find(x => x.ID == item.ID);
        desiredItem.Equip();

        _equippedItemDict[item.EquipmentSlot] = desiredItem;

        OnDataChanged?.Invoke(item.EquipmentSlot);
        _repo.SaveItemStorage(_storedItemDict);
        _repo.SaveInventory();
    }

    public void UnEquipItem(ItemDTO item)
    {
        if (item == null)
        {
            throw new Exception("해제하려는 아이템이 없습니다!");
        }

        if (!_equippedItemDict.ContainsKey(item.EquipmentSlot))
        {
            throw new Exception("해제하려는 아이템이 없습니다!");
        }

        Item desiredItem = _storedItemDict[item.EquipmentSlot].Find(x => x.ID == item.ID);
        desiredItem.UnEquip();

        _equippedItemDict[item.EquipmentSlot] = null;

        OnDataChanged?.Invoke(item.EquipmentSlot);
        _repo.SaveInventory();
    }
}
