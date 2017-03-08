using System.Text;
using System.Text.RegularExpressions;

namespace Translator.LexerAnalyzer.Tokens
{
    public class StringToken : Token
    {
        private readonly StringBuilder _value = new StringBuilder();

        public StringToken()
        {
        }

        public StringToken(string s)
        {
            _value.Append(s);
        }

        public override TokenType Type { get; set; } = TokenType.Keyword;

        public override string Substring => ToString();

        public string Escaped => Regex.Escape(ToString());

        public void Append(char symbol)
        {
            _value.Append(symbol);
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}