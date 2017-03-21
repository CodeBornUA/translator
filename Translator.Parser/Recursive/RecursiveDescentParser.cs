using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Translator.LexerAnalyzer.Tokens;

namespace Parser
{
    public class RecursiveDescentParser : IParser
    {
        private readonly IObserver<LogEvent> _logObserver;

        public RecursiveDescentParser(IObserver<LogEvent> logObserver)
        {
            _logObserver = logObserver;

            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Observers(ConfigureObservers)
                .CreateLogger();
        }

        public Logger Logger { get; set; }

        public bool CheckSyntax(IEnumerable<Token> tokens)
        {
            return Root(tokens.GetEnumerator());
        }

        private void ConfigureObservers(IObservable<LogEvent> observable)
        {
            if (_logObserver != null)
                observable.Subscribe(_logObserver);
        }

        private bool Root(IEnumerator<Token> tokens)
        {
            Logger.Information("Root parsing");
            TokensSequence.Logger = Logger;
            return TokensSequence.Init(ref tokens)
                .String("program")
                .Id()
                .NewLine()
                .String("var")
                .Check(DefList)
                .NewLine()
                .String("begin")
                .NewLine()
                .Check(OperatorList)
                .NewLine()
                .String("end")
                .Result;
        }

        private bool OperatorList(ref IEnumerator<Token> arg)
        {
            Logger.Information("List of operators");
            var s = TokensSequence.Init(ref arg)
                .Check(Operator)
                .Iterative(seq => seq.NewLine(), seq => seq.Check(Operator), true);

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in list of operators on '{arg.Current}'");
            arg = s.ToCompare;
            return s.Result;
        }

        private bool Operator(ref IEnumerator<Token> arg)
        {
            Logger.Information("Operator");
            return TokensSequence.AnyOf(ref arg, true,
                seq => seq
                    .Label()
                    .String(":")
                    .NewLine()
                    .Check(UnlabeledOperator),
                seq => seq
                    .Check(UnlabeledOperator));
        }

        private bool UnlabeledOperator(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Unlabeled operator");
            return TokensSequence.AnyOf(ref enumerator, true,
                seq => seq
                    .Id()
                    .String("=")
                    .Check(Expression),
                seq => seq
                    .String("readl")
                    .String("(")
                    .Check(IdList)
                    .String(")"),
                seq => seq
                    .String("writel")
                    .String("(")
                    .Check(IdList)
                    .String(")"),
                seq => seq
                    .String("do")
                    .Id()
                    .String("=")
                    .Check(Expression)
                    .String("to")
                    .Check(Expression)
                    .NewLine()
                    .Check(OperatorList)
                    .NewLine()
                    .String("next"),
                seq => seq
                    .String("if")
                    .Check(LogicalExpression)
                    .String("then")
                    .String("goto")
                    .Label());
        }

        private bool LogicalExpression(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Logical expression");
            var s = TokensSequence.Init(ref enumerator)
                .Check(LogicalTerm)
                .Iterative(seq => seq.String("or"), seq => seq.Check(LogicalTerm));

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in logical expression on '{enumerator.Current}'");
            enumerator = s.ToCompare;
            return s.Result;
        }

        private bool LogicalTerm(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Logical term");
            var s = TokensSequence.Init(ref enumerator)
                .Check(LogicalMultiplier)
                .Iterative(seq => seq.String("and"), seq => seq.Check(LogicalMultiplier));

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in logical term on '{enumerator.Current}'");
            enumerator = s.ToCompare;
            return s.Result;
        }

        private bool LogicalMultiplier(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Logical multiplier");
            return TokensSequence.AnyOf(ref enumerator, true,
                seq => seq.Check(Expression).AnyFrom(
                    innseq => innseq.String(">").Check(Expression),
                    innseq => innseq.String(">=").Check(Expression),
                    innseq => innseq.String("<").Check(Expression),
                    innseq => innseq.String("<=").Check(Expression),
                    innseq => innseq.String("==").Check(Expression),
                    innseq => innseq.String("!=").Check(Expression)),
                seq => seq.String("[")
                    .Check(LogicalExpression)
                    .String("]"),
                seq => seq.String("not").Check(LogicalExpression));
        }

        private bool Expression(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Expression");
            var s = TokensSequence.Init(ref enumerator)
                .Check(Term)
                .Iterative(seq => seq.String("+"), seq => seq.Check(Term))
                .Iterative(seq => seq.String("-"), seq => seq.Check(Term));

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in expression on '{enumerator.Current}'");
            enumerator = s.ToCompare;
            return s.Result;
        }

        private bool Term(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Term");
            var s = TokensSequence.Init(ref enumerator)
                .Check(Multiplier)
                .Iterative(seq => seq.String("/"), seq => seq.Check(Multiplier))
                .Iterative(seq => seq.String("*"), seq => seq.Check(Multiplier));

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in term on '{enumerator.Current}'");
            enumerator = s.ToCompare;
            return s.Result;
        }

        private bool Multiplier(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Multiplier");
            return TokensSequence.AnyOf(ref enumerator, true, seq => seq.Id(),
                seq => seq.Const(),
                seq => seq.String("(").Check(Expression).String(")"));
        }

        private bool IdList(ref IEnumerator<Token> enumerator)
        {
            Logger.Information("Id list");
            var s = TokensSequence.Init(ref enumerator)
                .Id()
                .Iterative(seq => seq.String(","), seq => seq.Id());

            if (!s.Result)
                TokensSequence.Logger.Error($"Error in id list on '{enumerator.Current}'");
            enumerator = s.ToCompare;
            return s.Result;
        }

        private bool DefList(ref IEnumerator<Token> arg)
        {
            Logger.Information("Definitions list");
            var seq = TokensSequence.Init(ref arg)
                .Check(Def)
                .Iterative(subseq => subseq.String(","), subseq => subseq.Check(Def)
                );

            if (!seq.Result)
                TokensSequence.Logger.Error($"Error in definition list on '{arg.Current}'");
            arg = seq.ToCompare;
            return seq.Result;
        }

        private bool Def(ref IEnumerator<Token> arg)
        {
            Logger.Information("Definition");
            return TokensSequence.Init(ref arg)
                .String("float")
                .Id()
                .Result;
        }
    }
}