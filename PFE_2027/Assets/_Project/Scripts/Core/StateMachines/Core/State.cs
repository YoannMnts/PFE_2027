using System.Collections.Generic;

namespace PFE.Core.Scripts.StateMachines
{
    public abstract class State
    {
        public State ActiveChild { get; private set; }
        
        public readonly StateMachine stateMachine;
        public readonly State parent;

        protected State(StateMachine stateMachine, State parent = null)
        {
            this.stateMachine = stateMachine;
            this.parent = parent;
        }

        protected virtual State GetInitialState() => null;
        protected virtual State GetTransition() => null;

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }
        
        internal void Enter()
        {
            if(parent != null)
                parent.ActiveChild = this;
            
            OnEnter();
            
            var initialState = GetInitialState();
            initialState?.Enter();
        }

        internal void Exit()
        {
            ActiveChild?.Exit();
            ActiveChild = null;
            OnExit();
        }

        public State Leaf()
        {
            var currentState = this;
            while(currentState.ActiveChild != null) 
                currentState = currentState.ActiveChild;
            
            return currentState;
        }

        public IEnumerable<State> PathToRoot()
        {
            for(State state = this; state != null; state = state.parent)
                yield return state;
        }
    }
}