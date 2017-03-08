using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Parser;
using Serilog.Events;
using Parser.Precedence;
using Translator.Lexer;

namespace Translator.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly Lexer.Lexer _lexer;
        private readonly IParser _parser;
        private MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindowViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged();
            }
        }


        public MainWindow()
        {
            _lexer = new Lexer.Lexer(new LogObserver(_viewModel));
            _parser = new PrecedenceParser(new LogObserver(_viewModel));
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AnalyzeButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.Reset();
                _lexer.ParseTokens(new StringReader(sourceTextBox.Text));
                ViewModel.AllTokens = _lexer.Parsed;
                ViewModel.Identifiers = _lexer.Identifiers;
                ViewModel.Constants = _lexer.Constants;
                ViewModel.Labels = _lexer.Labels.Distinct();
                //_lexer.Validate(ViewModel.AllTokens.ToList());

                var valid = !ViewModel.LogMessages.Any(x => x.Type >= LogEventLevel.Error);

                (_parser as PrecedenceParser).StackChanged += MainWindow_StackChanged;
                (_parser as PrecedenceParser).PRNChanged += (token, prn) =>
                {
                    if (token == PrecedenceParser.TokenEnum.Statement)
                    {
                        prn.Clear();
                    }
                };
                valid = valid && _parser.CheckSyntax(_lexer.Parsed);
                if (valid)
                {
                    MessageBox.Show("Program is valid");
                    return;
                }

                throw new Exception();

                //var table = new PrecedenceTable();
                //table.FillTable(PrecedenceParser.Grammar, (_parser as PrecedenceParser).Precedence);
                //table.ShowDialog();
            }
            catch (Exception)
            {
                MessageBox.Show("Program contains errors");
                //Ignore exceptions - all errors will be in log
            }
        }

        private void MainWindow_StackChanged(Stack<Token> stack, PrecedenceRelation relation, ArraySegment<Token> segment)
        {
            ViewModel.PrecedenceParsingSteps.Add(new PrecedenceParsingStep()
            {
                StackContent = stack.Aggregate(string.Empty, (agr, cur) => string.Join(" ", cur.Substring, agr)).Trim(),
                InputTokens = segment.Aggregate(string.Empty, (agr, cur) => string.Join(" ", agr, cur.Substring)).Trim(),
                Relation = relation.ToString()
            });
        }

        /// <summary>
        ///     Sets widths of columns in table to equal
        /// </summary>
        private void SetMinWidth(object sender, RoutedEventArgs e)
        {
            foreach (var column in (sender as DataGrid).Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }
    }

    internal class LogObserver : IObserver<LogEvent>
    {
        private readonly MainWindowViewModel _view;

        public LogObserver(MainWindowViewModel view)
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

    public class ErrorItem
    {
        public string Message { get; set; }

        public LogEventLevel Type { get; set; }

        public ErrorItem(LogEvent e)
        {
            Message = e.RenderMessage();
            Type = e.Level;
        }
    }
}
