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

            foreach (var cartridgeData in _cartridgeDataDict.Values)
            {
                if (!prefabDictById.TryGetValue(cartridgeData.ID, out Cartridge prefabCartridge))
                {
                    Debug.LogWarning($"카트리지 ID에 해당하는 프리팹 매핑이 없습니다. (ID: {cartridgeData.ID})");
                    continue;
                }

                _cartridgeDict[cartridgeData.ID] = prefabCartridge;
                Cartridge cartridge = _cartridgeDict[cartridgeData.ID];
                cartridge.gameObject.SetActive(false);
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

    public int GetMaxDurability(string id)
    {
        if (!_cartridgeDataDict.TryGetValue(id, out CartridgeData cartridgeData))
        {
            Debug.LogError($"카트리지를 찾지 못했습니다.(ID: {id})");
            return 0;
        }

        return cartridgeData.Durability;
    }

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
