using System.Collections.Generic;
using System.Linq;
using Translator.Lexer;

namespace Parser
{
    public class StackStateMachine
    {
        private readonly List<StateTransition> _transitions;

        public StackStateMachine(int state, List<StateTransition> transitions)
        {
            _transitions = transitions;
            State = state;
        }

        public Stack<int> StateStack { get; set; } = new Stack<int>();
        public int State { get; set; }

        public Token Current { get; set; }

        public void Fire(Token symbol)
        {
            Current = symbol;

            var transition = _transitions.First(x => x.PreviousState == State);
            var possible = transition.Transitions.FirstOrDefault(x => x.EnterPredicate(symbol));
            if (possible == null)
            {
                transition.OnUnequality?.Do();
                return;
            }

            if (possible.NewState != null)
            {
                State = possible.NewState.Value;
            }
            possible.StackOperation?.Do();
            possible.ExitOperation?.Do();
        }
    }
}