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

    private Statement Declaration()
    {
        if (Match(TokenType.VAR)) return VarDeclaration();
        if (Match(TokenType.FUN)) return Function("function");

        return Statement();
    }

    private Statement.VariableStatement VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER);

        Expr initializer = null!;
        if (Match(TokenType.EQUAL)) initializer = Expression();

        Consume(TokenType.SEMICOLON);

        return new Statement.VariableStatement(name, initializer!);
    }

    private Statement.Function Function(string kind)
    {
        Token name = Consume(TokenType.IDENTIFIER);

        Consume(TokenType.LEFT_PAREN);
        List<Token> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {

                if (arguments.Count >= 255)
                {
                    Console.WriteLine("Function can't have more than 255 arguments.");
                }

                arguments.Add(Consume(TokenType.IDENTIFIER));

            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN);

        Consume(TokenType.LEFT_BRACE);
        List<Statement> body = Block();

        return new Statement.Function(name, arguments, body);
    }

    private Statement Statement()
    {
        if (Match(TokenType.LOG)) return LogStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Statement.Block(Block());
        if (Match(TokenType.RETURN)) return ReturnStatement();
        return ExpressionStatement();
    }

    private Statement.Return ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(TokenType.SEMICOLON))
            value = Expression();

        Consume(TokenType.SEMICOLON);
        return new Statement.Return(keyword, value!);
    }

    private Statement ForStatement()
    {
        Consume(TokenType.LEFT_PAREN);

        Statement? initializer;
        if (Match(TokenType.SEMICOLON)) initializer = null;
        else if (Match(TokenType.VAR)) initializer = VarDeclaration();
        else initializer = ExpressionStatement();

        Expr? condition = null;

        if (!Check(TokenType.SEMICOLON))
            condition = Expression();

        Consume(TokenType.SEMICOLON);

        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
            increment = Expression();

        Consume(TokenType.RIGHT_PAREN);

        Statement body = Statement();

        if (increment != null)
        {
            List<Statement> alist = [body, new Statement.Expression(increment)];
            body = new Statement.Block(alist);
        }

        condition ??= new Expr.Literal(true);
        body = new Statement.While(condition, body);

        List<Statement> list = [initializer, body];
        if (initializer != null) body = new Statement.Block(list);

        return body;
    }

    private Statement.If IfStatement()
    {
        Consume(TokenType.LEFT_PAREN);
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN);

        Statement thenBranch = Statement();
        Statement elseBranch = null!;

        if (Match(TokenType.ELSE))
            elseBranch = Statement();

        return new Statement.If(condition, thenBranch, elseBranch);
    }

    private Statement.While WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN);
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN);

        Statement body = Statement();

        return new Statement.While(condition, body);
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
        return Assignment();
    }

    private List<Statement> Block()
    {
        List<Statement> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE);

        return statements;
    }

    /*
     * Assignment: Uses recursion to handle the right-hand side of the assignment, right-associativity.
     * Binary: Uses a loop to handle sequences of the same operator, left-associativity.
    */

    private Expr Assignment()
    {
        Expr expression = Or();

        if (Match(TokenType.EQUAL))
        {
            Expr value = Assignment();

            if (expression is Expr.VariableExpression varExpr)
            {
                // Token equals = Previous();
                Token name = varExpr.Name;
                return new Expr.Assign(name, value);
            }
            Console.WriteLine("Invalid assignment target: " + Previous().Lexeme);
        }
        return expression;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OR))
        {
            Token operation = Previous();
            Expr right = And();

            expr = new Expr.Logical(expr, operation, right);
        }

        return expr;
    }

    private Expr And()
    {
        Expr expr = Equality();

        while (Match(TokenType.AND))
        {
            Token operation = Previous();
            Expr right = And();

            expr = new Expr.Logical(expr, operation, right);
        }

        return expr;
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

        while (Match(TokenType.SLASH, TokenType.STAR, TokenType.MOD))
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

            return new Expr.Unary(right, operation);
        }

        return Call();
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (Match(TokenType.LEFT_PAREN))
            expr = FinishCall(expr);

        return expr;
    }

    private Expr.Call FinishCall(Expr expr)
    {
        List<Expr> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Console.WriteLine("Function can't have more than 255 arguments.");
                    break;
                }
                arguments.Add(Expression());
            }
            while (Match(TokenType.COMMA));
        }

        Token paren = Consume(TokenType.RIGHT_PAREN);

        return new Expr.Call((Expr.VariableExpression)expr, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(Previous().Literal);

        if (Match(TokenType.IDENTIFIER)) return new Expr.VariableExpression(Previous());

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
}