﻿using System.Collections.Generic;

namespace Parser
{
    public abstract class StackOperation : IOperation
    {
        protected Stack<int> StateStack { get; set; }

        protected StackOperation(Stack<int> stateStack)
        {
            StateStack = stateStack;
        }

        public abstract void Do();
    }

    public class WriteStackOperation : StackOperation
    {
        private readonly int _stateToWrite;

        public WriteStackOperation(Stack<int> stateStack, int stateToWrite) : base(stateStack)
        {
            _stateToWrite = stateToWrite;
        }

        public override void Do()
        {
            StateStack.Push(_stateToWrite);
        }
    }
}