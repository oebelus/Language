class Token(TokenType type, string lexeme, object? literal, int line)
{
    public static Dictionary<char, Instructions> Operations = new()
    {
        { '+', Instructions.ADD },
        { '-',  Instructions.SUB },
        { '/', Instructions.DIV },
        { '*', Instructions.MUL }
    };

    private readonly TokenType Type = type;
    private readonly string Lexeme = lexeme;
    private readonly object? Literal = literal;
    private readonly int Line = line;
}