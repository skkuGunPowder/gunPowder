using System;
using System.Collections.Generic;
using UnityEngine;


public class ItemStorage : MonoBehaviour
{
    public static ItemStorage Instance { get; private set; }

    private Dictionary<EItemType, List<InventoryItem>> _storedItemDict;
    private Dictionary<EItemType, InventoryItem> _equippedItemDict;

    public EItemType CurrentCategory { get; private set; }
    public int SelectedItemIndex { get; private set; }
    private InventoryItem _selectedItem;

    private ItemStorageRepo _repo;

    public event Action<EItemType> OnDataChanged;


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
        DontDestroyOnLoad(gameObject);

        _repo = new ItemStorageRepo();
        _repo.OnItemStorageLoaded += LoadItemStorageData;
        _repo.OnInventoryLoaded += LoadInventoryData;

        _storedItemDict = null;
        _equippedItemDict = null;
    }

    private void OnEnable()
    {
        Init();
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

        // // 아이템 추가 테스트
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     Item newItem = new Item("B0001", EItemType.Bomb, "기본 폭탄", "기본 폭탄", "Assets/05.Images/Item/PunIcon-128.png", null);
        //     InventoryItem testItem = new InventoryItem(new ItemDTO(newItem));

        //     AddItem(testItem);
        // }

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

    public void Init()
    {
        // 현재 카테고리 초기화
        CurrentCategory = EItemType.Head;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // 저장된 데이터 없을 시 아이템 보관함 초기화
        if (_storedItemDict == null)
        {
            _storedItemDict = new Dictionary<EItemType, List<InventoryItem>>();

            for (int i = 0; i < (int)EItemType.None; i++)
            {
                _storedItemDict.Add((EItemType)i, new List<InventoryItem>());
            }
        }

        // 저장된 데이터 없을 시 인벤토리 초기화
        if (_equippedItemDict == null)
        {
            _equippedItemDict = new Dictionary<EItemType, InventoryItem>((int)EItemType.None);
            for (int i = 0; i < (int)EItemType.None; i++)
            {
                _equippedItemDict.Add((EItemType)i, null);
            }
        }
    }

    private void LoadItemStorageData(Dictionary<EItemType, List<InventoryItem>> itemDict)
    {
        _storedItemDict = itemDict;

        OnDataChanged?.Invoke(CurrentCategory);
    }

    private void LoadInventoryData(Dictionary<EItemType, InventoryItem> equippedItemDict)
    {
        _equippedItemDict = equippedItemDict;

        OnDataChanged?.Invoke(CurrentCategory);
    }

    public List<InventoryItem> GetStoredItemList(EItemType itemType)
    {
        return _storedItemDict[itemType];
    }

    public InventoryItem GetEquppedItem(EItemType equipmentSlot)
    {
        if (_equippedItemDict[equipmentSlot] == null)
        {
            return null;
        }

        return _equippedItemDict[equipmentSlot];
    }

    public InventoryItem GetSelectedItem()
    {
        if (_selectedItem == null)
        {
            return null;
        }

        return _selectedItem;
    }

    public void ChangeCategory(EItemType nextCategory)
    {
        // 현재 카테고리 변경
        CurrentCategory = nextCategory;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
    }

    public void AddItem(InventoryItem newItem)
    {
        // null 검사
        if (newItem == null)
        {
            throw new Exception($"유효하지 않은 아이템입니다!");
        }

        // Item 객체 생성 및 컨테이너에 추가
        _storedItemDict[newItem.Item.ItemType].Add(newItem);

        // 현재 카테고리 변경
        CurrentCategory = newItem.Item.ItemType;

        // 데이터 저장
        _repo.SaveItemStorage(_storedItemDict);

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
    }

    public void SelectItem(InventoryItem item)
    {
        // 선택된 아이템 인덱스 검색
        int newSelectedItemIndex = _storedItemDict[item.Item.ItemType].FindIndex(x => x.ID == item.ID);

        if (newSelectedItemIndex == -1)
        {
            throw new Exception("선택된 아이템이 없습니다!");
        }

        _selectedItem = item;
        SelectedItemIndex = newSelectedItemIndex;

        OnDataChanged.Invoke(item.Item.ItemType);
    }

    public void EquipItem(InventoryItem item)
    {
        if (item == null)
        {
            throw new Exception("장착하려는 아이템이 없습니다!");
        }

        if (!_equippedItemDict.ContainsKey(item.Item.ItemType))
        {
            throw new Exception($"장착아이템 컨테이너에 해당 키가 없습니다 || {item.Item.ItemType}");
        }

        if (_equippedItemDict[item.Item.ItemType] != null)
        {
            InventoryItem equippedItem = _storedItemDict[item.Item.ItemType].Find(x => x.ID == _equippedItemDict[item.Item.ItemType].ID);
            equippedItem.UnEquip();
        }

        InventoryItem desiredItem = _storedItemDict[item.Item.ItemType].Find(x => x.ID == item.ID);
        desiredItem.Equip();

        _equippedItemDict[item.Item.ItemType] = desiredItem;

        _repo.SaveInventory(_equippedItemDict);
        _repo.SaveItemStorage(_storedItemDict);
        
        OnDataChanged?.Invoke(item.Item.ItemType);
    }

    public void UnEquipItem(InventoryItem item)
    {
        if (item == null)
        {
            throw new Exception("해제하려는 아이템이 없습니다!");
        }

        if (!_equippedItemDict.ContainsKey(item.Item.ItemType))
        {
            throw new Exception("해제하려는 아이템이 없습니다!");
        }

        InventoryItem desiredItem = _storedItemDict[item.Item.ItemType].Find(x => x.ID == item.ID);
        desiredItem.UnEquip();

        _equippedItemDict[item.Item.ItemType] = null;

        _repo.SaveInventory(_equippedItemDict);
        _repo.SaveItemStorage(_storedItemDict);

        OnDataChanged?.Invoke(item.Item.ItemType);
    }
}
