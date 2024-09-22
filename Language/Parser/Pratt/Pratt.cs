using Language.Typer;
using Binding = Precedence.Binding;
using Number = Language.Typer.Number;

class Pratt
{
    private List<Token>? Tokens;
    private int current = 0;
    private readonly Dictionary<TokenType, ParseRule> rules;
    public TokenType[] binary = [TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.MOD];
    public TokenType[] unary = [TokenType.BANG, TokenType.MINUS];

    public Pratt(List<Token> tokens)
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

    public List<Expr> Parse()
    {
        List<Expr> statements = [];
        while (!IsAtEnd()) statements.Add(Expression());
        return statements;
    }

    private Expr Expression()
    {
        // Start with the lowest precedence
        return ParsePrecedence(Binding.NONE);
    }

    public Expr ParseExpression(Binding precedence)
    {
        // Token token = Advance();
        // PrefixParselet prefix =
        throw new NotImplementedException();
    }

    // 1 + 5 * 10 / 11 - 5
    private Expr ParsePrecedence(Binding precedence)
    {
        // peek next token
        Token token = Advance();

        var prefixRule = GetRule(token.Type).Prefix ?? throw new InvalidOperationException($"Expected expression at {Previous()}");

        bool canAssign = precedence <= Binding.ASSIGNMENT;
        Expr expr = prefixRule(this);

        while (precedence < GetRule(Look().Type).Precedence)
        {
            token = Advance();
            var infixRule = GetRule(token.Type).Infix;

            if (infixRule == null) break;

            expr = infixRule(this, expr);
        }

        if (canAssign && Match(TokenType.EQUAL))
        {
            throw new InvalidOperationException("Invalid assignment target.");
        }

        return expr;
    }

    private Expr.Literal Number(Pratt _)
    {
        return new Expr.Literal(new Number(), Previous().Literal);
    }

    private Expr.Grouping Grouping(Pratt _)
    {
        // Consume(TokenType.LEFT_PAREN);
        Expr expression = Expression();
        Consume(TokenType.RIGHT_PAREN);
        return new Expr.Grouping(expression);
    }

    private Expr.Unary Unary(Pratt _)
    {
        Token operation = Previous();
        Expr right = ParsePrecedence(Binding.UNARY);

        return new Expr.Unary(right, operation);
    }

    private Expr.Binary Binary(Pratt _, Expr left)
    {
        Token operation = Previous();
        ParseRule rule = GetRule(operation.Type);
        Expr right = ParsePrecedence(rule.Precedence + 1);

        return new Expr.Binary(left, operation, right);
    }

    private ParseRule GetRule(TokenType type)
    {
        return rules[type];
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

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool Check(TokenType type)
    {
        return Peek().Type == type;
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Previous()
    {
        return Tokens![current - 1];
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private void Consume(TokenType type)
    {
        if (Check(type))
        {
            Advance();
        }
        throw new InvalidOperationException($"Expected token of type {type}, but got {Peek().Type}.");
    }

}