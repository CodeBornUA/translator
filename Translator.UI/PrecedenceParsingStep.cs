namespace Translator.UI
{
    public class PrecedenceParsingStep
    {
        public string StackContent { get; set; }
        public string Relation { get; set; }
        public string InputTokens { get; set; }
        public string Prn { get; set; }
    }

    public class ComputationStep
    {
        public string Before { get; set; }
        public string Highlighted { get; set; }
        public string After { get; set; }

        public string Stack { get; set; }
    }
}