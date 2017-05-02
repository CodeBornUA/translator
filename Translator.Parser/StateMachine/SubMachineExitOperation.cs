namespace Parser.StateMachine
{
    internal class SubMachineExitOperation : ExitOperation
    {
        private readonly int _exitState;
        private readonly bool _fireAgain;
        private readonly StackStateMachine _machine;
        private readonly int _subState;

        public SubMachineExitOperation(int subState, int exitState, StackStateMachine machine, bool fireAgain = false)
            : base(machine)
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
                _machine.Fire(_machine.Current);
        }
    }
}