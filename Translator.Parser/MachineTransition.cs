using System;
using System.Collections.Generic;
using Translator.Lexer;

namespace Parser
{
    public class MachineTransition
    {
        public Func<Token, bool> EnterPredicate { get; set; }

        public int? NewState { get; set; }

        public StackOperation StackOperation { get; set; }

        public ExitOperation ExitOperation { get; set; }
    }
}