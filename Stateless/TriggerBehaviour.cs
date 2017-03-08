using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            private readonly Func<bool> _guard;

            protected TriggerBehaviour(TTrigger trigger, Func<bool> guard)
            {
                Trigger = trigger;
                _guard = guard;
            }

            public TTrigger Trigger { get; }

            public bool IsGuardConditionMet
            {
                get { return _guard(); }
            }

            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}