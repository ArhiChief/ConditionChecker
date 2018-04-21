using sly.lexer;

namespace CategorySelector
{
    internal enum CategoryQueryToken
    {
        [Lexeme("true|false")]
        BOOLEAN,
        [Lexeme("[+-]?([0-9]+\\.[0-9]+)")]
        FLOAT,
        [Lexeme("[+-]?[0-9]+")]
        INTEGER,
        [Lexeme("\"[^\"]*\"|\\w+")]
        STRING,
        
        [Lexeme(">=")]
        GE,
        [Lexeme("<=")]
        LE,
        [Lexeme("!=")]
        NEQ,
        [Lexeme("~")]
        CONTAINS,
        [Lexeme("=")]
        EQ,
        [Lexeme(">")]
        GT,
        [Lexeme("<")]
        LT,

        [Lexeme("!")]
        NOT,
        [Lexeme("\\|\\|")]
        OR,
        [Lexeme("&&")]
        AND,
        [Lexeme("\\(")]
        LPAREN,
        [Lexeme("\\)")]
        RPAREN,

        [Lexeme("[ \\t]+", true)]
        WS,
        [Lexeme("[\\r\\n]", true, true)]
        EOL
    }
}
