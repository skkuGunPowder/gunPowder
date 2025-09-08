
using UnityEngine;

public class SpeedUpBuff : Buff, IBuff
{
    public override void Init()
    {
        base.Init();
        ID = "BF0001";
    }

    public override void StartBuff()
    {
        base.StartBuff();

        Debug.LogWarning("스피드 버프 시작");
        _owner.PlayerStat.MoveSpeed += Stat.ValueList[0];
    }

    public override void EndBuff()
    {
        Debug.LogWarning("스피드 버프 끝");
        _owner.PlayerStat.MoveSpeed -= Stat.ValueList[0];
        
        base.EndBuff();
    }
}
