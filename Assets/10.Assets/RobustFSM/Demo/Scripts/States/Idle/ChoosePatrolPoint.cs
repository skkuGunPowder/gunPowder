using Assets.RobustFSM.Demo.Scripts.States;
using Assets.SimpleFSM.Demo.Scripts.States.Patrol;
using UnityEngine;

namespace Assets.SimpleFSM.Demo.Scripts.States.Idle
{
    public class ChoosePatrolPoint : BCharacterMonoState
    {
        private float _thinkTime;
        private Material _initMaterial;

        public override void OnEnter()
        {
            base.OnEnter();

            // set some stuff
            _thinkTime = Random.Range(1f, 5f);
            _initMaterial = Owner.MeshRenderer.material;

            // invoke change material
            InvokeRepeating("ChangeToRandomMaterial", 0.5f, 0.1f);
        }

        public void Update()
        {
            _thinkTime -= Time.deltaTime;
            if(_thinkTime < 0f)
            {
                //find a random patrol point
                Transform temp = Owner.PatrolPoints[Random.Range(0, Owner.PatrolPoints.Count)];

                //check if point is valid
                do
                {
                    temp = Owner.PatrolPoints[Random.Range(0, Owner.PatrolPoints.Count)];
                }
                while (temp == Owner.Target);

                //update target and go to patrol
                Owner.Target = temp;
                SuperMachine.ChangeState<PatrolMainState>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            // reset material
            Owner.MeshRenderer.material = _initMaterial;
            CancelInvoke("ChangeToRandomMaterial");
        }

        public void ChangeToRandomMaterial()
        {
            Material choosenMaterial = Owner.Materials[Random.Range(0, Owner.Materials.Count)];
            Owner.MeshRenderer.material = choosenMaterial;
        }
    }
}
