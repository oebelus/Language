class Token(TokenType type, string lexeme, object literal, int line)
{
    public TokenType Type = type;
    public string Lexeme = lexeme;
    public object Literal = literal ?? "null";
    public int Line = line;

    public static void TokenLogger(Token token)
    {
        Console.WriteLine($"Type: {token.Type}, Lexeme: {token.Lexeme}, Literal: {token.Literal}, Line: {token.Line}");
    }
}

enum TokenType
{
    // Punctuators.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,
    OR, AND, MOD,

    // One or two character tokens.
    BANG, BANG_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,

    // Literals.
    IDENTIFIER, STRING, NUMBER, TYPE,

    // Keywords.
    ELSE, FALSE, FUN, FOR, IF, NIL,
    LOG, RETURN, TRUE, VAR, WHILE,

    EOF
}