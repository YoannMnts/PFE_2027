namespace PFE.Core.Scripts.StateMachines
{
    public class TransitionSequencer
    {
        public readonly StateMachine stateMachine;
        
        
        public TransitionSequencer(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        private void BeginTransition(State from, State to)
        {
            
        }

        private void EndTransition()
        {
            
        }

        public void RequestTransition(State from, State to)
        {
            StateMachine.ChangeState(from, to);
        }
    }
}