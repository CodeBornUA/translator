using System.Collections.Generic;

namespace Parser.StateMachine
{
    public class StateTransition
    {
        public int PreviousState { get; set; }
        public List<MachineTransition> Transitions { get; set; }

        public ExitOperation OnEquality { get; set; }
        public ExitOperation OnUnequality { get; set; }
    }
}