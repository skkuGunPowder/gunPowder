using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class CartridgeInventoryManager : PhotonSingleton<CartridgeInventoryManager>
{
    // CartridgeData.Durability == 0 (슬롯 제한 X, 같은 ID 중복 구매로 count 누적)
    private Dictionary<string, Cartridge> _consumableCartridges = new Dictionary<string, Cartridge>();

    // CartridgeData.Durability >= 1 (슬롯 제한 O, 구매 시 max로 충전)
    private Dictionary<string, Cartridge> _permanentCartridges = new Dictionary<string, Cartridge>();

    private const string ConsumablePrefix = "C";
    private const string PermanentPrefix = "P";
    private const char EntrySeparator = '|';

    private bool _loaded;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerEventManager.Instance.GetEvents(actorNumber).OnSpawned -= LoadFromCustomProperties;
        PlayerEventManager.Instance.GetEvents(actorNumber).OnSpawned += LoadFromCustomProperties;
        LoadFromCustomProperties();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        ClearAll();
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerEventManager.Instance.GetEvents(actorNumber).OnSpawned -= LoadFromCustomProperties;
    }

    public bool AddCartridge(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("카트리지 ID가 Null 혹은 비어있습니다.");
            return false;
        }

        if(_permanentCartridges.TryGetValue(id, out Cartridge existingPermanent))
        {
            Debug.Log($"이미 보유 중인 영구형 카트리지{id} 수리 시도.");
            return Repair(existingPermanent.Data.ID);
        }

        if(_consumableCartridges.ContainsKey(id))
        {
            Debug.LogWarning($"이미 보유 중인 소모형 카트리지{id}입니다.");
            return false;
        }

        Cartridge newCartridge = CartridgeFactory.Instance.GetCartridge(id);
        int maxDurability = newCartridge.GetMaxDurability();

        if (maxDurability == 0)
        {
            _consumableCartridges.Add(id, newCartridge);
        }
        else
        {
            _permanentCartridges.Add(id, newCartridge);
        }

        SyncToCustomProperties();
        return true;
    }

    public void RemoveCartridge(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("카트리지 ID가 Null 혹은 비어있습니다.");
            return;
        }

        if (_consumableCartridges.Remove(id) || _permanentCartridges.Remove(id))
        {
            Debug.Log($"카트리지 제거 성공. ID: {id}");
            SyncToCustomProperties();
        }
        else
        {
            Debug.LogWarning($"카트리지 제거 실패. 해당 ID의 카트리지가 인벤토리에 없습니다. ID: {id}");       
        }
    }

    public int GetCount(string id)
    {
        if (_consumableCartridges.TryGetValue(id, out Cartridge consumable))
        {
            return consumable.GetCurrentDurability();
        }

        if (_permanentCartridges.TryGetValue(id, out Cartridge permanent))
        {
            return permanent.GetCurrentDurability();
        }

        return 0;
    }

    // 단건 차감 (트리거형 카트리지 사용 시점)
    public bool Consume(string id, Player owner)
    {
        // 결과 반환 : true - 사용 성공, false - 사용 실패 (내구도 부족, 해당 카트리지 없음 등)
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (_consumableCartridges.TryGetValue(id, out Cartridge consumable))
        {
            if(!CheckCartridgeUsable(consumable)) return false;

            consumable.ExcuteGimmick(owner);
            SyncToCustomProperties();
            return true;
        }

        if (_permanentCartridges.TryGetValue(id, out Cartridge permanent))
        {
            if(!CheckCartridgeUsable(permanent)) return false;

            permanent.ExcuteGimmick(owner);
            SyncToCustomProperties();
            return true;
        }

        Debug.LogWarning($"카트리지 사용 실패. 해당 ID의 카트리지가 인벤토리에 없습니다. ID: {id}");
        return false;
    }

    // 라운드 시작 시 한 번에 모든 카트리지 count -1 (한 번만 동기화)
    public void ConsumeAll(Player owner)
    {
        List<Cartridge> consumables = new List<Cartridge>(_consumableCartridges.Values);
        List<Cartridge> permanents = new List<Cartridge>(_permanentCartridges.Values);
        foreach (Cartridge consumable in consumables)
        {
            if(!CheckCartridgeUsable(consumable)) continue;
            consumable.ExcuteGimmick(owner);
        }

        foreach (Cartridge permanent in permanents)
        {
            if(!CheckCartridgeUsable(permanent)) continue;
            permanent.ExcuteGimmick(owner);
        }

        SyncToCustomProperties();
    }

    public void ClearAll()
    {
        _consumableCartridges.Clear();
        _permanentCartridges.Clear();

        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        Hashtable props = new Hashtable
        {
            { EProperties.Cartridges.ToString(), null }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // 내부 수리 (AddCartridge에서 구매 가격으로 처리되는 경로)
    public bool Repair(string id)
    {
        if (_consumableCartridges.ContainsKey(id))
        {
            Debug.LogError($"소모형 카트리지{id}는 수리가 불가능합니다.");
            return false;
        }

        if (_permanentCartridges.TryGetValue(id, out Cartridge permanent))
        {
            if (!permanent.Repair()) return false;
            SyncToCustomProperties();
            return true;
        }

        return false;
    }

    // 수리 팝업에서 호출 — 빚 체크, GP 차감 포함
    public bool TryRepairWithCost(string id)
    {
        if (_consumableCartridges.ContainsKey(id))
        {
            Debug.LogError($"소모형 카트리지{id}는 수리가 불가능합니다.");
            return false;
        }

        if (!_permanentCartridges.TryGetValue(id, out Cartridge permanent))
        {
            return false;
        }

        if (RoomStatManager.Instance.PlayerGunpowder < 0)
        {
            Debug.LogWarning("빚이 있는 상태에서는 수리할 수 없습니다.");
            return false;
        }

        int repairCost = permanent.Data.GetRepairCost(permanent.GetRepairCount());
        if (!RoomStatManager.Instance.CanChangeGP(-repairCost))
        {
            Debug.LogWarning("GP가 부족하여 수리할 수 없습니다.");
            return false;
        }

        if (!permanent.Repair()) return false;

        RoomStatManager.Instance.ChangeGunpowder(-repairCost);
        SyncToCustomProperties();
        return true;
    }

    public void ResetTurnRepairFlags()
    {
        foreach (KeyValuePair<string, Cartridge> kvp in _permanentCartridges)
        {
            kvp.Value.ResetTurnRepairFlag();
        }
    }

    public int GetDiscountedPrice(string id)
    {
        CartridgeData data = CartridgeFactory.Instance.GetCartridgeData(id);
        int basePrice = data.GetPrice();

        // 이미 보유 중인 카트리지는 반값
        if (_permanentCartridges.ContainsKey(id) || _consumableCartridges.ContainsKey(id))
        {
            return basePrice / 2;
        }

        return basePrice;
    }

    public Dictionary<string, Cartridge> GetPermanentCartridges()
    {
        return _permanentCartridges;
    }

    private void SyncToCustomProperties()
    {
        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        List<string> serialized = new List<string>();

        foreach (KeyValuePair<string, Cartridge> kvp in _consumableCartridges)
        {
            serialized.Add($"{ConsumablePrefix}{EntrySeparator}{kvp.Key}{EntrySeparator}{kvp.Value.GetCurrentDurability()}");
        }

        foreach (KeyValuePair<string, Cartridge> kvp in _permanentCartridges)
        {
            serialized.Add($"{PermanentPrefix}{EntrySeparator}{kvp.Key}{EntrySeparator}{kvp.Value.GetCurrentDurability()}{EntrySeparator}{kvp.Value.GetRepairCount()}");
        }

        Hashtable props = new Hashtable
        {
            { EProperties.Cartridges.ToString(), serialized.ToArray() }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void LoadFromCustomProperties()
    {
        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        _loaded = true;

        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(EProperties.Cartridges.ToString(), out object value))
        {
            return;
        }

        if (!(value is string[] entries))
        {
            return;
        }

        _consumableCartridges.Clear();
        _permanentCartridges.Clear();

        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
            {
                continue;
            }

            string[] tokens = entry.Split(EntrySeparator);
            if (tokens.Length < 3)
            {
                continue;
            }

            if (!int.TryParse(tokens[2], out int count))
            {
                continue;
            }

            string prefix = tokens[0];
            string id = tokens[1];

            if (prefix == ConsumablePrefix)
            {
                _consumableCartridges.Add(id, CartridgeFactory.Instance.GetCartridge(id));
                _consumableCartridges[id].SetCurrentDurability(count);
            }
            else if (prefix == PermanentPrefix)
            {
                _permanentCartridges.Add(id, CartridgeFactory.Instance.GetCartridge(id));
                _permanentCartridges[id].SetCurrentDurability(count);
                if (tokens.Length >= 4 && int.TryParse(tokens[3], out int repairCount))
                {
                    _permanentCartridges[id].SetRepairCount(repairCount);
                }
            }
        }
    }

    private bool CheckCartridgeUsable(Cartridge cartridge)
    {
        // 결과 반환 : true - 사용가능, false - 사용불가 (내구도 부족)
        if (cartridge.GetMaxDurability() == 0)
        {
            if (cartridge.GetCurrentDurability() != 0)
            {
                Debug.LogError($"소모형 카트리지{cartridge.Data.ID} 제거. Count: {cartridge.GetCurrentDurability()}");
                RemoveCartridge(cartridge.Data.ID);
                return false;
            }
        }
        else
        {
            if (cartridge.GetCurrentDurability() <= 0)
            {
                Debug.LogError($"영구형 카트리지{cartridge.Data.ID} 제거. Count: {cartridge.GetCurrentDurability()}");
                RemoveCartridge(cartridge.Data.ID);
                return false;
            }
        }
        
        return true;
    }
}
