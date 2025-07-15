using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Inventory Instance;

    private Dictionary<EEquipmentSlot, Item> _equippedItemList;

    private InventoryRepo _repo;

    public event Action OnDataChanged;

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

    private void Init()
    {
        _repo = new InventoryRepo();
        _equippedItemList = _repo.LoadInventory();

        if (_equippedItemList == null)
        {
            _equippedItemList = new Dictionary<EEquipmentSlot, Item>();
        }
    }

    public void EquipItem(Item item)
    {
        if (item == null)
        {
            throw new Exception("장착하려는 아이템이 없습니다!");
        }


        if (_equippedItemList.ContainsKey(item.EquipmentSlot))
        {
            _equippedItemList[item.EquipmentSlot] = item;
        }
        else
        {
            _equippedItemList.Add(item.EquipmentSlot, item);
        }

        OnDataChanged?.Invoke();
    }

    public void UnEquipItem(Item item)
    {
        if (!_equippedItemList.ContainsKey(item.EquipmentSlot))
        {
            throw new Exception("해제하려는 아이템이 없습니다!");
        }

        _equippedItemList.Remove(item.EquipmentSlot);

        OnDataChanged?.Invoke();
    }
}
