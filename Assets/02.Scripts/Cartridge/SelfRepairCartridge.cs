using System.Collections;
using UnityEngine;

public class SelfRepairCartridge : Cartridge
{
    // _data.GimmickValues[0] : 회복주기
    // _data.GimmickValues[1] : 회복량
    
    private Coroutine _healCoroutine;

    public override void ExcuteGimmick(Player owner)
    {
        base.ExcuteGimmick(owner);

        if (_healCoroutine != null) return;

        _healCoroutine = StartCoroutine(HealRoutine(owner));
    }

    private IEnumerator HealRoutine(Player owner)
    {
        while (true)
        {
            yield return new WaitForSeconds(_data.GimmickValues[0]);
            owner.PlayerStat.IncreaseHP(Mathf.RoundToInt(_data.GimmickValues[1]));
        }
    }
}
