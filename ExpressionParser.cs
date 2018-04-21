using System;
using sly.parser.generator;
using System.Collections.Generic;

using Operand = System.Func<System.Collections.Generic.IDictionary<string, object>, bool>;
using Token = sly.lexer.Token<CategorySelector.QueryToken>;


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
    public class QueryExpressionParser
    {

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
            QueryToken opTok = (QueryToken)op.TokenID;
            QueryToken rightTok = (QueryToken)right.TokenID;

            // check for operation possibility
            switch (rightTok)
            {
                case QueryToken.STRING:
                    switch (opTok)
                    {
                        case QueryToken.GE:
                        case QueryToken.LE:
                        case QueryToken.GT:
                        case QueryToken.LT:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with string values");
                    }
                    break;
                case QueryToken.FLOAT:
                case QueryToken.INTEGER:
                    switch (opTok)
                    {
                        case QueryToken.CONTAINS:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with numeric operand");
                    }
                    break;
                case QueryToken.BOOLEAN:
                    switch (opTok)
                    {
                        case QueryToken.GE:
                        case QueryToken.LE:
                        case QueryToken.GT:
                        case QueryToken.LT:
                        case QueryToken.CONTAINS:
                            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with boolean values");
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operation for '{left.Value} {op.Value} {right.Value}': Operator '{op.Value}' can't be used with this values");
            }

            // select condition checker
            string leftVal = left.Value;
            switch (opTok)
            {
                case QueryToken.GE:
                    return (p) => Convert.ToDouble(p[leftVal]) >= right.DoubleValue;
                case QueryToken.LE:
                    return (p) => Convert.ToDouble(p[leftVal]) <= right.DoubleValue;
                case QueryToken.NEQ:
                    switch (rightTok)
                    {
                        case QueryToken.STRING:
                            return (Operand)((p) => !string.Equals((string)p[leftVal], right.StringWithoutQuotes, StringComparison.InvariantCultureIgnoreCase));
                        case QueryToken.INTEGER:
                            return (Operand)((p) => (int)p[leftVal] != right.IntValue);
                        case QueryToken.FLOAT:
                            return (Operand)((p) => (double)p[leftVal] != right.DoubleValue);
                        case QueryToken.BOOLEAN:
                            return (Operand)((p) => (bool)p[leftVal] != Convert.ToBoolean(right.StringWithoutQuotes));
                    }
                    break;
                case QueryToken.CONTAINS:
                    return (p) => ((string)p[leftVal]).Contains(right.StringWithoutQuotes);
                case QueryToken.EQ:
                    switch (rightTok)
                    {
                        case QueryToken.STRING:
                            return (Operand)((p) => string.Equals((string)p[leftVal], right.StringWithoutQuotes, StringComparison.InvariantCultureIgnoreCase));
                        case QueryToken.INTEGER:
                            return (Operand)((p) => (int)p[leftVal] == right.IntValue);
                        case QueryToken.FLOAT:
                            return (Operand)((p) => (double)p[leftVal] == right.DoubleValue);
                        case QueryToken.BOOLEAN:
                            return (Operand)((p) => (bool)p[leftVal] == Convert.ToBoolean(right.StringWithoutQuotes));
                    }
                    break;
                case QueryToken.GT:
                    return (p) => Convert.ToDouble(p[leftVal]) > right.DoubleValue;
                case QueryToken.LT:
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