using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator;
using TransformedQuery = System.Func<System.Collections.Generic.IDictionary<string, object>, bool>;

namespace CategorySelector
{
    public class CategoryQueryTransformator : ICategoryQueryTransformator
    {
        public (bool isOk, string[] errors) Transform(string query, out TransformedQuery transformedQuery, out HashSet<string> queryChecks)
        {
            transformedQuery = null;
            queryChecks = null;

            var lexerBuilder = LexerBuilder.BuildLexer<CategoryQueryToken>(new BuildResult<ILexer<CategoryQueryToken>>());

            if (lexerBuilder.IsError)
            {
                return (false, lexerBuilder.Errors.Select(error => error.Message).ToArray());
            }

            List<Token<CategoryQueryToken>> tokenStream = null;

            try // we need this block because Exception could be thrown during lexical analyze because of invalid character in example
            {   
                tokenStream = lexerBuilder.Result.Tokenize(query).ToList();
            }
            catch (LexerException<CategoryQueryToken> e)
            {
                return (false, new[] { e.Error.ErrorMessage });
            }

            queryChecks = new HashSet<string>(5);
            CategoryQueryParser categoryQueryParser = new CategoryQueryParser(queryChecks);
            var parserBuilder = new ParserBuilder<CategoryQueryToken, object>();

            var parser = parserBuilder.BuildParser(categoryQueryParser, ParserType.LL_RECURSIVE_DESCENT, "expr");

            if (parser.IsError)
            {
                queryChecks = null;
                return (false, parser.Errors.Select(error => error.Message).ToArray());
            }

            var parsingResult = parser.Result.Parse(tokenStream);

            if (parsingResult.IsError)
            {
                queryChecks = null;
                return (false, parsingResult.Errors.Select(error => error.ErrorMessage).ToArray());
            }

            transformedQuery = (TransformedQuery)parsingResult.Result;

            return (true, null);
        }
    }
}