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

    public virtual void ExcuteGimmick(Player owner)
    {
        if(_currentDurability <= 0)
        {
            Debug.LogWarning("카트리지의 내구도가 부족합니다.");
            return;
        }
        _currentDurability--;
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
