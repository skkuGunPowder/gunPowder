using UnityEngine;

public class SuperAmorBuff : Buff
{
    [SerializeField] private int _maxHitCount;
    private int _currentHitCount;

    public override void Init()
    {
        base.Init();
        ID = "BF0003";
    }

    public override void StartBuff()
    {
        base.StartBuff();

        _currentHitCount = 0;

        _owner.OnHit += OnPlayerHit;

        // 플레이어 슈퍼아머 활성화 (넉백 무효화 및 위치 고정)
        _owner.SetSuperArmor();
        _isActive = true;
    }

    public override void EndBuff()
    {
        _owner.OnHit -= OnPlayerHit;

        // 플레이어 슈퍼아머 비활성화 (원래 상태로 복원)
        _owner.ResetSuperArmor();
        base.EndBuff();
    }
    
    public override void Update()
    {
        
    }

    private void OnPlayerHit()
    {
        _currentHitCount++;

        if (_currentHitCount >= _maxHitCount)
        {
            EndBuff();
        }
    }
}
