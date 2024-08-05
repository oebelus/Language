using System.Linq.Expressions;

class Parser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    private int current = 0;

    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!IsAtEnd()) statements.Add(Declaration());
        return statements;
    }

    private Statement Declaration() {
        try {
            if (Match(TokenType.VAR)) return VarDeclaration();

            return Statement();
        } catch (Exception) {
            Synchronize();
            return null!;
        }
    }

    private Statement.Var VarDeclaration() {
        Token name = Consume(TokenType.IDENTIFIER);

        Expr initializer = null!; 
        if (Match(TokenType.EQUAL)) initializer = Expression();
        
        Consume(TokenType.SEMICOLON);

        return new Statement.Var(name, initializer!);
    } 

    private Statement Statement()
    {
        if (Match(TokenType.LOG)) return LogStatement();

        return ExpressionStatement();
    }

    private Statement.Log LogStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON);
        return new Statement.Log(expr);
    }

    private Statement.Expression ExpressionStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SEMICOLON);
        return new Statement.Expression(value);
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token operation = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, operation, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token operation = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, operation, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token operation = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, operation, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token operation = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, operation, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        while (Match(TokenType.MINUS, TokenType.BANG))
        {
            Token operation = Previous();
            Expr right = Unary();
            Console.WriteLine("> BINARY: " + operation.Lexeme + " " + right);
            return new Expr.Unary(right, operation);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(Previous().Literal);

        if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN);
            return new Expr.Grouping(expr);
        }

        return new Expr.Literal(null);
    }

    private Token Consume(TokenType type)
    {
        if (Check(type)) return Advance();
        return null!;
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

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return Tokens[current];
    }

    private Token Previous()
    {
        return Tokens[current - 1];
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) return;

            switch (Peek().Type)
            {
                case TokenType.FUN:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.LOG:
                case TokenType.VAR:
                case TokenType.WHILE:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }
}