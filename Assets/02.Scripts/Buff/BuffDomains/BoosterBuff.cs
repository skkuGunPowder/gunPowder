using UnityEngine;

public class BoosterBuff : Buff, IBuff
{
    public override void Init()
    {
        base.Init();
        ID = "BF0002";
    }

    public override void StartBuff()
    {
        base.StartBuff();

        Debug.LogWarning("부스트 버프 시작");
        _owner.PlayerStat.MaxJumpCount = Stat.ValueList[0];
    }

    public override void EndBuff()
    {
        Debug.LogWarning("부스트 버프 끝");
        _owner.PlayerStat.MaxJumpCount = 2;
        
        base.EndBuff();
    }
}
