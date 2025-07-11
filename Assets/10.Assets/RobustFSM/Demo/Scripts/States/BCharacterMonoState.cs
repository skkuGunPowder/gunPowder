using Assets.SimpleFSM.Demo.Scripts;
using RobustFSM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RobustFSM.Demo.Scripts.States
{
    public class BCharacterMonoState : MonoState
    {
        /// <summary>
        /// A little extra stuff. Accessing info inside the OwnerFSM
        /// </summary>
        public Character Owner
        {
            get
            {
                return ((CharacterFSM)SuperMachine).Owner;
            }
        }
    }
}
