using System;
using UnityEngine;

public class Buff : MonoBehaviour, IBuff
{
    public string ID { get; protected set; }
    public BuffStat Stat { get; private set; }
    public Sprite Icon;

    protected Player _owner;
    public float _timer;
    protected bool _isActive = false;

    public virtual void Init()
    {
        _timer = 0f;
        _isActive = false;
    }

    public void SetStat(BuffStat stat)
    {
        if (stat == null)
        {
            Debug.LogError("Stat이 비어있습니다.");
            // throw new Exception("Stat이 비어있습니다.");
        }

        Stat = stat;
    }

    public void SetOwner(Player player)
    {
        if (player == null)
        {
            throw new Exception("플레이어가 없습니다.");
        }

        _owner = player;
    }

    public void ResetTimer()
    {
        _timer = 0f;
    }

    public virtual void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer > Stat.Duration)
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
        _owner.PlayerBuffHandler.RemoveBuff(this);
    }
}
