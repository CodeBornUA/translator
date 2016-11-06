using Serilog.Events;

namespace Translator.Lexer
{
    public class ErrorItem
    {
        public string Message { get; set; }

        public LogEventLevel Type { get; set; }

        public int Line { get; set; }

        public int Position { get; set; }

        public ErrorItem(LogEvent e)
        {
            Message = e.RenderMessage();
            Type = e.Level;
        }
    }
}