using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Translator.LexerAnalyzer.Tokens;

namespace Translator.LexerAnalyzer
{
    public class LexerValidator
    {
        private readonly Lexer _lexer;

        public LexerValidator(Lexer lexer)
        {
            _lexer = lexer;
        }

        public void ValidateLabels(IList<Token> tokens)
        {
            var labelUsages = tokens.SkipWhile(x => x.Substring != "begin").OfType<LabelToken>().Distinct();
            var labels = tokens.Where(x => x.Type == TokenType.Label).OfType<LabelToken>();

            var notUsed = labels.Where(x => labelUsages.All(us => us.Substring != x.Name));
            foreach (var labelToken in notUsed)
                _lexer.Log(LogEventLevel.Warning, "Unused label: {0} at line {1}", false, labelToken.Name,
                    labelToken.Line);

            var undefined = labelUsages.Where(us => labels.All(x => us.Substring != x.Name));
            foreach (var labelToken in undefined.OfType<IdentifierToken>())
                _lexer.Log(LogEventLevel.Error, "Undefined label: {0} at line {1}", false, labelToken.Name,
                    labelToken.Line);
        }

        public void ValidateIds(IList<Token> tokens)
        {
            var idDefinitions =
                tokens.SkipWhile(x => x.Substring != "var")
                    .TakeWhile(x => x.Substring != "\r\n")
                    .OfType<IdentifierToken>()
                    .Distinct()
                    .ToList();
            var ids = tokens.SkipWhile(x => x.Substring != "begin").OfType<IdentifierToken>().Distinct();

            var undefined = ids.Where(x => idDefinitions.All(us => us.Name != x.Name));
            foreach (var idToken in undefined)
                _lexer.Log(LogEventLevel.Error, "Undefined id: {0} at line {1}", false, idToken.Name, idToken.Line);

            var unused = idDefinitions.Where(us => ids.All(x => us.Name != x.Name));
            foreach (var idToken in unused)
                _lexer.Log(LogEventLevel.Warning, "Unused id: {0} at line {1}", false, idToken.Name, idToken.Line);
        }
    }
}