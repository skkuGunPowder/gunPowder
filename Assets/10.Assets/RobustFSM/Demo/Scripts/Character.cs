using System.Collections.Generic;
using UnityEngine;

namespace Assets.SimpleFSM.Demo.Scripts
{
    [RequireComponent(typeof(CharacterFSM))]
    public  class Character : MonoBehaviour
    {
        [SerializeField]
        private int _speed = 3;
        public int Speed => _speed;

        [SerializeField]
        private List<Material> _materials;
        public List<Material> Materials => _materials;

        [SerializeField]
        private List<Transform> _patrolPoints;
        public List<Transform> PatrolPoints => _patrolPoints;

        [SerializeField]
        private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer => _meshRenderer;

        public CharacterFSM FSM { get; set; }
        public Transform Target { get; set; }

        private void Start()
        {
            FSM = GetComponent<CharacterFSM>();
        }
    }
}
