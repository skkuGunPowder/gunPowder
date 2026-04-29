using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class CartridgeFactory : DontDestroySingleton<CartridgeFactory>
{
    [Serializable]
    private class CartridgePrefabEntry
    {
        public string ID;
        public Cartridge Prefab;
    }

    [SerializeField] private CartridgePrefabEntry[] _cartridgePrefabEntries;

    private CartridgeRepository _cartridgeRepository;
    private Dictionary<string, CartridgeData> _cartridgeDataDict = new Dictionary<string, CartridgeData>();
    private Dictionary<string, Cartridge> _cartridgeDict = new Dictionary<string, Cartridge>();
    private Dictionary<CartridgeRarity, List<string>> _cartridgeIDsByRarity = new Dictionary<CartridgeRarity, List<string>>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        _cartridgeRepository = new CartridgeRepository();
        InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            Dictionary<string, CartridgeData> loadedData = await _cartridgeRepository.LoadCartridgeDataAsync();
            _cartridgeDataDict = loadedData ?? new Dictionary<string, CartridgeData>();
            _cartridgeDict.Clear();
            Dictionary<string, Cartridge> prefabDictById = BuildPrefabDictionary();

            foreach (CartridgeData cartridgeData in _cartridgeDataDict.Values)
            {
                if (!prefabDictById.TryGetValue(cartridgeData.ID, out Cartridge prefabCartridge))
                {
                    Debug.LogWarning($"카트리지 ID에 해당하는 프리팹 매핑이 없습니다. (ID: {cartridgeData.ID})");
                    continue;
                }

                _cartridgeDict[cartridgeData.ID] = prefabCartridge;
                Cartridge cartridge = _cartridgeDict[cartridgeData.ID];
                cartridge.gameObject.SetActive(false);

                // 레어도별 딕셔너리에 등록
                if (!_cartridgeIDsByRarity.ContainsKey(cartridgeData.Rarity))
                {
                    _cartridgeIDsByRarity[cartridgeData.Rarity] = new List<string>();
                }
                _cartridgeIDsByRarity[cartridgeData.Rarity].Add(cartridgeData.ID);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"카트리지 초기화 실패: {e.Message}");
        }
    }

    private Dictionary<string, Cartridge> BuildPrefabDictionary()
    {
        Dictionary<string, Cartridge> prefabById = new Dictionary<string, Cartridge>();

        if (_cartridgePrefabEntries == null)
        {
            return prefabById;
        }

        foreach (CartridgePrefabEntry entry in _cartridgePrefabEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ID))
            {
                Debug.LogWarning("카트리지 프리팹 매핑에 비어있는 ID 항목이 있습니다.");
                continue;
            }

            if (entry.Prefab == null)
            {
                Debug.LogError($"카트리지 프리팹 매핑에 프리팹이 비어있습니다. (ID: {entry.ID})");
                continue;
            }

            if (prefabById.ContainsKey(entry.ID))
            {
                Debug.LogWarning($"중복된 카트리지 프리팹 매핑 ID가 있습니다. 마지막 항목으로 덮어씁니다. (ID: {entry.ID})");
            }

            prefabById[entry.ID] = entry.Prefab;
        }

        return prefabById;
    }
    
    public string[] GetRandomCartridgeIDs(int count)
    {
        // 가중치 테이블 (Common 45, Rare 35, Epic 20)
        (CartridgeRarity rarity, int weight)[] weights = new (CartridgeRarity rarity, int weight)[]
        {
            (CartridgeRarity.Common, 45),
            (CartridgeRarity.Rare,   35),
            (CartridgeRarity.Epic,   20),
        };

        List<string> selectedIDs = new List<string>();
        HashSet<string> used = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            int totalWeight = 0;
            foreach ((CartridgeRarity rarity, int weight) in weights)
            {
                if (_cartridgeIDsByRarity.TryGetValue(rarity, out List<string> list) &&
                    list.Exists(id => !used.Contains(id)))
                {
                    totalWeight += weight;
                }
            }

            if (totalWeight == 0)
            {
                break;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            string picked = null;

            foreach ((CartridgeRarity rarity, int weight) in weights)
            {
                if (!_cartridgeIDsByRarity.TryGetValue(rarity, out List<string> list))
                {
                    continue;
                }

                List<string> available = list.FindAll(id => !used.Contains(id));
                if (available.Count == 0)
                {
                    continue;
                }

                cumulative += weight;
                if (roll < cumulative)
                {
                    picked = available[UnityEngine.Random.Range(0, available.Count)];
                    break;
                }
            }

            if (picked == null)
            {
                break;
            }

            used.Add(picked);
            selectedIDs.Add(picked);
        }

        return selectedIDs.ToArray();
    }

    public CartridgeData GetCartridgeData(string id)
    {
        if (!_cartridgeDataDict.TryGetValue(id, out CartridgeData cartridgeData))
        {
            Debug.LogError($"카트리지를 찾지 못했습니다.(ID: {id})");
            return null;
        }
        
        return cartridgeData;
    }

    public Cartridge GetCartridge(string id)
    {
        if (!_cartridgeDataDict.TryGetValue(id, out CartridgeData cartridgeData))
        {
            Debug.LogError($"카트리지를 찾지 못했습니다.(ID: {id})");
            return null;
        }

        if (!_cartridgeDict.TryGetValue(id, out Cartridge cachedCartridge))
        {
            Debug.LogError($"카트리지를 찾지 못했습니다.(ID: {id})");
            return null;
        }

        GameObject newCartridgeObject = Instantiate(cachedCartridge.gameObject);
        Cartridge newCartridge = newCartridgeObject.GetComponent<Cartridge>();
        newCartridge.Init(cartridgeData);
        return newCartridge;
    }

    // public int GetMaxDurability(string id)
    // {
    //     if (!_cartridgeDataDict.TryGetValue(id, out CartridgeData cartridgeData))
    //     {
    //         Debug.LogError($"카트리지를 찾지 못했습니다.(ID: {id})");
    //         return 0;
    //     }

    //     return cartridgeData.Durability;
    // }

    #region 테스트용
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            string id = "CT0001";
            TestCartridge("CT0001");
            //CartridgeInventoryManager.Instance.AddCartridge(id);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            string id = "CT0011";
            TestCartridge("CT0011");
            //CartridgeInventoryManager.Instance.AddCartridge(id);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            string id = "CT0014";
            TestCartridge("CT0014");
            //CartridgeInventoryManager.Instance.AddCartridge(id);
            
        }
    }

    private void TestCartridge(string id)
    {
        Player owner = FindLocalPlayer();
        if (owner == null) { Debug.LogWarning("[CartridgeTest] 로컬 플레이어를 찾을 수 없습니다."); return; }

        Cartridge cartridge = GetCartridge(id);
        if (cartridge == null) return;

        cartridge.ExcuteGimmick(owner);
        Debug.Log($"[CartridgeTest] {id} 실행 (Rarity: {_cartridgeDataDict[id].Rarity}, Values: [{string.Join(", ", _cartridgeDataDict[id].GimmickValues)}])");
        Destroy(cartridge.gameObject);
    }

    private Player FindLocalPlayer()
    {
        foreach (var p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (p.PhotonView != null && p.PhotonView.IsMine) return p;
        }
        return null;
    }
    #endregion

}
