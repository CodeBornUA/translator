using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Precedence
{
    public class PrecedenceGrammarHelper
    {
        private readonly ILogger _logger;

        public PrecedenceGrammarHelper(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<Token> FirstPlus(IList<GrammarReplaceRule> grammar, Token token)
        {
            var list = new List<Token>();

            void Impl(Token tokenIn)
            {
                if (tokenIn == null || list.Contains(tokenIn))
                    return;

                list.Add(tokenIn);
                if (tokenIn.Type == TokenType.Nonterminal)
                    foreach (var kv in grammar.Where(_ => _.Token == tokenIn))
                        Impl(kv.CompositeToken.FirstOrDefault());
            }

            foreach (var source in grammar.Where(x => x.Token == token))
                Impl(source.CompositeToken.First());

            return list.Distinct();
        }

        private IEnumerable<Token> First(IList<KeyValuePair<Token, CompositeToken>> grammar, Token token)
        {
            var list = new List<Token>();

            void Impl(Token tokenIn)
            {
                if (tokenIn == null || list.Contains(tokenIn))
                    return;

                list.Add(tokenIn);
            }

            foreach (var source in grammar.Where(x => x.Key == token))
                Impl(source.Value.First());

            return list.Distinct();
        }

        public IEnumerable<Token> LastPlus(IList<GrammarReplaceRule> grammar, Token token)
        {
            var list = new List<Token>();

            void Impl(Token tokenIn)
            {
                if (tokenIn == null || list.Contains(tokenIn))
                    return;

                list.Add(tokenIn);
                if (tokenIn.Type == TokenType.Nonterminal)
                    foreach (var kv in grammar.Where(_ => _.Token == tokenIn))
                        Impl(kv.CompositeToken.LastOrDefault());
            }

            foreach (var source in grammar.Where(x => x.Token == token))
                Impl(source.CompositeToken.Last());

            return list.Distinct();
        }

        public Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> GetPrecedenceTable(
            IList<GrammarReplaceRule> grammar)
        {
            var dict = new Dictionary<Token, Dictionary<Token, PrecedenceRelation?>>();

            //Equal
            foreach (var token in grammar)
            {
                for (var index = 0; index < token.CompositeToken.Count - 1; index++)
                    //From first to before last elements
                {
                    var tokenInner = token.CompositeToken[index];
                    if (index < token.CompositeToken.Count)
                    {
                        var nextToken = token.CompositeToken[index + 1];
                        SetRelation(dict, tokenInner, nextToken, PrecedenceRelation.Equal);
                    }
                }

                if (!dict.ContainsKey(token.CompositeToken[token.CompositeToken.Count - 1]))
                    dict[token.CompositeToken[token.CompositeToken.Count - 1]] =
                        new Dictionary<Token, PrecedenceRelation?>();
            }

            //>
            foreach (var row in dict.Where(k => k.Key.Type == TokenType.Nonterminal).ToList())
                //All relations where first token is non-terminal
            {
                var leftToken = row.Key;
                foreach (
                        var relation in
                        row.Value.Where(r => r.Key.Type != TokenType.Nonterminal && r.Value == PrecedenceRelation.Equal))
                    //= relations
                {
                    var rightToken = relation.Key;
                    var lastPlus = LastPlus(grammar, leftToken); //Last+ of left non-terminal
                    foreach (var token in lastPlus)
                        SetRelation(dict, token, rightToken, PrecedenceRelation.More);
                }
            }

            //<
            foreach (var row in dict.Where(k => k.Value.Any(c => c.Key.Type == TokenType.Nonterminal)).ToList())
                //All rows that contain relations where last token is non-terminal
            {
                var leftToken = row.Key;
                foreach (
                    var relation in
                    row.Value.Where(
                        r =>
                            row.Key.Type != TokenType.Nonterminal && r.Key.Type == TokenType.Nonterminal &&
                            r.Value == PrecedenceRelation.Equal).ToList()) //= relations
                {
                    var rightToken = relation.Key;
                    var firstPlus = FirstPlus(grammar, rightToken); //First+ of right non-terminal
                    foreach (var token in firstPlus)
                        SetRelation(dict, leftToken, token, PrecedenceRelation.Less);
                }
            }

            //Two nonterminals
            foreach (var row in dict.Where(k => k.Value.Any(c => c.Key.Type == TokenType.Nonterminal)).ToList())
                //All rows that contain relations where last token is non-terminal
            {
                var leftToken = row.Key;
                foreach (
                    var relation in
                    row.Value.Where(
                        r =>
                            row.Key.Type == TokenType.Nonterminal && r.Key.Type == TokenType.Nonterminal &&
                            r.Value == PrecedenceRelation.Equal).ToList()) //= relations
                {
                    var rightToken = relation.Key;
                    var lastPlus = LastPlus(grammar, leftToken).ToList();
                    var firstPlus = FirstPlus(grammar, rightToken).ToList(); //First+ of right non-terminal
                    foreach (var token in lastPlus)
                    foreach (var token2 in firstPlus)
                        SetRelation(dict, token, token2, PrecedenceRelation.More);

                    //LEFT < FIRST(RIGHT)
                    foreach (var token2 in firstPlus)
                        SetRelation(dict, leftToken, token2, PrecedenceRelation.Less);
                }
            }

            foreach (var kv in dict)
                dict[kv.Key][PrecedenceParser.TokenEnum.Sharp] = PrecedenceRelation.More;
            dict.Add(PrecedenceParser.TokenEnum.Sharp,
                dict.Keys.ToDictionary(x => x, x => (PrecedenceRelation?) PrecedenceRelation.Less));


            return dict;
        }

        private void SetRelation(Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> relationMatrix,
            Token leftToken,
            Token rightToken,
            PrecedenceRelation precedenceRelation)
        {
            if (!relationMatrix.ContainsKey(leftToken))
                relationMatrix[leftToken] = new Dictionary<Token, PrecedenceRelation?>();

            if (relationMatrix[leftToken].ContainsKey(rightToken) &&
                precedenceRelation != relationMatrix[leftToken][rightToken])
            {
                _logger.Error("Conflict: {0} {1} (was {2}, attempted {3})", leftToken, rightToken,
                    relationMatrix[leftToken][rightToken], precedenceRelation);
                throw new InvalidOperationException("There is an another relation in this cell already");
            }
            relationMatrix[leftToken][rightToken] = precedenceRelation;
        }
    }

    public enum PrecedenceRelation
    {
        Less,
        Equal,
        More
    }

    public class CompositeToken : Token, IList<Token>
    {
        private readonly List<Token> _tokens = new List<Token>();

        public override TokenType Type { get; set; } = TokenType.Composite;

        public IEnumerator<Token> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _tokens).GetEnumerator();
        }

        public void Add(Token item)
        {
            _tokens.Add(item);
        }

        public void Clear()
        {
            _tokens.Clear();
        }

        public bool Contains(Token item)
        {
            return _tokens.Contains(item);
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            _tokens.CopyTo(array, arrayIndex);
        }

        public bool Remove(Token item)
        {
            return _tokens.Remove(item);
        }

        public int Count
        {
            get { return _tokens.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<Token>) _tokens).IsReadOnly; }
        }

        public int IndexOf(Token item)
        {
            return _tokens.IndexOf(item);
        }

        public void Insert(int index, Token item)
        {
            _tokens.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _tokens.RemoveAt(index);
        }

        public Token this[int index]
        {
            get { return _tokens[index]; }
            set { _tokens[index] = value; }
        }
    }
}