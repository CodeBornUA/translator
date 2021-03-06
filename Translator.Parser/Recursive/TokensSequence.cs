﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog.Core;
using Serilog.Events;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Recursive
{
    public class TokensSequence : Collection<Token>
    {
        public delegate bool RefEnumeratorBoolFunc(ref IEnumerator<Token> enumerator);

        private IEnumerator<Token> _toCompare;

        private TokensSequence(IEnumerator<Token> toCompare)
        {
            _toCompare = toCompare;
        }

        public bool Result { get; private set; } = true;

        public IEnumerator<Token> ToCompare => _toCompare;

        public static Logger Logger { get; set; }

        public bool TryMode { get; set; }

        public static TokensSequence Init(ref IEnumerator<Token> toCompare)
        {
            return new TokensSequence(toCompare);
        }

        public TokensSequence String(string checkText)
        {
            Log(LogEventLevel.Information,
                $"Looking for the {(checkText == "\r\n" ? "new line" : $"string {checkText}")}");
            Evaluate(token => token is StringToken && (token as StringToken).Substring == checkText);
            return this;
        }

        private bool Evaluate(Func<Token, bool> func)
        {
            if (!Result)
                return false;

            var hasNext = _toCompare.MoveNext();
            if (!hasNext)
            {
                Log(TryMode ? LogEventLevel.Information : LogEventLevel.Error, "Sequence doesn't contain tokens anymore",
                    _toCompare.Current);
                Result = false;
                return false;
            }

            var current = _toCompare.Current;
            if (current == null)
            {
                Log(LogEventLevel.Verbose, "Token is null while evaluating");
                Result = false;
                return false;
            }

            var newSuccess = func(current);
            if (Result && !newSuccess)
                if (TryMode)
                    Log(LogEventLevel.Verbose, "Evaluate try failed: {0}", _toCompare.Current);
                else
                    Log(LogEventLevel.Error, "Evaluate failed: {0}", _toCompare.Current);

            Result = Result && newSuccess;
            return Result;
        }

        public TokensSequence Id(Func<IdentifierToken, bool> filterFunc = null)
        {
            Log(LogEventLevel.Verbose, "Looking for an identifier");
            Evaluate(token => token is IdentifierToken && (filterFunc?.Invoke((IdentifierToken) token) ?? true));
            return this;
        }

        public TokensSequence Label()
        {
            Log(LogEventLevel.Verbose, "Looking for a label");
            Evaluate(token => token is LabelToken);
            return this;
        }

        public TokensSequence NewLine()
        {
            return String("\r\n");
        }

        public TokensSequence Check(RefEnumeratorBoolFunc checkFunc)
        {
            Result = Result && checkFunc(ref _toCompare);

            return this;
        }

        public TokensSequence Iterative(Func<TokensSequence, TokensSequence> func,
            Func<TokensSequence, TokensSequence> restFunc, bool tryMode = true)
        {
            bool funcSuccess;
            do
            {
                var clone = _toCompare.Clone();
                var tokensSequence = Init(ref clone);
                tokensSequence.TryMode = tryMode;
                var seq = func(tokensSequence);
                funcSuccess = seq.Result;
                if (seq.Result)
                {
                    Log(LogEventLevel.Verbose, "First part of iterative found: {0}", seq.ToCompare.Current);
                    funcSuccess = funcSuccess && restFunc(seq).Result;
                    if (funcSuccess)
                    {
                        Log(LogEventLevel.Verbose, "Second part of iterative found: {0}", seq.ToCompare.Current);
                        _toCompare = seq.ToCompare.Clone();
                    }
                    else
                    {
                        Log(LogEventLevel.Verbose, "Iterative failed on {0}", seq.ToCompare.Current);
                    }
                }
            } while (funcSuccess);

            return this;
        }

        public static bool AnyOf(ref IEnumerator<Token> enumerator, bool tryMode = true,
            params Func<TokensSequence, TokensSequence>[] seqs)
        {
            var copy = enumerator;
            var any = seqs.Any(seq =>
            {
                var clone = copy.Clone();
                var tokensSequence = Init(ref clone);
                tokensSequence.TryMode = tryMode;
                var result = seq(tokensSequence);
                if (result?.Result == true)
                {
                    copy = result.ToCompare;
                    return true;
                }
                return false;
            });

            if (any)
            {
                Logger.Verbose("AnyOf found: {0}", copy.Current);
                enumerator = copy;
                return true;
            }
            return false;
        }

        public TokensSequence Const()
        {
            Logger.Verbose("Looking for a constant");
            Evaluate(token => token is ConstantToken<float>);
            return this;
        }

        public TokensSequence LabelDef()
        {
            Logger.Verbose("Looking for a label definition");
            Evaluate(token => token is LabelToken);
            return this;
        }

        public TokensSequence AnyFrom(params Func<TokensSequence, TokensSequence>[] seqs)
        {
            var copy = _toCompare;
            var any = seqs.Select(seq =>
            {
                var clone = copy.Clone();
                var tokensSequence = Init(ref clone);
                tokensSequence.TryMode = true;
                var result = seq(tokensSequence);
                if (result.Result)
                {
                    Log(LogEventLevel.Information, "AnyFrom found: {0}", result.ToCompare.Current);
                    copy = result.ToCompare;
                    return result;
                }
                return null;
            }).FirstOrDefault(x => x != null);

            if (any != null)
            {
                _toCompare = copy;
                return any;
            }
            return null;
        }

        public void Log(LogEventLevel level, string messageFormat, Token token = null, bool includePosition = true)
        {
            Logger.Write(level, $"{(includePosition ? token?.Line.ToString() : string.Empty)} {messageFormat}", token);
        }
    }
}