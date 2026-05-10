using UnityEngine;

public abstract class Cartridge : MonoBehaviour, ICartridge
{
    protected CartridgeData _data;
    public CartridgeData Data => _data;
    protected int _currentDurability;
    private int _repairCount;
    private bool _repairedThisTurn;

    public void Init(CartridgeData data)
    {
        _data = data;
        _currentDurability = _data.Durability;
        _repairCount = 0;
        _repairedThisTurn = false;
    }

    public bool Repair()
    {
        if (_repairedThisTurn)
        {
            Debug.LogWarning("이번 턴에 이미 수리한 카트리지입니다.");
            return false;
        }
        if (_currentDurability >= _data.Durability)
        {
            Debug.LogWarning("카트리지의 내구도가 이미 최대입니다.");
            return false;
        }
        _currentDurability = _data.Durability;
        _repairCount++;
        _repairedThisTurn = true;
        return true;
    }

    public void ResetTurnRepairFlag()
    {
        _repairedThisTurn = false;
    }

    public bool IsRepairedThisTurn()
    {
        return _repairedThisTurn;
    }

    public int GetRepairCount()
    {
        return _repairCount;
    }

    public void SetRepairCount(int count)
    {
        _repairCount = count;
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
