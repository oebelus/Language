class Scanner
{
    private static string Code = Code ?? "";
    private static readonly List<Token> Tokens = [];
    private static int start = 0; // the first character in the lexeme being scanned
    private static int current = 0; // the character currently being considered
    private static int line = 1;
    private static readonly Dictionary<string, TokenType> Keywords = new() {
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "function", TokenType.FUN },
        { "for", TokenType.FOR },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "log", TokenType.LOG },
        { "return", TokenType.RETURN },
        { "true", TokenType.TRUE },
        { "let", TokenType.VAR },
        { "while", TokenType.WHILE },
        { "or", TokenType.OR },
        { "and", TokenType.AND },
    };

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
            case '%': AddToken(TokenType.MOD, null); break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd()) // A comment goes until the end of the line.
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
            case '"': String(); break;
            default:
                if (char.IsDigit(c)) HandleNumber();
                else if (IsAlpha(c)) HandleAlpha();
                else throw new InvalidOperationException("Unexpected Character"); break;
        }
    }

    private static void AddToken(TokenType type, object? literal)
    {
        string text = Code[start..current];

        Tokens.Add(new Token(type, text, literal ?? "", line));
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

    private static char PeekNext()
    {
        if (current + 1 >= Code.Length) return '\0';
        return Code[current += 1];
    }

    private static char Advance()
    {
        return Code[current++];
    }

    private static void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Console.WriteLine("Unterminated string.");
            return;
        }

        Advance(); // to find the closing "

        string value = Code.Substring(start + 1, current - start - 2);
        AddToken(TokenType.STRING, value);
    }

    private static void HandleNumber()
    {
        while (char.IsDigit(Peek())) Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        float value = float.Parse(Code[start..current]);
        AddToken(TokenType.NUMBER, value);
    }

    private static void HandleAlpha()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = Code[start..current];

        TokenType tokenType;

        if (Keywords.TryGetValue(text, out TokenType value))
            tokenType = value;
        else
            if (text == "num" || text == "bool")
        {
            tokenType = TokenType.TYPE;
            AddToken(tokenType, text);
            return;
        }
        else
            tokenType = TokenType.IDENTIFIER;

        AddToken(tokenType, null);
    }

    private static bool IsAlpha(char c)
    {
        return c == '_' || char.IsLetter(c);
    }

    private static bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || char.IsDigit(c);
    }
}
