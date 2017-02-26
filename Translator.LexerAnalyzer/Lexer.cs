using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Stateless;
using Translator.Core;
using Translator.LexerAnalyzer.Tokens;
using IContainer = Autofac.IContainer;
using StateMachine = Stateless.StateMachine<Translator.Lexer.LexerState, Translator.Lexer.Symbol>;

namespace Translator.Lexer
{

    public class Lexer
    {
        private readonly IObserver<LogEvent> _logObserver;
        private StringToken _currentToken;
        private readonly IList<string> _tokens;

        private readonly ICollection<Token> _parsed = new ObservableCollection<Token>();
        private readonly ICollection<Identifier> _identifiers = new ObservableCollection<Identifier>();
        private readonly ICollection<Constant<float>> _constants = new ObservableCollection<Constant<float>>();
        private readonly ICollection<LabelToken> _labels = new ObservableCollection<LabelToken>();
        private readonly IList<SymbolClass> _classes;
        private readonly StateMachine _machine;
        private int _line;
        private int _position;
        private readonly LexerValidator _lexerValidator;

        public Logger Logger { get; set; }

        public StringToken CurrentToken
        {
            get { return _currentToken ?? (_currentToken = new StringToken()); }
            set { _currentToken = value; }
        }

        #region Configuration

        public static IContainer ApplicationContainer { get; private set; }

        public static IConfigurationRoot Configuration { get; set; }

        private void Configure()
        {
            var assembly = typeof(Lexer).GetTypeInfo().Assembly;
            var builder = new ConfigurationBuilder()
                .AddEmbeddedJsonFile(assembly, "grammar.json");

            Configuration = builder.Build();

            // Create the container builder.
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(new LoggerFactory().AddSerilog())
                .As<ILoggerFactory>();

            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Observers(ConfigureObservers)
                .CreateLogger();

            ApplicationContainer = containerBuilder.Build();
        }

        private void ConfigureObservers(IObservable<LogEvent> observable)
        {
            if (_logObserver != null)
            {
                observable.Subscribe(_logObserver);
            }
        }

        #endregion

        public Lexer(IObserver<LogEvent> logObserver = null)
        {
            _logObserver = logObserver;
            Configure();

            _machine = CreateMachine();

            _tokens = GetTokens();
            _classes = GetClasses();

            (_identifiers as ObservableCollection<Identifier>).CollectionChanged += (sender, args) => SetTokenIndex(args, sender);
            (_constants as ObservableCollection<Constant<float>>).CollectionChanged += (sender, args) => SetTokenIndex(args, sender);
            //(_labels as ObservableCollection<LabelToken>).CollectionChanged += (sender, args) => SetTokenIndex(args, sender);
            _lexerValidator = new LexerValidator(this);
        }

