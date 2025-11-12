using UnityEngine;
using DG.Tweening;

public class PlayerLastDieState : PlayerBaseState
{
    
    public float VibrateDuration = 1f;
    public float IntervalTime = 1.5f;
    public float VibratePower = 0.65f;
    public int Vibrato = 110;
    private bool _effectInitial = false;
    public override void OnEnter()
    {
        Debug.Log("LastDieState Enter");
        base.OnEnter();
        _owner.RPC_SetAnimatorTrigger("HitLoop");
        EventManager.Instance.OnGameSet += LastDiePlay;
        SetImmuneState();
        EventManager.Instance.LastAttack(_owner.PhotonView.OwnerActorNr);

    }

    public override void OnExit()
    {
        base.OnExit();
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameSet -= LastDiePlay;
        }
    }
    
    public override void MineUpdate()
    {
        
    }

    private void LastDiePlay()
    {
        EventManager.Instance.OnGameSet -= LastDiePlay;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(IntervalTime);
        seq.Append(_owner.transform.DOShakePosition(VibrateDuration, VibratePower, Vibrato,90f, false,true,ShakeRandomnessMode.Harmonic));
        seq.AppendCallback(ExecuteDeath);
    }
    
    private void ExecuteDeath()
    {
        ExecuteDeathEffects();
        GameManager.Instance.RequestGameOver();
        SyncStateChange<PlayerObserveState>();
    }
    private void SetImmuneState()
    {
        _owner.gameObject.tag = "Immune";
        _owner.PlayerStat.IsImmune = true;
    }
}
