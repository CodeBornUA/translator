using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Events;
using Translator.Lexer;
using Translator.LexerAnalyzer.Tokens;

namespace Parser
{
    public class StateMachineParser : IParser
    {
        private readonly ILogger _logger;
        private List<StateTransition> _transitions = new List<StateTransition>();
        private StackStateMachine _machine;
        private int operatorFirstState;
        private IObserver<LogEvent> _logObserver;
        private int expFirstState;

        public StateMachineParser(ILogger logger)
        {
            _logger = logger;
            _machine = new StackStateMachine(1, _transitions);
        }

        public StateMachineParser(IObserver<LogEvent> logObserver)
        {
            _machine = new StackStateMachine(1, _transitions);

            _logObserver = logObserver;
            _logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Observers(ConfigureObservers)
                .CreateLogger();
        }

        private void ConfigureObservers(IObservable<LogEvent> observable)
        {
            if (_logObserver != null)
            {
                observable.Subscribe(_logObserver);
            }
        }

        public bool CheckSyntax(IEnumerable<Token> tokens)
        {
            _machine.State = 1;
            _machine.StateStack.Clear();
            FillTransitionsTable();

            var stream = tokens.GetEnumerator();
            while (stream.MoveNext())
            {
                _machine.Fire(stream.Current);
            }

            return !_machine.StateStack.Any();
        }

        private void FillTransitionsTable()
        {
            FillProgram();
        }

