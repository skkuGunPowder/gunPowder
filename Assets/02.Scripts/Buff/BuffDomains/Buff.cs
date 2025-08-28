using System;
using UnityEngine;

public abstract class Buff : MonoBehaviour, IBuff
{
    public string ID { get; protected set; }
    public BuffStat Stat { get; private set; }

    protected Player _owner;
    protected float _timer;
    protected bool _isActive = false;

    public void SetStat(BuffStat stat)
    {
        if (stat == null)
        {
            throw new Exception("Stat이 비어있습니다.");
        }

        Stat = stat;

        _owner = null;
        _timer = 0f;
        _isActive = false;
    }

    public void SetOwner(Player player)
    {
        if (player != null)
        {
            throw new Exception("플레이어가 없습니다.");
        }

        _owner = player;
    }

    public virtual void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer > Stat. Duration)
        {
            EndBuff();
        }
    }

    public virtual void StartBuff()
    {
        if (_isActive)
        {
            return;
        }

        _isActive = true;
    }

    public virtual void EndBuff()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        _timer = 0f;
        Destroy(this);
    }
}