        private static void SetTokenIndex(NotifyCollectionChangedEventArgs args, object sender)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                ((dynamic)args.NewItems[0]).Index = (sender as ICollection).Count - 1;
            }
        }

        public StateMachine<LexerState, Symbol> CreateMachine()
        {
            Log(LogEventLevel.Information, "Configuring the state machine");
            var machine = new StateMachine<LexerState, Symbol>(LexerState.Initial);

            machine.OnUnhandledTrigger((state, symbol) => ReturnToken(CurrentToken, symbol));
            machine.OnTransition(transition =>
            {
                Log(LogEventLevel.Information, "Transition: {0} -> ({1}) -> {2}", true, transition.Source, Regex.Escape(transition.Trigger.ToString()), transition.Destination);
                CurrentToken.Append((char) transition.Trigger.Value);
            });

            machine.Configure(LexerState.Initial)
                .Permit(Symbol.Letter, LexerState.String)
                .Permit(Symbol.Digit, LexerState.Number)
                .Permit(Symbol.Operator, LexerState.Operator)
                .Permit(Symbol.Point, LexerState.Point)
                .Permit(Symbol.Less, LexerState.LessOperator)
                .Permit(Symbol.Greater, LexerState.GreaterOperator)
                .Permit(Symbol.Equal, LexerState.AssignmentOperator)
                .Permit(Symbol.Exclamation, LexerState.Not)
                .Permit(Symbol.Splitter, LexerState.Splitter)
                .Permit(Symbol.Comma, LexerState.Comma)
                .Permit(Symbol.Hypen, LexerState.Hypen)
                .Permit(Symbol.Colon, LexerState.Colon)
                .Ignore(Symbol.Space)
                .OnUnhandled(Error);

            machine.Configure(LexerState.String)
                .PermitReentry(Symbol.Letter)
                .PermitReentry(Symbol.Digit)
                .OnUnhandled(ReturnIdOrTokenOrLabel);

            machine.Configure(LexerState.Number)
                .PermitReentry(Symbol.Digit)
                .Permit(Symbol.Point, LexerState.NumberWithPoint)
                .OnUnhandled(ReturnConst);

            machine.Configure(LexerState.NumberWithPoint)
               .PermitReentry(Symbol.Digit)
               .OnUnhandled(ReturnConst);

            machine.Configure(LexerState.Point)
                .Permit(Symbol.Digit, LexerState.NumberWithPoint)
                .OnUnhandled(Error);

            machine.Configure(LexerState.LessOperator)
                .Permit(Symbol.Equal, LexerState.LessEqual);

            machine.Configure(LexerState.GreaterOperator)
                .Permit(Symbol.Equal, LexerState.GreaterEqual);

            machine.Configure(LexerState.AssignmentOperator)
                .Permit(Symbol.Equal, LexerState.EqualOperator);

            machine.Configure(LexerState.Not)
                .Permit(Symbol.Equal, LexerState.NotEqual);

            machine.Configure(LexerState.Splitter)
                .PermitReentry(Symbol.Splitter);

            return machine;
        }

        private void ReturnToken(Token token, Symbol trigger)
        {
            if (_tokens.Contains(CurrentToken.ToString()) && CurrentToken.TokenIndex == null)
            {
                CurrentToken.TokenIndex = _tokens.IndexOf(CurrentToken.ToString()) + 1;
            }
            Log(LogEventLevel.Information, "Added token: {0}", false, CurrentToken.Escaped);
            _parsed.Add(token);
            CurrentToken = new StringToken() { Line = _line };

            _machine.Reset();
            if (trigger != Symbol.Space && trigger.Value != null)
            {
                _machine.Fire(trigger);
            }
        }

        private void ReturnConst(LexerState state, Symbol trigger)
        {
            Log(LogEventLevel.Information, "Found a constant");
            var value = Constant<float>.Parse(CurrentToken.ToString());
            Constant<float> con = _constants.FirstOrDefault(x => Math.Abs(x.Value - value) < 1E-5)?.Clone() as Constant<float>;
            if (con == null)
            {
                con = new Constant<float>(CurrentToken.ToString())
                {
                    TokenIndex = ConstIndex,
                    Substring = CurrentToken.ToString()
                };
                _constants.Add(con);
            }
            else
            {
                Log(LogEventLevel.Information, "The constant is already processed");
            }
            con.Line = _line;
            ReturnToken(con, trigger);
        }

        private void ReturnLabel(StateMachine.Transition transition)
        {
            Log(LogEventLevel.Information, "Found a label: {0}", false, CurrentToken);
            var name = CurrentToken.Substring.Trim(':');
            var existingLabel = _labels.FirstOrDefault(l => l.Name == name);
            //var label = new LabelToken(name)
            //{
            //    Line = _line,
            //    TokenIndex = LabelIndex,
            //    Index = existingLabel != null ? existingLabel.Index : _labels.Count
            //};
            //_labels.Add(label);

            var label = _identifiers.FirstOrDefault(x => x.Name == name)?.Clone() as LabelToken;
            if (label == null)
            {
                label = new LabelToken(name)
                {
                    Line = _line,
                    TokenIndex = LabelIndex,
                    Index = existingLabel != null ? existingLabel.Index : _labels.Count
                };
                _labels.Add(label);
            }
            label.Line = _line;

            ReturnToken(label, transition.Trigger);
        }

        private void Error(LexerState state, Symbol trigger)
        {
            Log(LogEventLevel.Error, "Error while parsing tokens. State: {0}, trigger: {1}", true, state, trigger);
            throw new InvalidOperationException();
        }

        private void ReturnIdOrTokenOrLabel(LexerState lexerState, Symbol symbol)
        {
            if (_tokens.Contains(CurrentToken.ToString()))
            {
                CurrentToken.TokenIndex = _tokens.IndexOf(CurrentToken.ToString()) + 1;
                Log(LogEventLevel.Information, "Found token {0}", false, CurrentToken);
                ReturnToken(CurrentToken, symbol);
            }
            else if(symbol.Class.Class == Class.Colon || _parsed.Last().Substring == "goto")
            {
                //Label
                ReturnLabel(new StateMachine<LexerState, Symbol>.Transition(lexerState, LexerState.LabelDefinition, symbol));
            }
            else
            {
                Log(LogEventLevel.Information, "Not found token - treat as ID: {0}", false, CurrentToken);
                Identifier identifier = _identifiers.FirstOrDefault(x => x.Name == CurrentToken.ToString())?.Clone() as Identifier;
                if (identifier == null)
                {
                    identifier = new Identifier(CurrentToken.ToString())
                    {
                        TokenIndex = IdIndex
                    };
                    _identifiers.Add(identifier);
                }
                identifier.Line = _line;
                ReturnToken(identifier, symbol);
            }
        }

        public int? IdIndex => _tokens.Count + 1;
        public int? ConstIndex => _tokens.Count + 2;
        public int? LabelIndex => _tokens.Count + 3;

        public int Position
        {
            get { return _position; }
        }

        public int Line
        {
            get { return _line; }
        }

        public ICollection<Token> Parsed
        {
            get { return _parsed; }
        }

        public ICollection<Identifier> Identifiers
        {
            get { return _identifiers; }
        }

        public ICollection<Constant<float>> Constants
        {
            get { return _constants; }
        }

        public ICollection<LabelToken> Labels
        {
            get { return _labels; }
        }

        public ICollection<Token> ParseTokens(TextReader reader)
        {
            Log(LogEventLevel.Information, "Start of parsing");
            Reset();

            while (true)
            {
                var symbol = reader.Read();
                if (symbol == -1)
                {
                    Log(LogEventLevel.Information, "EOF", true);
                    _machine.Fire(new Symbol(null));
                    break;
                }
                _position++;

                _machine.Fire(new Symbol((char)symbol, _classes));

                if (symbol == '\n')
                {
                    Log(LogEventLevel.Information, "New line reached");
                    _line++;
                    _position = 0;
                }
            }

            Log(LogEventLevel.Information, "End of parsing");
            return _parsed;
        }

        private void Reset()
        {
            Log(LogEventLevel.Information, "State machine was reset");

            _line = 1;
            _position = 1;
            _machine.Reset();
            CurrentToken = new StringToken() { Line = _line };
            _identifiers.Clear();
            _constants.Clear();
            _labels.Clear();
            _parsed.Clear();
        }

        public IList<string> GetTokens()
        {
            Log(LogEventLevel.Information, "Loading tokens...");
            return Configuration.GetSection("tokens").GetChildren().Select(x => x.Value).ToList();
        }

        public IList<SymbolClass> GetClasses()
        {
            Log(LogEventLevel.Information, "Loading classes...");
            return Enum.GetValues(typeof(Class)).Cast<Class>().Select(@class => new SymbolClass()
            {
                Class = @class,
                Symbols = Configuration["classes:" + @class.ToString("G")].ToCharArray()
            }).ToList();
        }

        public void Validate(IList<Token> tokens)
        {
            _lexerValidator.ValidateIds(tokens);
            _lexerValidator.ValidateLabels(tokens);
        }

        public void Log(LogEventLevel level, string message, bool includePosition = false)
        {
            Logger.Write(level, $"{(includePosition ? Line + ":" + Position : string.Empty)} {message}");
        }

        public void Log(LogEventLevel level, string messageFormat, bool includePosition = true, params object[] objs)
        {
            Logger.Write(level, $"{(includePosition ? Line + ":" + Position : string.Empty)} {messageFormat}", objs);
        }
    }
}
