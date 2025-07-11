using Assets.RobustFSM.Demo.Scripts.States;
using RobustFSM.Base;
using UnityEngine;

namespace Assets.SimpleFSM.Demo.Scripts.States.Idle
{
    public class SleepSubState : BCharacterMonoState
    {
        float _sleepTime;

        public override void Initialize()
        {
            base.Initialize();

            //set a specific name for this state
            StateName = "Sleep";
        }

        public override void OnEnter()
        {
            base.OnEnter();

            //set the sleep time
            _sleepTime = Random.Range(3f, 5f);
        }

        public void Update()
        {
            // decrement time
            _sleepTime -= Time.deltaTime;

            //if time is exhausted go to choose patrol point state
            if (_sleepTime <= 0)
                Machine.ChangeState<ChoosePatrolPoint>();
        }
    }
}
