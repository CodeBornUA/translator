using System;

namespace Parser
{
    internal class SubMachineExitOperation : ExitOperation
    {
        private int _subState;
        private int _exitState;
        private StackStateMachine _machine;
        private bool _fireAgain;

        public SubMachineExitOperation(int subState, int exitState, StackStateMachine machine, bool fireAgain = false) : base(machine)
        {
            _subState = subState;
            _exitState = exitState;
            _machine = machine;
            _fireAgain = fireAgain;
        }

        public override void Do()
        {
            _machine.StateStack.Push(_exitState);
            _machine.State = _subState;

            if (_fireAgain)
            {
                _machine.Fire(_machine.Current);
            }
        }
    }
}