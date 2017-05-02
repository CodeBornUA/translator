using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    internal class ForContext
    {
        public LabelToken LoopLabel { get; set; }
        public LabelToken ExitLabel { get; set; }

        public IdentifierToken Parameter { get; set; }
        public IdentifierToken ToIdentifier { get; set; }
    }
}
