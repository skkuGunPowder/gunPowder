using UnityEngine;
using System.Collections.Generic;

public class CartridgeFactory : DontDestroySingleton<CartridgeFactory>
{
    [SerializeField] private GameObject[] _cartridgePrefabs;

    private Dictionary<string, CartridgeData> _cartridgeDataDict = new Dictionary<string, CartridgeData>();
    private Dictionary<string, Cartridge> _cartridgeDict = new Dictionary<string, Cartridge>();

    protected override void Awake()
    {
        base.Awake();
        // TODO: 뒤끝으로부터 카트리지 데이터 로드
        Init();
    }

    private void Init()
    {
        // TODO: 카트리지 데이터 로드
        // _cartridgeDataDict.Add("cartridge_001", new CartridgeData());
        // _cartridgeDataDict.Add("cartridge_002", new CartridgeData());

        int index = 0;
        foreach (var cartridgeData in _cartridgeDataDict.Values)
        {
            if(index >= _cartridgePrefabs.Length)
            {
                Debug.LogWarning("카트리지 프리팹이 로드한 카트리지 데이터보다 적습니다.");
                break;
            }

            _cartridgeDict.Add(cartridgeData.ID, _cartridgePrefabs[index].GetComponent<Cartridge>());
            Cartridge cartridge = _cartridgeDict[cartridgeData.ID];
            cartridge.gameObject.SetActive(false);

            index++;
        }
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
}
