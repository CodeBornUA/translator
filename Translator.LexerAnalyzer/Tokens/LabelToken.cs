using System.Linq;
using System.Text.RegularExpressions;
using Translator.Lexer;

namespace Translator.LexerAnalyzer.Tokens
{
    public class LabelToken : StringToken
    {
        private readonly string _substring;
        public string Name { get; set; }
        public override TokenType Type => TokenType.Label;

        public override string ToString()
        {
            return _substring;
        }

        public LabelToken(string substring)
        {
            _substring = substring;
            Name = _substring.Split(':').First();
        }
    }
}
