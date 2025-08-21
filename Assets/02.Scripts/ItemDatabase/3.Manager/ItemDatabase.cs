using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase
{
    public static ItemDatabase Instance { get; private set; } = new ItemDatabase();

    private Dictionary<string, Item> _items;
    private Dictionary<string, IStat> _stats;

    private ItemDatabaseRepo _repo;


    public void Init()
    {
        _repo = new ItemDatabaseRepo();
        _repo.OnitemDataLoaded += SetData;
        _repo.Init();
    }

    public void SetData(Dictionary<string, Item> itemDict, Dictionary<string, IStat> statDict)
    {
        _items = itemDict;
        _stats = statDict;
        
        if (_items == null)
        {
            throw new System.Exception("아이템 데이터를 불러오는데 실패하였습니다.");
        }

        if (_stats == null)
        {
            throw new System.Exception("아이템 스탯 데이터를 불러오는데 실패하였습니다.");
        }

        PhotonServerManager.Instance.SetPhotonPrefabPool(_items);
    }

    public ItemDTO GetItem(string itemID)
    {
        if (!_items.TryGetValue(itemID, out Item item))
        {
            Debug.LogError($"{this} || 존재하지 않는 아이템입니다. ({itemID})");
            return null;
        }
        return new ItemDTO(item);
    }

    public T GetStat<T>(string itemID) where T : class, IStat
    {      
        if (!_stats.TryGetValue(itemID, out IStat stat))
        {
            Debug.LogError($"{this} || 존재하지 않는 아이템이거나, 스탯이 존재하지 않는 아이템입니다. ({itemID})");
            return null;
        }
        return stat as T;
    }
}