        private void FillProgram()
        {
            _transitions.Add(StandartTransition(1,
                "Program must start with program keyword",
                (x => x.Substring == "program", 2)));
            _transitions.Add(StandartTransition(2,
                "There should be an identifier after program keyword ",
                (x => x is Identifier, 3)));
            _transitions.Add(StandartTransition(3,
                "Program and its name should be proceeded by the line feed",
                (x => x.Substring == "\r\n", 4)));
            _transitions.Add(StandartTransition(4,
                "Expected var here",
                (x => x.Substring == "var", 5)));
            _transitions.Add(StandartTransition(5,
                "Only float type is supported now",
                (x => x.Substring == "float", 6)));
            _transitions.Add(StandartTransition(6,
                "Identifier is expected after type of variable",
                (x => x is Identifier, 7)));
            _transitions.Add(StandartTransition(7,
                "Variable definitions should be split with comma"
                , (x => x.Substring == ",", 5)));
            _transitions.Add(StandartTransition(7,
                "End of line expected here"
                , (x => x.Substring == "\r\n", 8)));

            operatorFirstState = FillOperator();

            _transitions.Add(StandartTransition(8,
                "'begin' keyword is expected here", (x => x.Substring == "begin", 9)));
            _transitions.Add(StandartTransition(9, "New line is expected here", (x => x.Substring == "\r\n", 10)));

            _transitions.Add(new StateTransition()
            {
                PreviousState = 10,
                OnUnequality = new SubMachineExitOperation(101, 9, _machine, true),
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == "end"
                    }
                }
            });
        }

        private int FillOperator()
        {
            var intialState = 101;

            _transitions.Add(new StateTransition()
            {
                PreviousState = intialState,
                OnUnequality = new ErrorExitOperation(_logger, "Operator is expected here", _machine),
                Transitions = new List<MachineTransition>()
                {
                     new MachineTransition(){EnterPredicate = x => x is Identifier,NewState = 102},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "readl",NewState = 104},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "writel",NewState = 104},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "do",NewState = 107},
                     new MachineTransition(){EnterPredicate = x => x is LabelToken,NewState = 113},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "if",NewState = 114}
                }
            });

            _transitions.Add(StandartTransition(113, "New line is expected here", (x => x.Substring == "\r\n", 118)));

            expFirstState = FillExpression();
            _transitions.Add(SubMachineTransition(102, expFirstState, x => x.Substring == "=", 103, "'=' is expected here"));
            _transitions.Add(ExitOnUnequality(103));
            _transitions.Add(StandartTransition(104, "'(' expected here", (x => x.Substring == "(", 105)));
            _transitions.Add(StandartTransition(105, "Identifier is expected here", (x => x is Identifier, 106)));
            _transitions.Add(StandartTransition(106, "Comma is expected here", (x => x.Substring == ",", 105)));
            _transitions.Add(StandartTransition(106, "')' expected here", (x => x.Substring == ")", null)));
            _transitions.Add(StandartTransition(107, "Identifier expected here", (x => x is Identifier, 108)));
            _transitions.Add(SubMachineTransition(108, expFirstState, x => x.Substring == "=", 109, "'=' is not found"));
            _transitions.Add(SubMachineTransition(109, expFirstState, x => x.Substring == "to", 110, "'to' is not found"));
            _transitions.Add(SubMachineTransition(110, 101, x => x.Substring == "\r\n", 111, "New line is expected here"));
            _transitions.Add(StandartTransition(111, "New line is expected here", (x => x.Substring == "\r\n", 112)));

            _transitions.Add(new StateTransition()
            {
                PreviousState = 112,
                OnUnequality = new SubMachineExitOperation(101, 111, _machine, true),
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == "next",
                        ExitOperation = new StackExitOperation(_machine)
                    }
                }
            });

            var logExpFirstState = FillLogExpression();
            _transitions.Add(new StateTransition()
            {
                PreviousState = 114,
                OnUnequality = new SubMachineExitOperation(logExpFirstState, 115, _machine, true),
                Transitions = new List<MachineTransition>()
            });

            _transitions.Add(StandartTransition(115, "'then' expected here", (x => x.Substring == "then", 116)));
            _transitions.Add(StandartTransition(116, "'goto' expected here", (x => x.Substring == "goto", 117)));
            _transitions.Add(StandartTransition(117, "Label usage is expected here", (x => x is LabelToken, null)));

            _transitions.Add(new StateTransition()
            {
                PreviousState = 118,
                OnUnequality = new StackExitOperation(_machine),
                Transitions = new List<MachineTransition>()
                {
                     new MachineTransition(){EnterPredicate = x => x is Identifier,NewState = 102},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "readl",NewState = 104},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "writel",NewState = 104},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "do",NewState = 107},
                     new MachineTransition(){EnterPredicate = x => x.Substring == "if",NewState = 114}
                     //Everything but label
                }
            });

            return intialState;
        }

        private int FillLogExpression()
        {
            var initial = 301;

            _transitions.Add(new StateTransition()
            {
                PreviousState = 301,
                OnUnequality = new SubMachineExitOperation(expFirstState, 302, _machine, true),
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == "not",
                        NewState = 301
                    },
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == "[",
                        NewState = initial,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 304)
                    }
                }
            });

            _transitions.Add(new StateTransition()
            {
                OnUnequality = new ErrorExitOperation(_logger, "Relation operator is expected", _machine),
                PreviousState = 302,
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == ">",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    },
                    new MachineTransition(){
                        EnterPredicate = x => x.Substring == ">=",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    },
                    new MachineTransition(){
                        EnterPredicate = x => x.Substring == "<",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    },
                    new MachineTransition(){
                        EnterPredicate = x => x.Substring == "<=",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    },
                    new MachineTransition(){
                        EnterPredicate = x => x.Substring == "==",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    },
                    new MachineTransition(){
                        EnterPredicate = x => x.Substring == "!=",
                        NewState = expFirstState,
                        StackOperation = new WriteStackOperation(_machine.StateStack, 303)
                    }
                }
            });

            _transitions.Add(new StateTransition()
            {
                OnUnequality = new StackExitOperation(_machine, true),
                PreviousState = 303,
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition(){EnterPredicate = x => x.Substring == "and", NewState = 301},
                    new MachineTransition(){EnterPredicate = x => x.Substring == "or", NewState = 301}
                }
            });

            _transitions.Add(StandartTransition(304, "']' is expected here", (x => x.Substring == "]", 303)));

            return initial;
        }

        private int FillExpression()
        {
            var initial = 200;

            _transitions.Add(new StateTransition()
            {
                PreviousState = 200,
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => x.Substring == "-",
                        NewState = 201
                    }
                },
                OnUnequality = new TransitionExitOperation(_machine, 201, true)
            });

            _transitions.Add(StandartTransition(201, "Constant of identifier is expected here", (x => x is Identifier, 202), (x => x is Constant<float>, 202)));
            _transitions.Add(SubMachineTransition(201, initial, x => x.Substring == "(", 203));

            _transitions.Add(new StateTransition()
            {
                OnUnequality = new StackExitOperation(_machine, true),
                PreviousState = 202,
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition(){EnterPredicate = x => x.Substring == "+", NewState = 201},
                    new MachineTransition(){EnterPredicate = x => x.Substring == "-", NewState = 201},
                    new MachineTransition(){EnterPredicate = x => x.Substring == "*", NewState = 201},
                    new MachineTransition(){EnterPredicate = x => x.Substring == "/", NewState = 201}
                }
            });

            _transitions.Add(StandartTransition(203, null, (x => x.Substring == ")", 202)));

            return initial;
        }

        private StateTransition SubMachineTransition(int prevState, int subState, Func<Token, bool> pred, int? nextState = null, string errorMessage = null)
        {
            var prevT = _transitions.FirstOrDefault(x => x.PreviousState == prevState);
            var t = prevT ?? new StateTransition()
            {
                PreviousState = prevState,
                Transitions = new List<MachineTransition>(),
                OnUnequality = new ErrorExitOperation(_logger, errorMessage ?? "Error", _machine)
            };
            if (prevT != null)
            {
                _transitions.Remove(prevT);
            }

            var mt = new MachineTransition()
            {
                EnterPredicate = pred,
                NewState = subState,
                StackOperation = nextState != null ? new WriteStackOperation(_machine.StateStack, nextState.Value) : null,
            };

            if (nextState == null && t.OnEquality == null)
            {
                t.OnEquality = new StackExitOperation(_machine);
            }

            t.Transitions.Add(mt);
            return t;
        }

        private StateTransition ExitOnUnequality(int prevState)
        {
            var t = new StateTransition()
            {
                PreviousState = prevState,
                Transitions = new List<MachineTransition>()
                {
                    new MachineTransition()
                    {
                        EnterPredicate = x => false
                    }
                },
                OnUnequality = new StackExitOperation(_machine, true)
            };
            return t;
        }

        private StateTransition StandartTransition(int prevState, string errorMessage = null, params ValueTuple<Func<Token, bool>, int?>[] func)
        {
            var prevT = _transitions.FirstOrDefault(x => x.PreviousState == prevState);
            var t = prevT ?? new StateTransition()
            {
                PreviousState = prevState,
                Transitions = new List<MachineTransition>(),
                OnUnequality = new ErrorExitOperation(_logger, errorMessage ?? "Error", _machine)
            };
            if (prevT != null)
            {
                _transitions.Remove(prevT);
            }

            Array.ForEach(func, x => t.Transitions.Add(new MachineTransition()
            {
                EnterPredicate = x.Item1,
                NewState = x.Item2,
                ExitOperation = x.Item2 == null ? new StackExitOperation(_machine) : null
            }));

            return t;
        }
    }

    internal enum ProgramState
    {
        Initial,
        Begin,
        OneOperator
    }
}
