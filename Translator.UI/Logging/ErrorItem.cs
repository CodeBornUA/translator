using Serilog.Events;

namespace Translator.UI.Logging
{
    public class ErrorItem
    {
        public ErrorItem(LogEvent e)
        {
            Message = e.RenderMessage();
            Type = e.Level;
        }

        public string Message { get; set; }

        public LogEventLevel Type { get; set; }
    }
}