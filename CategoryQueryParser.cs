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

            operand ->  STRING EQ STRING
                    |   STRING NEQ STRING
                    |   STRING CONTAINS STRING
                    |   STRING EQ BOOLEAN
                    |   STRING NEQ BOOLEAN
                    |   STRING EQ number
                    |   STRING NEQ numbe
                    |   STRING GT number
                    |   STRING LT number
                    |   STRING LE number
                    |   STRING GE number
                    |   LPAREN expr RPAREN
                    |   NOT operand

            number  ->  FLOAT
                    |   INTEGER
                    
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


        [Production("operand: STRING EQ string")]
        [Production("operand: STRING NEQ string")]
        [Production("operand: STRING CONTAINS string")]
        [Production("operand: STRING EQ boolean")]
        [Production("operand: STRING NEQ boolean")]
        [Production("operand: STRING NEQ number")]
        [Production("operand: STRING EQ number")]
        [Production("operand: STRING GT number")]
        [Production("operand: STRING LT number")]
        [Production("operand: STRING LE number")]
        [Production("operand: STRING GE number")]
        public Operand Operand(Token left, Token op, TokenValue right)
        {
            string leftVal = left.StringWithoutQuotes;
            _queryTokens.Add(leftVal);
            switch (op.TokenID)
            {
                case CategoryQueryToken.CONTAINS:
                    return (p) => ((string)p[leftVal]).Contains(right.Value.ToString());
                case CategoryQueryToken.EQ:
                    return (p) => Compare(p[leftVal], right.Value) == 0;
                case CategoryQueryToken.NEQ:
                    return (p) => Compare(p[leftVal], right.Value) != 0;
                case CategoryQueryToken.GT:
                    return (p) => Compare(p[leftVal], right.Value) < 0;
                case CategoryQueryToken.LT:
                    return (p) => Compare(p[leftVal], right.Value) > 0;
                case CategoryQueryToken.LE:
                    return (p) => Compare(p[leftVal], right.Value) >= 0;
                case CategoryQueryToken.GE:
                    return (p) => Compare(p[leftVal], right.Value) <= 0;
            }

            throw new InvalidOperationException($"Invalid operation for '{left.Value} {op.Value} {right}': Can't recognize.");
        }
        [Production("operand: LPAREN expr RPAREN")]
        public Operand Operand(Token _1, Operand expr, Token _2) => expr;
        [Production("operand: NOT operand")]
        public Operand Operand(Token _, Operand expr) => (p) => !expr(p);


        [Production("number: FLOAT")]
        [Production("number: INTEGER")]
        public TokenValue Number(Token tok) => (tok.TokenID == CategoryQueryToken.FLOAT)
            ? new TokenValue(tok.DoubleValue)
            : new TokenValue(tok.IntValue);

        [Production("string: STRING")]
        public TokenValue String(Token tok) => new TokenValue(tok.StringWithoutQuotes);

        [Production("boolean: BOOLEAN")]
        public TokenValue Boolean(Token tok) => new TokenValue(bool.Parse(tok.Value));

        public class TokenValue
        {
            public TokenValue(int val)
            {
                intVal = val;
                valueType = ValueType.Integer;
            }

            public TokenValue(double val)
            {
                doubleVal = val;
                valueType = ValueType.Double;
            }

            public TokenValue(bool val)
            {
                boolVal = val;
                valueType = ValueType.Boolean;
            }

            public TokenValue(string val)
            {
                stringVal = val;
                valueType = ValueType.String;
            }

            public TokenValue(object val)
            {
                objectVal = val;
                valueType = ValueType.Object;
            }

            public object Value
            {
                get
                {
                    switch (valueType)
                    {
                        case ValueType.Boolean:
                            return boolVal;
                        case ValueType.Double:
                            return doubleVal;
                        case ValueType.Integer:
                            return intVal;
                        case ValueType.String:
                            return stringVal;
                        default:
                            return objectVal;
                    }
                }
            }

            bool boolVal;
            int intVal;
            double doubleVal;
            string stringVal;
            object objectVal;

            ValueType valueType;

            enum ValueType
            {
                Object,
                String,
                Integer,
                Double,
                Boolean,
            }
        }

        private static int Compare(object a, object b)
        {                           
            if (a is int && b is double)
            {
                a = Convert.ToDouble(a);
            }

            if (a is double && b is int)
            {
                b = Convert.ToDouble(b);
            }

            return ((IComparable)a).CompareTo(b);
        }
    }
}