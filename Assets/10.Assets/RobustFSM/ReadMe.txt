Welcome to RobustFSM

RobustFSM is a package designed to ease the creation
of finite state machines for your project. RobustFSM is
ideal for simple finite state machines. Besides having the
ability to create finite state machines in the grasp of
hands, you can also create hierichal state machines from
RobustFSM
RobustFSM comes with a demo scene to show how you can implement this 
package.

How to install
--------------
Simply the import the package into your project. I'm sure you have
done this already. You can move the RobustFSM folder into any
location of your convienience

How to use
-----------
To create a finite state machine, simply inherit from the finite
state machine base class as shown "MonoFSM" e.g

'public class CustomFSM : MonoFSM<TypeOfOwnerOfThisState>'

The passed type allows the state machine to automatically retrive
the component from the gameobject and be accessible inside the state machine
itself and the states

The MonoFSM inherits from the MonoBehaviour class. You will need to
implement the AddStates method of the BFSM. Look at the example 
in the demo scene

To create a state, inherit from the base state class(MonoState) e.g

'public class CustomState : MonoState'

If you to create a sub state machine, instead of inheriting from
the MonoState class inherit form the base hybrid state machine class
(MonoHState). This allows your state to act both like a state and a state 
machine e.g

'public class CustomHeirichalState : MonoHState'

To access the super state machine inside a state
use this logic

SuperMachine.

To access the owner of a state
use this logic. Assume we have a gameobject with a Character and CharacterFSM script
attached to it

public Character Owner => get ((CharacterFSM)SuperMachine).Owner;

Check the demo scene of more examples to access the owner finite state machine

To change state simply use this logic

'machine.ChangeState<NextState>();'

This applies for both the basic state and the hybrid state. If you are in a
BHState and you want to trigger a state transition of that state use

'ChangeState<NextState>();

Thus

'machine.ChangeState<Next>()' - will trigger the FSM owning this state to go to the next state
'ChangeState<NextState>()' - works if the calling state inherits from the BHState, will trigger a state
transition to the net state in that sub state machine.

Thank you for using RobustFSM. You are free to add features to this package and if you do
please share them so that I also share with the rest of the world.

Here is some of my assets
Soccer AI - https://assetstore.unity.com/packages/templates/soccer-ai-63743

Contact
Developer: Andrew Blessing Manyore
Company: Wasu Studio
Email: contact@wasustudio.com		
Mobile: +263786123725