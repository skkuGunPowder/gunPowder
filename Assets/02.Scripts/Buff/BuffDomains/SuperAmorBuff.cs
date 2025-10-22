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

        // TODO
        // 플레이어 넉백 제거 설정
        _owner.IsSuperArmor = true;
        _isActive = true;
    }

    public override void EndBuff()
    {
        _owner.OnHit -= OnPlayerHit;

        // TODO
        // 플레이어 넉백 제거 해제 설정
        _owner.IsSuperArmor = false;
        _isActive = false;

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
