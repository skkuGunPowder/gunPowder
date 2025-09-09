using UnityEngine;
using DG.Tweening;

public class PlayerLastDieState : PlayerBaseState
{
    
    public float VibrateDuration = 1f;
    public float IntervalTime = 1.5f;
    public float VibratePower = 1f;
    private bool _effectInitial = false;
    public override void OnEnter()
    {
        base.OnEnter();
        EventManager.Instance.LastAttack(this._owner.PhotonView.OwnerActorNr);
        LastDiePlay();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
    
    public override void MineUpdate()
    {
        
    }

    private void LastDiePlay()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(IntervalTime);
        seq.Append(this.gameObject.transform.DOShakePosition(VibrateDuration, VibratePower));
        seq.AppendCallback(ExecuteDeath);
    }
    
    private void ExecuteDeath()
    {
        ExecuteDeathEffects();
        GameManager.Instance.RequestGameOver();
    }
}
