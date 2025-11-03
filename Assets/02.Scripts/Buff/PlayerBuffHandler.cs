using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerBuffHandler : MonoBehaviour
{
    private List<Buff> _buffs;

    public Action<Buff> OnBuffAdded;

    private void OnEnable()
    {
        GameObject hud = GameObject.FindWithTag("InGameHUD");
        if (hud != null)
        {
            UI_PlayerBuff uiPlayerBuff = hud.GetComponentInChildren<UI_PlayerBuff>();
            if (uiPlayerBuff != null)
            {
                uiPlayerBuff.Init(this);
            }
            else
            {
                Debug.LogError("UI_PlayerBuff를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("InGameHUD 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }

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
        OnBuffAdded?.Invoke(newBuff);
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
