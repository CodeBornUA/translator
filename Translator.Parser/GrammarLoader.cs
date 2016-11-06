using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Translator.Core;
using Translator.Lexer;

namespace Parser
{
    public class GrammarLoader
    {
        private IConfigurationRoot _configuration;

        public GrammarLoader()
        {
            var assembly = typeof(GrammarLoader).GetTypeInfo().Assembly;
            var builder = new ConfigurationBuilder()
                .AddEmbeddedJsonFile(assembly, "grammar.json");

            _configuration = builder.Build();
        }

        //public IEnumerable<TokensSequence> GetSequences()
        //{
        //    var strings = _configuration.GetChildren().ToList();
        //    return strings.Select(kv =>
        //    {
        //        return new TokensSequence(kv.Key, kv.Value);
        //    });
        //}
    }
}
