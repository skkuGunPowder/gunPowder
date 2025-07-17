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

    private void LoadItemStorageData(Dictionary<EEquipmentSlot, List<Item>> itemDict)
    {
        _storedItemDict = itemDict;

        OnDataChanged?.Invoke(CurrentCategory);
    }

    private void LoadInventoryData(Dictionary<EEquipmentSlot, Item> equippedItemDict)
    {
        _equippedItemDict = equippedItemDict;

        OnDataChanged?.Invoke(CurrentCategory);
    }


    #if UNITY_EDITOR
    public Sprite TestIcon;
    private void Update()
    {
        // 저장된 데이터 로드 테스트
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            _repo.LoadItemStorage();
            _repo.LoadInventory();
        }

        // 아이템 추가 테스트
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ItemDTO newItem = new ItemDTO(
                UnityEngine.Random.Range(0, 11).ToString(),
                "Test Item",
                "Descriptions...",
                TestIcon,
                "Assets/05.Images/Item/PunIcon-128.png",
                (EEquipmentSlot)UnityEngine.Random.Range(0, (int)EEquipmentSlot.None),
                false
            );

            AddItem(newItem);
        }

        // 아이템 장착 테스트
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
        _repo.OnItemStorageLoaded += LoadItemStorageData;
        _repo.OnInventoryLoaded += LoadInventoryData;

        // TODO
        // 파이어베이스 연결 성공 때까지 대기(이벤트 매니저 활용)
        _storedItemDict = null;
        _equippedItemDict = null;


        // 저장된 데이터 없을 시 아이템 보관함 초기화
        if (_storedItemDict == null)
        {
            _storedItemDict = new Dictionary<EEquipmentSlot, List<Item>>();

            for (int i = 0; i < (int)EEquipmentSlot.None; i++)
            {
                _storedItemDict.Add((EEquipmentSlot)i, new List<Item>());
            }
        }

        // 저장된 데이터 없을 시 인벤토리 초기화
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
            Item equippedItem = _storedItemDict[item.EquipmentSlot].Find(x => x.ID == _equippedItemDict[item.EquipmentSlot].ID);
            equippedItem.UnEquip();
        }

        Item desiredItem = _storedItemDict[item.EquipmentSlot].Find(x => x.ID == item.ID);
        desiredItem.Equip();

        _equippedItemDict[item.EquipmentSlot] = desiredItem;

        _repo.SaveInventory(_equippedItemDict);
        _repo.SaveItemStorage(_storedItemDict);
        
        OnDataChanged?.Invoke(item.EquipmentSlot);
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

        _repo.SaveInventory(_equippedItemDict);
        _repo.SaveItemStorage(_storedItemDict);

        OnDataChanged?.Invoke(item.EquipmentSlot);
    }
}
