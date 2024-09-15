using Language.Typer;
using Binding = Precedence.Binding;

class PParser
{
    private List<Token>? Tokens;
    private int current = 0;
    private readonly Dictionary<TokenType, ParseRule> rules;
    public TokenType[] binary = [TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.MOD];
    public TokenType[] unary = [TokenType.BANG, TokenType.MINUS];

    public PParser(List<Token> tokens)
    {
        Tokens = tokens;
        rules = new()
        {
            [TokenType.LEFT_PAREN] = new ParseRule(Grouping, null, Binding.NONE),
            [TokenType.RIGHT_PAREN] = new ParseRule(null, null, Binding.NONE),
            [TokenType.LEFT_BRACE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.RIGHT_BRACE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.COMMA] = new ParseRule(null, null, Binding.NONE),
            [TokenType.DOT] = new ParseRule(null, null, Binding.NONE),
            [TokenType.MINUS] = new ParseRule(Unary, Binary, Binding.TERM),
            [TokenType.PLUS] = new ParseRule(null, Binary, Binding.TERM),
            [TokenType.SEMICOLON] = new ParseRule(null, null, Binding.NONE),
            [TokenType.SLASH] = new ParseRule(null, Binary, Binding.FACTOR),
            [TokenType.STAR] = new ParseRule(null, Binary, Binding.FACTOR),
            [TokenType.BANG] = new ParseRule(null, null, Binding.NONE),
            [TokenType.BANG_EQUAL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.EQUAL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.EQUAL_EQUAL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.GREATER] = new ParseRule(null, null, Binding.NONE),
            [TokenType.GREATER_EQUAL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.LESS] = new ParseRule(null, null, Binding.NONE),
            [TokenType.LESS_EQUAL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.IDENTIFIER] = new ParseRule(null, null, Binding.NONE),
            [TokenType.STRING] = new ParseRule(null, null, Binding.NONE),
            [TokenType.NUMBER] = new ParseRule(Number, null, Binding.NONE),
            [TokenType.AND] = new ParseRule(null, null, Binding.NONE),
            [TokenType.CLASS] = new ParseRule(null, null, Binding.NONE),
            [TokenType.ELSE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.FALSE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.FOR] = new ParseRule(null, null, Binding.NONE),
            [TokenType.FUN] = new ParseRule(null, null, Binding.NONE),
            [TokenType.IF] = new ParseRule(null, null, Binding.NONE),
            [TokenType.NIL] = new ParseRule(null, null, Binding.NONE),
            [TokenType.OR] = new ParseRule(null, null, Binding.NONE),
            [TokenType.LOG] = new ParseRule(null, null, Binding.NONE),
            [TokenType.RETURN] = new ParseRule(null, null, Binding.NONE),
            [TokenType.SUPER] = new ParseRule(null, null, Binding.NONE),
            [TokenType.THIS] = new ParseRule(null, null, Binding.NONE),
            [TokenType.TRUE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.VAR] = new ParseRule(null, null, Binding.NONE),
            [TokenType.WHILE] = new ParseRule(null, null, Binding.NONE),
            [TokenType.EOF] = new ParseRule(null, null, Binding.NONE)
        };
    }

    public Expr Parse()
    {
        return Expression();
    }

    private Expr Expression()
    {
        Token lhs = Peek();
        Binding precedence = GetRule(lhs.Type).Precedence;
        return ParsePrecedence(lhs, precedence);
    }

    // 1 + 5 * 10
    private Expr ParsePrecedence(Token token, Binding p)
    {
        if (p >= Binding.PRIMARY)
        {
            ParsePrimary(token);
        }

        Expr lhs = ParsePrimary(token);

        Advance();

        Token lookahead = Look();
        Binding precedence = GetRule(lookahead.Type).Precedence;

        while (binary.Contains(lookahead.Type) && precedence >= p)
        {
            Token operation = Look();
            Binding opPrecedence = GetRule(operation.Type).Precedence;

            Console.WriteLine($"Operation: {operation.Lexeme} at {current} with precedence {opPrecedence}\n");

            Consume(TokenType.NUMBER);

            Expr rhs;

            lookahead = Peek();
            Console.WriteLine($"Operation: {lookahead.Lexeme} at {current}\n");

            while (binary.Contains(lookahead.Type) && precedence >= opPrecedence /*OR RIGHT ASSOCIATIVE OPERATOR*/)
            {
                Binding prec = GetRule(Peek().Type).Precedence > opPrecedence ? opPrecedence + 1 : opPrecedence;
                rhs = ParsePrecedence(Look(), prec);
                lookahead = Peek();

                lhs = new Expr.Binary(lhs, operation, rhs);
            }
        }
        Console.WriteLine(lhs);

        return lhs;
    }

    private static Expr.Literal ParsePrimary(Token token)
    {
        return new Expr.Literal(new Number(), token.Literal);
    }

    private Expr.Literal Number()
    {
        Expr number = ParsePrecedence(Peek(), Binding.PRIMARY);
        return new Expr.Literal(new Number(), number);
    }

    private Expr.Grouping Grouping()
    {
        throw new NotImplementedException();
    }

    private Expr.Unary Unary()
    {
        Token operation = Previous();
        Expr right = ParsePrecedence(Peek(), Binding.UNARY);

        return new Expr.Unary(right, operation);
    }

    private Expr.Binary Binary(Expr left)
    {
        Token operation = Previous();
        TokenType operatorType = operation.Type;
        ParseRule rule = GetRule(operatorType);
        Expr right = ParsePrecedence(Peek(), rule.Precedence + 1);

        return new Expr.Binary(left, operation, right);
    }

    private ParseRule GetRule(TokenType type)
    {
        return rules[type];
    }

    private void Consume(TokenType type)
    {
        if (Check(type)) Advance();
        else Console.WriteLine($"Expected {type} but got {Peek().Type}");
    }

    private Token Look()
    {
        return Tokens![current];
    }

    private Token Peek()
    {
        int next = current + 1;
        return Tokens![next];
    }

    private void Advance()
    {
        if (!IsAtEnd()) current++;
    }

    private bool Check(TokenType type)
    {
        return Peek().Type == type;
    }

    private Token Previous()
    {
        return Tokens![current - 1];
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }
}