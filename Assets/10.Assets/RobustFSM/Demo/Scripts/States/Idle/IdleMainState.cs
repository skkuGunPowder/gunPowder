using RobustFSM.Base;
using UnityEngine;

namespace Assets.SimpleFSM.Demo.Scripts.States.Idle
{
    public class IdleMainState : MonoHState
    {
        public override void AddStates()
        {
            AddState<ChoosePatrolPoint>();
            AddState<SleepSubState>();

            SetInitialState<SleepSubState>();
        }
    }
}
