﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Parser.Precedence;
using Translator.LexerAnalyzer.Tokens;

namespace Translator.UI
{
    /// <summary>
    ///     Логика взаимодействия для PrecedenceTable.xaml
    /// </summary>
    public partial class PrecedenceTable : Window
    {
        public PrecedenceTable()
        {
            InitializeComponent();
        }

        public void FillTable(IList<KeyValuePair<Token, CompositeToken>> grammar,
            Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> dict)
        {
            Grid.Columns.Clear();

            var allTokens = new List<Token>();

            void AddAllTokens(Token token)
            {
                void AddToken(Token t)
                {
                    if (!allTokens.Contains(t) && t.Type != TokenType.Axiom)
                        allTokens.Add(t);
                }

                if (!(token is CompositeToken))
                    AddToken(token);
                if (token is CompositeToken ct)
                    foreach (var tInner in ct)
                        AddAllTokens(tInner);
            }

            //Dictionary<TKey, TValue> UnionDict<TKey, TValue>(Dictionary<TKey, TValue> dictA, Dictionary<TKey, TValue> dictB)
            //{
            //    var d = new Dictionary<TKey, TValue>();
            //    foreach (var VARIABLE in COLLECTION)
            //    {

            //    }
            //}

            foreach (var pair in grammar)
            {
                AddAllTokens(pair.Key);
                AddAllTokens(pair.Value);
            }


            foreach (var kv in allTokens)
            {
                if (!dict.ContainsKey(kv))
                    dict[kv] = new Dictionary<Token, PrecedenceRelation?>();
                dict[kv][PrecedenceParser.TokenEnum.Sharp] = PrecedenceRelation.More;
            }
            dict.Add(PrecedenceParser.TokenEnum.Sharp,
                allTokens.ToDictionary(x => x, x => (PrecedenceRelation?) PrecedenceRelation.Less));
            allTokens.Add(PrecedenceParser.TokenEnum.Sharp);

            for (var i = 0; i < allTokens.Count; i++)
            {
                var kv = allTokens[i];
                Grid.Columns.Add(new DataGridTextColumn
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