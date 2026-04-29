using UnityEngine;

public abstract class Cartridge : MonoBehaviour, ICartridge
{
    protected CartridgeData _data;
    public CartridgeData Data => _data;
    protected int _currentDurability;

    public void Init(CartridgeData data)
    {
        _data = data;
        _currentDurability = _data.Durability;
    }

    public bool Repair()
    {
        if(_currentDurability >= _data.Durability)
        {
            Debug.LogWarning("카트리지의 내구도가 이미 최대입니다.");
            return false;
        }
        _currentDurability ++;
        return true;
    }

    public virtual bool ExcuteGimmick(Player owner)
    {
        if(_data.Durability == 0)
        {
            if(_currentDurability < 0)
            {
                Debug.LogWarning("카트리지의 내구도가 부족합니다.");
                return false;
            }
        }
        else
        {
            if(_currentDurability <= 0)
            {
                Debug.LogWarning("카트리지의 내구도가 부족합니다.");
                return false;
            }
        }
        
        _currentDurability--;
        return true;
    }

    public int GetMaxDurability()
    {
        return _data.Durability;
    }

    public int GetCurrentDurability()
    {
        return _currentDurability;
    }

    public void SetCurrentDurability(int durability)
    {
        _currentDurability = durability;
    }
}
