using System.Collections;
using UnityEngine;

public class SelfRepairCartridge : Cartridge
{
    // _data.GimmickValues[0] : 회복주기
    // _data.GimmickValues[1] : 회복량
    
    private Coroutine _healCoroutine;

    public override bool ExcuteGimmick(Player owner)
    {
        if(!base.ExcuteGimmick(owner))
        {
            return false;
        }

        if (_healCoroutine != null) return true;

        _healCoroutine = StartCoroutine(HealRoutine(owner));
        return true;
    }

    private void OnDestroy()
    {
        if (_healCoroutine != null)
        {
            StopCoroutine(_healCoroutine);
            _healCoroutine = null;
        }
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
