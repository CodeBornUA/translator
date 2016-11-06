using System.Text;
using System.Text.RegularExpressions;

namespace Translator.Lexer
{
    public class StringToken : Token
    {

        private readonly StringBuilder _value = new StringBuilder();

        public override TokenType Type => TokenType.Keyword;

        public void Append(char symbol)
        {
            _value.Append(symbol);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override string Substring => ToString();

        public string Escaped => Regex.Escape(ToString());
    }
}