using System;
using Serilog.Events;

namespace Translator.UI.Logging
{
    internal class MainWindowLogObserver : IObserver<LogEvent>
    {
        private readonly MainWindowViewModel _view;

        public MainWindowLogObserver(MainWindowViewModel view)
        {
            _view = view;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(LogEvent value)
        {
            _view.LogMessages.Add(new ErrorItem(value));
        }
    }
}