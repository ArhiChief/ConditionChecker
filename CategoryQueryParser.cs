using System;
using sly.parser.generator;
using System.Collections.Generic;

using Operand = System.Func<System.Collections.Generic.IDictionary<string, object>, bool>;
using Token = sly.lexer.Token<CategorySelector.CategoryQueryToken>;


namespace CategorySelector
{
    /*
            Grammar (roor -> expr):

            expr    ->  term OR expr
                    |   term

            term    ->  operand AND term
                    |   operand

            operand ->  STRING op STRING
                    |   STRING op INTEGER
                    |   STRING op FLOAT
                    |   STRING op BOOLEAN
                    |   LPAREN expr RPAREN
                    |   NOT operand

            op  ->  GT      // This production is needed as shortcut for "operand" production
                |   LT
                |   GE
                |   LE
                |   EQ
                |   NEQ
                |   CONTAINS
                    
    */
    internal class CategoryQueryParser
    {
        private readonly HashSet<string> _queryTokens;

        public CategoryQueryParser(HashSet<string> queryTokens)
        {
            _queryTokens = queryTokens ?? throw new ArgumentNullException(nameof(queryTokens));
        }


        [Production("expr: term OR expr")]
        public Operand Expr(Operand left, Token _, Operand right) => (p) => left(p) || right(p);

        [Production("expr: term")]
        public Operand Expr(Operand op) => op;


        [Production("term: operand AND term")]
        public Operand Term(Operand left, Token _, Operand right) => (p) => left(p) && right(p);
        [Production("term: operand")]
        public Operand Term(Operand op) => op;



        [Production("operand: STRING op STRING")]
        [Production("operand: STRING op INTEGER")]
        [Production("operand: STRING op FLOAT")]
        [Production("operand: STRING op BOOLEAN")]
        public Operand Operand(Token left, Token op, Token right)
        {
            CategoryQueryToken opTok = (CategoryQueryToken)op.TokenID;
            CategoryQueryToken rightTok = (CategoryQueryToken)right.TokenID;

            // check for operation possibility
            switch (rightTok)
            {
                case CategoryQueryToken.STRING:
                    switch (opTok)
                    {
                        case CategoryQueryToken.GE:
                        case CategoryQueryToken.LE:
                        case CategoryQueryToken.GT:
                        case CategoryQueryToken.LT:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with string values");
                    }
                    break;
                case CategoryQueryToken.FLOAT:
                case CategoryQueryToken.INTEGER:
                    switch (opTok)
                    {
                        case CategoryQueryToken.CONTAINS:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with numeric operand");
                    }
                    break;
                case CategoryQueryToken.BOOLEAN:
                    switch (opTok)
                    {
                        case CategoryQueryToken.GE:
                        case CategoryQueryToken.LE:
                        case CategoryQueryToken.GT:
                        case CategoryQueryToken.LT:
                        case CategoryQueryToken.CONTAINS:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with boolean values");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with this values");
            }

            
            // select condition checker
            string leftVal = left.Value;
            _queryTokens.Add(leftVal);
            switch (opTok)
            {
                case CategoryQueryToken.GE:
                    return (p) => Convert.ToDouble(p[leftVal]) >= right.DoubleValue;
                case CategoryQueryToken.LE:
                    return (p) => Convert.ToDouble(p[leftVal]) <= right.DoubleValue;
                case CategoryQueryToken.NEQ:
                    switch (rightTok)
                    {
                        case CategoryQueryToken.STRING:
                            return (Operand)((p) => !string.Equals((string)p[leftVal], right.StringWithoutQuotes, StringComparison.InvariantCultureIgnoreCase));
                        case CategoryQueryToken.INTEGER:
                            return (Operand)((p) => (int)p[leftVal] != right.IntValue);
                        case CategoryQueryToken.FLOAT:
                            return (Operand)((p) => (double)p[leftVal] != right.DoubleValue);
                        case CategoryQueryToken.BOOLEAN:
                            return (Operand)((p) => (bool)p[leftVal] != Convert.ToBoolean(right.StringWithoutQuotes));
                    }
                    break;
                case CategoryQueryToken.CONTAINS:
                    return (p) => ((string)p[leftVal]).Contains(right.StringWithoutQuotes);
                case CategoryQueryToken.EQ:
                    switch (rightTok)
                    {
                        case CategoryQueryToken.STRING:
                            return (Operand)((p) => string.Equals((string)p[leftVal], right.StringWithoutQuotes, StringComparison.InvariantCultureIgnoreCase));
                        case CategoryQueryToken.INTEGER:
                            return (Operand)((p) => (int)p[leftVal] == right.IntValue);
                        case CategoryQueryToken.FLOAT:
                            return (Operand)((p) => (double)p[leftVal] == right.DoubleValue);
                        case CategoryQueryToken.BOOLEAN:
                            return (Operand)((p) => (bool)p[leftVal] == Convert.ToBoolean(right.StringWithoutQuotes));
                    }
                    break;
                case CategoryQueryToken.GT:
                    return (p) => Convert.ToDouble(p[leftVal]) > right.DoubleValue;
                case CategoryQueryToken.LT:
                    return (p) => Convert.ToDouble(p[leftVal]) < right.DoubleValue;
                default:
                    throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used here");
            }

            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Can't recognize.");
        }
        [Production("operand: LPAREN expr RPAREN")]
        public Operand Operand(Token _1, Operand expr, Token _2) => expr;
        [Production("operand: NOT operand")]
        public Operand Operand(Token _, Operand expr) => (p) => !expr(p);

        [Production("op: GT")]
        [Production("op: LT")]
        [Production("op: GE")]
        [Production("op: LE")]
        [Production("op: EQ")]
        [Production("op: NEQ")]
        [Production("op: CONTAINS")]
        public Token Op(Token tok) => tok;
    }
}