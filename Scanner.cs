class Scanner
{
    private static string Code;
    private static readonly List<Token> Tokens = [];
    private static int start = 0; // the first character in the lexeme being scanned
    private static int current = 0; // the character currently being considered
    private static readonly int line = 1;

    Scanner(string code)
    {
        Code = code;
    }

    public static List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
        }

        Tokens.Add(new Token(TokenType.EOF, "", null, line));
        return Tokens;
    }

    private static bool IsAtEnd()
    {
        return current >= Code.Length;
    }

    private static void ScanToken()
    {
        char c = Code[current++];

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN, null); break;
            case ')': AddToken(TokenType.RIGHT_PAREN, null); break;
            case '{': AddToken(TokenType.LEFT_BRACE, null); break;
            case '}': AddToken(TokenType.RIGHT_BRACE, null); break;
            case ',': AddToken(TokenType.COMMA, null); break;
            case '.': AddToken(TokenType.DOT, null); break;
            case '-': AddToken(TokenType.MINUS, null); break;
            case '+': AddToken(TokenType.PLUS, null); break;
            case ';': AddToken(TokenType.SEMICOLON, null); break;
            case '*': AddToken(TokenType.STAR, null); break;
            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG, null); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL, null); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS, null); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER, null); break;
            default: Console.WriteLine("Unexpected Character"); break;
        }
    }

    private static void AddToken(TokenType type, object? literal)
    {
        string text = Code.Substring(start, current);
        Tokens.Add(new Token(type, text, literal, line));
    }

    private static Boolean Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Code[current] != expected) return false;

        current++;
        return true;
    }
}