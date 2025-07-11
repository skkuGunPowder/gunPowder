using Assets.RobustFSM.Demo.Scripts.States;
using Assets.SimpleFSM.Demo.Scripts.States.Idle;
using UnityEngine;

namespace Assets.SimpleFSM.Demo.Scripts.States.Patrol
{
    public class PatrolMainState : BCharacterMonoState
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public void Update()
        {
            //find direction to target
            Vector3 targetDir = Owner.Target.position - Owner.transform.position;
            targetDir.y = 0f;

            //find target rot
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            //look at target
            Owner.transform.rotation = Quaternion.Lerp(Owner.transform.rotation, 
                targetRot, 
                5 * Owner.Speed * Time.deltaTime);

            //move towards target
            Owner.transform.position = Vector3.MoveTowards(Owner.transform.position, 
                Owner.Target.position, 
                Owner.Speed * Time.deltaTime);

            //go to idle state if we have reached our target
            if (Vector3.Distance(Owner.transform.position, Owner.Target.position) <= 0.1f)
                SuperMachine.ChangeState<IdleMainState>();
        }

    }
}
