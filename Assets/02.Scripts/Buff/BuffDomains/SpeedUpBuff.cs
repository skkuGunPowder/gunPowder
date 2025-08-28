
using UnityEngine;

public class SpeedUpBuff : Buff, IBuff
{
    private void Awake()
    {
        ID = "BF0001";
    }

    public override void StartBuff()
    {
        base.StartBuff();

        Debug.LogWarning("스피드 버프 시작");
        _owner.PlayerStat.MoveSpeed += Stat.Value;
    }

    public override void EndBuff()
    {
        Debug.LogWarning("스피드 버프 끝");
        _owner.PlayerStat.MoveSpeed -= Stat.Value;
        
        base.EndBuff();
    }
}
