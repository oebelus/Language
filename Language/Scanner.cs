class Scanner
{
    private static string Code = Code ?? "";
    private static readonly List<Token> Tokens = [];
    private static int start = 0; // the first character in the lexeme being scanned
    private static int current = 0; // the character currently being considered
    private static int line = 1;

    public Scanner(string code)
    {
        Code = code;
    }

    public static List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        Tokens.Add(new Token(TokenType.EOF, "", new object(), line));
        return Tokens;
    }

    private static bool IsAtEnd()
    {
        return current >= Code.Length;
    }

    private static void ScanToken()
    {
        char c = Advance();

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
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else AddToken(TokenType.SLASH, null);
                break;
            case ' ':
            case '\r':
            case '\t': break;
            case '\n': line++; break;
            default: Console.WriteLine("Unexpected Character"); break;
        }
    }

    private static void AddToken(TokenType type, object? literal)
    {
        string text = Code[start..current];

        Tokens.Add(new Token(type, text, new object(), line));
    }

    private static bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Code[current] != expected) return false;

        current++;
        return true;
    }

    private static char Peek()
    {
        if (IsAtEnd()) return '\0';
        return Code[current];
    }

    private static char Advance()
    {
        return Code[current++];
    }
}