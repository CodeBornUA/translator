using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using Parser;
using Parser.Executor;
using Parser.Precedence;
using Serilog.Events;
using Translator.LexerAnalyzer;
using Translator.LexerAnalyzer.Tokens;

namespace Translator.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly ILifetimeScope _scope = (Application.Current as App)?.ServiceProvider;

        private readonly Lexer _lexer;
        private readonly IParser _parser;
        private VariableStore _variables;


        public MainWindow()
        {
            _lexer = _scope.Resolve<Lexer>();
            _parser = _scope.Resolve<IParser>();
            _variables = _scope.Resolve<VariableStore>();

            (_parser as PrecedenceParser).StackChanged += MainWindow_StackChanged;
            //(_parser as PrecedenceParser).PRNChanged += (token, prn) =>
            //{
            //    if (token == PrecedenceParser.TokenEnum.Statement)
            //    {
            //        MessageBox.Show($"PRN: {string.Join(", ", prn)}. Result: {PrnExpressionExecutor.ComputeExpression(prn, _variables)}");
            //        prn.Clear();
            //    }
            //};

            ViewModel = _scope.Resolve<MainWindowViewModel>();

            InitializeComponent();
        }

        public MainWindowViewModel ViewModel { get; set; }

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
                
                valid = valid && _parser.CheckSyntax(_lexer.Parsed);
                if (valid)
                {
                    MessageBox.Show("Program is valid");

                    _variables[ViewModel.Identifiers.First(x => x.Name == "a")] = new ConstantToken<float>(1);
                    _variables[ViewModel.Identifiers.First(x => x.Name == "b")] = new ConstantToken<float>(2);
                    _variables[ViewModel.Identifiers.First(x => x.Name == "c")] = new ConstantToken<float>(3);
                    MessageBox.Show(PrnExpressionExecutor.ComputeExpression((_parser as PrecedenceParser).Prn, _variables).Value.ToString());

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

        private void MainWindow_StackChanged(Stack<Token> stack, PrecedenceRelation relation,
            ArraySegment<Token> segment, List<Token> prn)
        {
            ViewModel.PrecedenceParsingSteps.Add(new PrecedenceParsingStep
            {
                StackContent = stack.Aggregate(string.Empty, (agr, cur) => string.Join(" ", cur.Substring, agr)).Trim(),
                InputTokens = segment.Aggregate(string.Empty, (agr, cur) => string.Join(" ", agr, cur.Substring)).Trim(),
                Prn = prn.Aggregate(string.Empty, (agr, cur) => string.Join(" ", agr, cur.Substring)).Trim(),
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
}