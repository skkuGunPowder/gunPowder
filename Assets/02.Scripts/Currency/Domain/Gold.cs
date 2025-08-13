using UnityEngine;
using System;

public class Gold
{
    private int _amount;

    public Gold(int amount)
    {
        if (amount < 0)
        {
            throw new Exception("금액은 0 이상이어야 합니다.");
        }
        _amount = amount;
    }

    public int GetAmount()
    {
        return _amount;
    }

    public void Add(int amount)
    {
        if (amount < 0)
        {
            throw new Exception("추가할 금액은 0 이상이어야 합니다.");
        }
        _amount += amount;
    }

    public bool Subtract(int amount)
    {
        if (amount < 0)
        {
            throw new Exception("차감할 금액은 0 이상이어야 합니다.");
        }

        if (_amount < amount)
        {
            Debug.LogWarning($"금액이 부족합니다. 현재 금액: {_amount} | 차감할 금액: {amount}");
            return false;
        }

        _amount -= amount;
        return true;
    }
}
