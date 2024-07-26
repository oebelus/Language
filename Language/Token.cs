class Token(TokenType type, string lexeme, object literal, int line)
{
    public static Dictionary<char, Instructions> Operations = new()
    {
        { '+', Instructions.ADD },
        { '-', Instructions.SUB },
        { '/', Instructions.DIV },
        { '*', Instructions.MUL }
    };

    public TokenType Type = type;
    public string Lexeme = lexeme;
    public object Literal = literal ?? new object();
    public int Line = line;

    public static void TokenLogger(Token token)
    {
        Console.WriteLine($"Type: {token.Type}, Lexeme: {token.Lexeme}, Literal: {token.Literal}, Line: {token.Line}");
    }
}