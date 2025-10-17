using UnityEngine;
using System.Collections.Generic;

public class PlayerBuffHandler : MonoBehaviour
{
    private List<Buff> _buffs;

    private void OnEnable()
    {
        if (_buffs == null)
        {
            _buffs = new List<Buff>();
        }
        else
        {
            ClearBuffs();
        }
    }

    public void AddBuff(Buff newBuff)
    {
        foreach (Buff buff in _buffs)
        {
            if (buff.ID == newBuff.ID)
            {
                buff.ResetTimer();
                return;
            }
        }

        _buffs.Add(newBuff);
        newBuff.StartBuff();
    }

    public void RemoveBuff(string buffID)
    {
        foreach (Buff buff in _buffs)
        {
            if (buff.ID == buffID)
            {
                buff.EndBuff();
                _buffs.Remove(buff);
                Destroy(buff.gameObject);
                return;
            }
        }

        Debug.LogWarning($"[{buffID}] 버프를 찾을 수 없습니다.");
    }

    private void ClearBuffs()
    {
        foreach (Buff buff in _buffs)
        {
            buff.EndBuff();
            Destroy(buff.gameObject);
        }
        _buffs.Clear();
    }
}
