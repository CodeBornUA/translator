using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Parser.Executor;
using Serilog.Events;
using Translator.LexerAnalyzer.Tokens;
using Translator.UI.Logging;

namespace Translator.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly VariableStore _store;
        private IEnumerable<Token> _allTokens;
        private IEnumerable<ConstantToken<float>> _constants;
        private IEnumerable<IdentifierToken> _identifiers;
        private IEnumerable<LabelToken> _labels;
        private LogEventLevel _level;

        public MainWindowViewModel(VariableStore store)
        {
            _store = store;
            LogMessages.CollectionChanged += (sender, args) => { OnPropertyChanged(nameof(LogMessagesEnumerable)); };
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

        public IEnumerable<IdentifierToken> Identifiers
        {
            get { return _identifiers; }
            set
            {
                _identifiers = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Tuple<IdentifierToken, ConstantToken<float>>> IdValues => _identifiers == null
            ? null
            : (from id in Identifiers
                join value in _store on id.Name equals value.Key.Name into vs
                from v in vs.DefaultIfEmpty()
                select new Tuple<IdentifierToken, ConstantToken<float>>(id, v.Value));

        public IEnumerable<ConstantToken<float>> Constants
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

        public IEnumerable<ErrorItem> LogMessagesEnumerable
        {
            get { return LogMessages.Where(x => x.Type >= _level); }
        }

        public ObservableCollection<ErrorItem> LogMessages { get; } = new ObservableCollection<ErrorItem>();

        public ObservableCollection<PrecedenceParsingStep> PrecedenceParsingSteps { get; } =
            new ObservableCollection<PrecedenceParsingStep>();
        public ObservableCollection<ComputationStep> ComputationSteps { get; } =
            new ObservableCollection<ComputationStep>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Reset()
        {
            LogMessages.Clear();
            PrecedenceParsingSteps.Clear();
            ComputationSteps.Clear();
        }

        public void UpdateIdValues()
        {
            OnPropertyChanged(nameof(IdValues));
        }
    }
}