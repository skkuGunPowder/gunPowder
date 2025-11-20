using UnityEngine;

public class ConfuseDebuff : Buff, IBuff
{
    public override void Init()
    {
        base.Init();
        ID = "BF0003";
    }

    public override void StartBuff()
    {
        base.StartBuff();

        Debug.LogWarning("혼란 디버프 시작");
    }

    public override void EndBuff()
    {
        Debug.LogWarning("혼란 디버프 끝");
        
        base.EndBuff();
    }
}
