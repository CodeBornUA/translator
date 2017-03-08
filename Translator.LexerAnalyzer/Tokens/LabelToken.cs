using System.Linq;

namespace Translator.LexerAnalyzer.Tokens
{
    public class LabelToken : StringToken
    {
        private readonly string _substring;

        public LabelToken(string substring)
        {
            _substring = substring;
            Name = _substring.Split(':').First();
        }

        public string Name { get; set; }
        public override TokenType Type => TokenType.Label;

        public override string ToString()
        {
            return _substring;
        }
    }
}