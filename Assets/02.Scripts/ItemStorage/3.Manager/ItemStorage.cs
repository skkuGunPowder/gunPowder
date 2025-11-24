using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;


public class ItemStorage : DontDestroySingleton<ItemStorage>
{
    private Dictionary<EItemType, List<InventoryItem>> _storedItemDict;
    private Dictionary<EItemType, InventoryItem> _equippedItemDict;

    public EMainCategory CurrentMainCategory { get; private set; }
    public EItemType CurrentCategory { get; private set; }
    public int SelectedItemIndex { get; private set; }
    private InventoryItem _selectedItem;

    private ItemStorageRepo _repo;

    public event Action<EItemType> OnDataChanged;


    protected override void Awake()
    {
        base.Awake();

        _storedItemDict = null;
        _equippedItemDict = null;

        _repo = new ItemStorageRepo();

        // 현재 카테고리 초기화
        CurrentMainCategory = EMainCategory.Bomb;
        CurrentCategory = EItemType.Bomb;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        Init();
    }

    private async void Init()
    {
        _storedItemDict = await _repo.LoadItemStorage();
        _equippedItemDict = await _repo.LoadInventory();

        // 저장된 데이터 없을 시 아이템 보관함 초기화
        if (_storedItemDict == null)
        {
            _storedItemDict = new Dictionary<EItemType, List<InventoryItem>>();

            for (int i = 0; i < (int)EItemType.None; i++)
            {
                _storedItemDict.Add((EItemType)i, new List<InventoryItem>());

                // 스타터 아이템 지급(미사일)
                if ((EItemType)i == EItemType.Bomb)
                {
                    _storedItemDict[EItemType.Bomb].Add(new InventoryItem(ItemDatabase.Instance.GetItem("BO0005")));
                    _storedItemDict[EItemType.Bomb].Add(new InventoryItem(ItemDatabase.Instance.GetItem("BO0007")));
                    _storedItemDict[EItemType.Bomb].Add(new InventoryItem(ItemDatabase.Instance.GetItem("BO0011")));
                    _storedItemDict[EItemType.Bomb].Add(new InventoryItem(ItemDatabase.Instance.GetItem("BO0018")));
                }

                if ((EItemType)i == EItemType.Face)
                {
                    _storedItemDict[EItemType.Face].Add(new InventoryItem(ItemDatabase.Instance.GetItem("SK0001")));
                }
            }

            _repo.SaveItemStorage(_storedItemDict);
        }

        // 저장된 데이터 없을 시 인벤토리 초기화
        if (_equippedItemDict == null)
        {
            _equippedItemDict = new Dictionary<EItemType, InventoryItem>((int)EItemType.None);
            for (int i = 0; i < (int)EItemType.None; i++)
            {
                _equippedItemDict.Add((EItemType)i, null);
            }
            _repo.SaveInventory(_equippedItemDict);
        }

        // 스타터 아이템 장착(미사일)
        if (_equippedItemDict[EItemType.Bomb] == null)
        {
            EquipItem(_storedItemDict[EItemType.Bomb][0]);
        }

        SetPlayerCustomProperties();
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

    public void ChangeMainCategory(EMainCategory nextMainCategory)
    {
        if (nextMainCategory == CurrentMainCategory)
        {
            return;
        }

        // 현재 카테고리 변경
        CurrentMainCategory = nextMainCategory;
        if (CurrentMainCategory == EMainCategory.Character)
        {
            CurrentCategory = EItemType.Head;
        }
        else
        {
            CurrentCategory = EItemType.Bomb;
        }


        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
    }

    public void ChangeCategory(EItemType nextCategory)
    {
        if (nextCategory == CurrentCategory)
        {
            return;
        }

        // 현재 카테고리 변경
        CurrentCategory = nextCategory;

        // 선택된 아이템 인덱스 초기화
        SelectedItemIndex = -1;

        // UI 업데이트
        OnDataChanged?.Invoke(CurrentCategory);
    }

    public void AddItem(string itemID)
    {
        // null 검사
        if (string.IsNullOrEmpty(itemID))
        {
            throw new Exception($"유효하지 않은 아이템입니다!");
        }

        InventoryItem newItem = new InventoryItem(ItemDatabase.Instance.GetItem(itemID));

        // Item 객체 생성 및 컨테이너에 추가
        _storedItemDict[newItem.Item.ItemType].Add(newItem);

        // 현재 카테고리 변경
        CurrentCategory = newItem.Item.ItemType;

        // 데이터 저장
        _repo.SaveItemStorage(_storedItemDict);

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

        SetPlayerCustomProperties();
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

        if(item.Item.ItemType == EItemType.Bomb)
        {
            return;
        }

        InventoryItem desiredItem = _storedItemDict[item.Item.ItemType].Find(x => x.ID == item.ID);
        desiredItem.UnEquip();

        _equippedItemDict[item.Item.ItemType] = null;

        _repo.SaveInventory(_equippedItemDict);
        _repo.SaveItemStorage(_storedItemDict);

        SetPlayerCustomProperties();
        OnDataChanged?.Invoke(item.Item.ItemType);
    }

    public void SetPlayerCustomProperties()
    {
        Hashtable equipedItems = new Hashtable();
        foreach (var kvp in _equippedItemDict)
        {
            // 장착 해제된 슬롯은 null을 전송해 커스텀 프로퍼티에서 키를 제거한다
            if (kvp.Value != null)
            {
                equipedItems[kvp.Key.ToString()] = kvp.Value.ID;
            }
            else
            {
                equipedItems[kvp.Key.ToString()] = null;
            }
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(equipedItems);
    }
}
