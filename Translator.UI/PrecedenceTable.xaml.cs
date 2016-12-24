using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Parser.Precedence;
using Translator.Lexer;

namespace Translator.UI
{
    /// <summary>
    /// Логика взаимодействия для PrecedenceTable.xaml
    /// </summary>
    public partial class PrecedenceTable : Window
    {
        public PrecedenceTable()
        {
            InitializeComponent();
        }

        public void FillTable(IList<KeyValuePair<Token, CompositeToken>> grammar, Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> dict)
        {
            Grid.Columns.Clear();

            var allTokens = new List<Token>();

            void AddAllTokens(Token token)
            {
                void AddToken(Token t)
                {
                    if (!allTokens.Contains(t) && t.Type != TokenType.Axiom)
                    {
                        allTokens.Add(t);
                    }
                }

                if (!(token is CompositeToken))
                {
                    AddToken(token);
                }
                if (token is CompositeToken ct)
                {
                    foreach (var tInner in ct)
                    {
                        AddAllTokens(tInner);
                    }
                }
            }

            foreach (var pair in grammar)
            {
                AddAllTokens(pair.Key);
                AddAllTokens(pair.Value);
            }

            for (var i = 0; i < allTokens.Count; i++)
            {
                var kv = allTokens[i];
                Grid.Columns.Add(new DataGridTextColumn()
                {
                    Header = kv.Substring,
                    Binding = new Binding($"Table[{i}]")
                });
            }

            Grid.ItemsSource = allTokens.Select(x =>
            {
                PrecedenceRelation?[] arr;
                if (!dict.ContainsKey(x))
                {
                    arr = new PrecedenceRelation?[allTokens.Count];
                }
                else
                {
                    var relations = dict[x];
                    arr = allTokens.Select(y => relations.ContainsKey(y) ? relations[y] : null).ToArray();
                }
                return new
                {
                    Header = x.Substring,
                    Table = arr
                };

            }).ToList();
        }
    }
}
