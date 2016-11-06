using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog.Events;
using Translator.Lexer;
using Translator.LexerAnalyzer.Tokens;

namespace Translator.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private IEnumerable<Token> _allTokens;
        private IEnumerable<Identifier> _identifiers;
        private IEnumerable<Constant<float>> _constants;
        private IEnumerable<LabelToken> _labels;
        private LogEventLevel _level;
        private readonly ObservableCollection<ErrorItem> _logMessages = new ObservableCollection<ErrorItem>();

        public MainWindowViewModel()
        {
            _logMessages.CollectionChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(LogMessagesEnumerable));
            };
        }

        public IEnumerable<Token> AllTokens
        {
            get { return _allTokens; }
            set
            {
                _allTokens = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Identifier> Identifiers
        {
            get { return _identifiers; }
            set
            {
                _identifiers = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Constant<float>> Constants
        {
            get { return _constants; }
            set
            {
                _constants = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LabelToken> Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged();
            }
        }

        public LogEventLevel Level
        {
            get { return _level; }
            set
            {
                if (value != _level)
                {
                    _level = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LogMessages));
                    OnPropertyChanged(nameof(LogMessagesEnumerable));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ErrorItem> LogMessagesEnumerable
        {
            get
            {
                return _logMessages.Where(x => x.Type >= _level);
            }
        }

        public ObservableCollection<ErrorItem> LogMessages
        {
            get
            {
                return _logMessages;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Reset()
        {
            LogMessages.Clear();
        }
    }
}