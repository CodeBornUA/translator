using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public class VariableStore : Dictionary<IdentifierToken, ConstantToken<float>>
    {
    }
}