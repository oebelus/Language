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