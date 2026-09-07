using UnityEngine.Pool;

namespace PFE.Core.Scripts.StateMachines
{
    public class StateMachine
    {
        public readonly State root;
        public readonly TransitionSequencer sequencer;
        
        private bool started;

        public StateMachine(State root)
        {
            this.root = root;
            sequencer = new TransitionSequencer(this);
        }
        
        public void StartMachine()
        {
            if(started)
                return;
            
            started = true;
            root.Enter();
        }
        
        public static void ChangeState(State from, State to)
        {
            if(from == to || from == null || to == null)
                return;
            
            var lca = Lca(from, to);
            
            for(State state = from; state != lca; state = state.parent)
                state.Exit();

            using (ListPool<State>.Get(out var list))
            {
                for(State state = to; state != lca; state = state.parent)
                    list.Add(state);

                list.Reverse();

                foreach (var state in list)
                    state.Enter();
            }
        }
        
        //Lowest Common Ancestor
        public static State Lca(State a, State b)
        {
            using (HashSetPool<State>.Get(out var hash))
            {
                for(State s = a; s != null; s = s = s.parent)
                    hash.Add(s);
                
                for(State s = b; s != null; s = s.parent)
                    if(hash.Contains(s))
                        return s;
                
                return null;
            }
        }
    }
}