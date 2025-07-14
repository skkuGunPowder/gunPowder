using Assets.SimpleFSM.Demo.Scripts.States.Idle;
using Photon.Realtime;
using RobustFSM.Base;
using UnityEngine;

public class PlayerFSM : MonoFSM<Player>
{
    public override void AddStates()
    {
        // 상태 등록
        AddState<PlayerIdleState>();
        AddState<PlayerWalkState>();
        AddState<PlayerJumpState>();
        

        // 초기 상태 설정
        SetInitialState<PlayerIdleState>();
    }
} 