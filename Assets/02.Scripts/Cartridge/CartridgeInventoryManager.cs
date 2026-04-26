using System.Collections.Generic;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CartridgeInventoryManager : PhotonSingleton<CartridgeInventoryManager>
{
    // CartridgeData.Durability == 0 (슬롯 제한 X, 같은 ID 중복 구매로 count 누적)
    private readonly Dictionary<string, int> _consumableCartridges = new Dictionary<string, int>();

    // CartridgeData.Durability >= 1 (슬롯 제한 O, 구매 시 max로 충전)
    private readonly Dictionary<string, int> _permanentCartridges = new Dictionary<string, int>();

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

        LoadFromCustomProperties();
    }

    public void AddCartridge(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        int maxDurability = CartridgeFactory.Instance.GetMaxDurability(id);

        if (maxDurability <= 0)
        {
            if (!_consumableCartridges.TryGetValue(id, out int current))
            {
                current = 0;
            }
            _consumableCartridges[id] = current + 1;
        }
        else
        {
            _permanentCartridges[id] = maxDurability;
        }

        SyncToCustomProperties();
    }

    public void RemoveCartridge(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        bool removed = _consumableCartridges.Remove(id);
        removed |= _permanentCartridges.Remove(id);

        if (!removed)
        {
            return;
        }

        SyncToCustomProperties();
    }

    // 라운드 시작 시 효과를 받아야 할 카트리지 ID 목록 (count > 0)
    public IReadOnlyList<string> GetActiveCartridgeIds()
    {
        if (!_loaded)
        {
            LoadFromCustomProperties();
        }

        List<string> ids = new List<string>();

        foreach (KeyValuePair<string, int> kvp in _consumableCartridges)
        {
            if (kvp.Value > 0)
            {
                ids.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<string, int> kvp in _permanentCartridges)
        {
            if (kvp.Value > 0)
            {
                ids.Add(kvp.Key);
            }
        }

        return ids;
    }

    public int GetCount(string id)
    {
        if (_consumableCartridges.TryGetValue(id, out int consumable))
        {
            return consumable;
        }

        if (_permanentCartridges.TryGetValue(id, out int permanent))
        {
            return permanent;
        }

        return 0;
    }

    // 단건 차감 (트리거형 카트리지 사용 시점)
    public bool Consume(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (_consumableCartridges.TryGetValue(id, out int consumable) && consumable > 0)
        {
            _consumableCartridges[id] = consumable - 1;
            SyncToCustomProperties();
            return true;
        }

        if (_permanentCartridges.TryGetValue(id, out int permanent) && permanent > 0)
        {
            _permanentCartridges[id] = permanent - 1;
            SyncToCustomProperties();
            return true;
        }

        return false;
    }

    // 라운드 시작 시 한 번에 모든 카트리지 count -1 (한 번만 동기화)
    public void ConsumeAll()
    {
        bool changed = false;

        List<string> consumableKeys = new List<string>(_consumableCartridges.Keys);
        foreach (string id in consumableKeys)
        {
            int current = _consumableCartridges[id];
            if (current <= 0)
            {
                continue;
            }
            _consumableCartridges[id] = current - 1;
            changed = true;
        }

        List<string> permanentKeys = new List<string>(_permanentCartridges.Keys);
        foreach (string id in permanentKeys)
        {
            int current = _permanentCartridges[id];
            if (current <= 0)
            {
                continue;
            }
            _permanentCartridges[id] = current - 1;
            changed = true;
        }

        if (changed)
        {
            SyncToCustomProperties();
        }
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

    private void SyncToCustomProperties()
    {
        if (PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        List<string> serialized = new List<string>();

        foreach (KeyValuePair<string, int> kvp in _consumableCartridges)
        {
            serialized.Add($"{ConsumablePrefix}{EntrySeparator}{kvp.Key}{EntrySeparator}{kvp.Value}");
        }

        foreach (KeyValuePair<string, int> kvp in _permanentCartridges)
        {
            serialized.Add($"{PermanentPrefix}{EntrySeparator}{kvp.Key}{EntrySeparator}{kvp.Value}");
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
            if (tokens.Length != 3)
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
                _consumableCartridges[id] = count;
            }
            else if (prefix == PermanentPrefix)
            {
                _permanentCartridges[id] = count;
            }
        }
    }
}
