using UnityEngine;

public abstract class Cartridge : MonoBehaviour, ICartridge
{
    protected CartridgeData _data;
    protected int _currentDurability;

    public void Init(CartridgeData data)
    {
        _data = data;
        _currentDurability = _data.Durability;
    }

    public void Repair()
    {
        _currentDurability = _data.Durability;
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
}
