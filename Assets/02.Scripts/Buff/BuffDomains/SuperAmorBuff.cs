using UnityEngine;

public class SuperAmorBuff : Buff
{
    [SerializeField] private int _maxHitCount;
    private int _currentHitCount;

    public override void StartBuff()
    {
        base.StartBuff();

        _currentHitCount = 0;

        // TODO
        // EventManager.OnPlayerHitEvent += OnPlayerHit;

        // TODO
        // 플레이어 넉백 제거 설정
    }
    
    public override void EndBuff()
    {
        base.EndBuff();

        // TODO
        // EventManager.OnPlayerHitEvent -= OnPlayerHit;

        // TODO
        // 플레이어 넉백 제거 해제 설정
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
