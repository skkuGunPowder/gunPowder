using RobustFSM.Base;
using UnityEngine;

public class PlayerWalkState : MonoState
{
     private PlayerFSM _playerFSM;
    private Player _owner;
    public override void OnEnter()
    {
        Debug.Log($"Enter {this.GetType().Name} State");

        // 캐스팅
        _playerFSM = SuperMachine as PlayerFSM;
        _owner = _playerFSM.Owner;

        // 애니메이션 재생
        // _owner.MyAnimator.SetTrigger("Walk");
    }
    public override void OnExit()
    {
        Debug.Log($"Exit {this.GetType().Name} State");
    }

    /// <summary>
    /// 실제 행동 로직
    /// </summary>
    private void Update()
    {
        // 이동
    
    }
}
