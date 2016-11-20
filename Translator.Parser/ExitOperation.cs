using System;
using System.Collections.Generic;
using Serilog;
using Stateless;
using Translator.Lexer;

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
            _logger.Error(_message, Machine.Current);
            throw new Exception(_message);
        }
    }

    public class StackExitOperation : ExitOperation
    {
        private bool _fireAgain;

        public StackExitOperation(StackStateMachine machine, bool fireAgain = false) : base(machine)
        {
            _fireAgain = fireAgain;
        }

        public override void Do()
        {
            var nextState = Machine.StateStack.Pop();
            Machine.State = nextState;

            if (_fireAgain)
            {
                Machine.Fire(Machine.Current);
            }
        }
    }
}