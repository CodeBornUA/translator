using System;
using Serilog;

namespace Parser
{
    public abstract class ExitOperation : IOperation
    {
        protected readonly StackStateMachine Machine;

        public ExitOperation(StackStateMachine machine)
        {
            Machine = machine;
        }

        public abstract void Do();
    }

    public class ErrorExitOperation : ExitOperation
    {
        private readonly ILogger _logger;
        private readonly string _message;

        public ErrorExitOperation(ILogger logger, string message, StackStateMachine machine) : base(machine)
        {
            _logger = logger;
            _message = message;
        }

        public override void Do()
        {
            _logger.Error(_message + ", but found {0} at line {1}", Machine.Current, Machine.Current.Line);
            throw new Exception(_message);
        }
    }

    public class StackExitOperation : ExitOperation
    {
        private readonly bool _fireAgain;

        public StackExitOperation(StackStateMachine machine, bool fireAgain = false) : base(machine)
        {
            _fireAgain = fireAgain;
        }

        public override void Do()
        {
            var nextState = Machine.StateStack.Pop();
            Machine.State = nextState;

            if (_fireAgain)
                Machine.Fire(Machine.Current);
        }
    }

    public class TransitionExitOperation : ExitOperation
    {
        private readonly bool _fireAgain;
        private readonly int _newState;

        public TransitionExitOperation(StackStateMachine machine, int newState, bool fireAgain = false) : base(machine)
        {
            _newState = newState;
            _fireAgain = fireAgain;
        }

        public override void Do()
        {
            Machine.State = _newState;

            if (_fireAgain)
                Machine.Fire(Machine.Current);
        }
    }
}