using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;

namespace CategorySelector
{
    class Program
    {
        static void Main(string[] args)
        {
            var lexerRes = LexerBuilder.BuildLexer<QueryToken>(new BuildResult<ILexer<QueryToken>>());
            var tokenStream = lexerRes.Result.Tokenize("!!(a != w) && !!(a = \"Misc works\" && a != test) && b >=45 && c = false" ).ToList();

            QueryExpressionParser queryParserDef = new QueryExpressionParser();

            var parserBuilder = new ParserBuilder<QueryToken, object>();
            var parser = parserBuilder.BuildParser(queryParserDef, ParserType.LL_RECURSIVE_DESCENT, "expr");

            var pres = parser.Result.Parse(tokenStream);

            var res = (Func<IDictionary<string, object>, bool>)pres.Result;

            var dict = new Dictionary<string, object>
            {
                { "a", "Misc works" },
                { "b", 45 },
                { "c", false }
            };

            Console.WriteLine(res(dict));
        }
    }
}
