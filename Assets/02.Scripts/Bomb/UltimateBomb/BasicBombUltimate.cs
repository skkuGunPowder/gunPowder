using UnityEngine;

public class BasicBombUltimate : Ultimate
{
    private const string OWNER_ID = "BO0001";
    private const float DURATION = 8f;
    private const float REDUCTION_PERCENT = 80f;

    public override void Init()
    {
        _ownerBombID = OWNER_ID;
        _bombStat = ItemDatabase.Instance.GetStat<BombStat>(OWNER_ID);
    }

    public override void ExcuteUltimate()
    {
        if (_owner == null)
        {
            Debug.LogError("[BasicBombUltimate] Owner가 없습니다.");
            return;
        }
        _owner.ApplyBasicBombCooldownBuff(REDUCTION_PERCENT, OperationType.Multiplicative, DURATION);
    }
}
