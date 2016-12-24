using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator.Lexer;

namespace Parser.Precedence
{
    public class PrecedenceGrammarHelper
    {
        public IEnumerable<Token> FirstPlus(IList<KeyValuePair<Token, CompositeToken>> grammar, Token token)
        {
            var list = new List<Token>();
            void Impl(Token tokenIn)
            {
                if (tokenIn == null || list.Contains(tokenIn))
                {
                    return;
                }

                list.Add(tokenIn);
                if (tokenIn.Type == TokenType.Nonterminal)
                {
                    foreach (var kv in grammar.Where(_ => _.Key == tokenIn))
                    {
                        Impl(kv.Value.FirstOrDefault());
                    }
                }
            }

            foreach (var source in grammar.Where(x => x.Key == token))
            {
                Impl(source.Value.First());
            }

            return list.Distinct();
        }

        public IEnumerable<Token> LastPlus(IList<KeyValuePair<Token, CompositeToken>> grammar, Token token)
        {
            var list = new List<Token>();
            void Impl(Token tokenIn)
            {
                if (tokenIn == null || list.Contains(tokenIn))
                {
                    return;
                }

                list.Add(tokenIn);
                if (tokenIn.Type == TokenType.Nonterminal)
                {
                    foreach (var kv in grammar.Where(_ => _.Key == tokenIn))
                    {
                        Impl(kv.Value.LastOrDefault());
                    }
                }
            }

            foreach (var source in grammar.Where(x => x.Key == token))
            {
                Impl(source.Value.Last());
            }

            return list.Distinct();
        }

        public Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> GetPrecedenceTable(IList<KeyValuePair<Token, CompositeToken>> grammar)
        {
            var dict = new Dictionary<Token, Dictionary<Token, PrecedenceRelation?>>();

            //Equal
            foreach (var token in grammar)
            {
                for (var index = 0; index < token.Value.Count - 1; index++) //From first to before last elements
                {
                    var tokenInner = token.Value[index];
                    if (index < token.Value.Count)
                    {
                        var nextToken = token.Value[index + 1];
                        SetRelation(dict, tokenInner, nextToken, PrecedenceRelation.Equal);
                    }
                }
            }

            //>
            foreach (var row in dict.Where(k => k.Key.Type == TokenType.Nonterminal).ToList()) //All relations where first token is non-terminal
            {
                var leftToken = row.Key;
                foreach (var relation in row.Value.Where(r => r.Key.Type != TokenType.Nonterminal && r.Value == PrecedenceRelation.Equal)) //= relations
                {
                    var rightToken = relation.Key;
                    var lastPlus = LastPlus(grammar, leftToken); //Last+ of left non-terminal
                    foreach (var token in lastPlus)
                    {
                        //Last+ > rightToken
                        SetRelation(dict, token, rightToken, PrecedenceRelation.More);
                    }
                }
            }

            //<
            foreach (var row in dict.Where(k => k.Value.Any(c => c.Key.Type == TokenType.Nonterminal)).ToList()) //All rows that contain relations where last token is non-terminal
            {
                var leftToken = row.Key;
                foreach (var relation in row.Value.Where(r => row.Key.Type != TokenType.Nonterminal && r.Key.Type == TokenType.Nonterminal && r.Value == PrecedenceRelation.Equal).ToList()) //= relations
                {
                    var rightToken = relation.Key;
                    var firstPlus = FirstPlus(grammar, rightToken); //First+ of right non-terminal
                    foreach (var token in firstPlus)
                    {
                        //leftToken < FIRST+
                        SetRelation(dict, leftToken, token, PrecedenceRelation.Less);
                    }
                }
            }

            //Two nonterminals
            foreach (var row in dict.Where(k => k.Value.Any(c => c.Key.Type == TokenType.Nonterminal)).ToList()) //All rows that contain relations where last token is non-terminal
            {
                var leftToken = row.Key;
                foreach (var relation in row.Value.Where(r => row.Key.Type == TokenType.Nonterminal && r.Key.Type == TokenType.Nonterminal && r.Value == PrecedenceRelation.Equal).ToList()) //= relations
                {
                    var rightToken = relation.Key;
                    var lastPlus = LastPlus(grammar, leftToken).ToList();
                    var firstPlus = FirstPlus(grammar, rightToken).ToList(); //First+ of right non-terminal
                    foreach (var token in lastPlus)
                    {
                        foreach (var token2 in firstPlus)
                        {
                            //LAST+(LEFT) > FIRST+(RIGHT)
                            SetRelation(dict, token, token2, PrecedenceRelation.More);
                        }
                    }
                }
            }

            return dict;
        }

        private static void SetRelation(Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> relationMatrix, 
            Token leftToken, 
            Token rightToken, 
            PrecedenceRelation precedenceRelation)
        {
            if (!relationMatrix.ContainsKey(leftToken))
            {
                relationMatrix[leftToken] = new Dictionary<Token, PrecedenceRelation?>();
            }

            if (relationMatrix[leftToken].ContainsKey(rightToken) && precedenceRelation != relationMatrix[leftToken][rightToken])
            {
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
        private List<Token> _tokens = new List<Token>();
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

        public override TokenType Type { get; set; } = TokenType.Composite;
    }
}
