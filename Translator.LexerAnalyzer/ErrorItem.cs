﻿using Serilog.Events;

namespace Translator.LexerAnalyzer
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

        public int Line { get; set; }

        public int Position { get; set; }
    }
}