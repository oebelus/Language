using Type = Language.Typer.Type;
using Boolean = Language.Typer.Boolean;
using Void = Language.Typer.Void;
using Integer = Language.Typer.Number;
using Chars = Language.Typer.String;

class RDParser(List<Token> tokens)
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
        if (Match(TokenType.TYPE, TokenType.VAR)) return VarDeclaration();

        if (Match(TokenType.FUN)) return Function();

        return Statement();
    }

    private Statement.VariableStatement VarDeclaration()
    {
        Token typeToken = Previous();
        Type? type = null;

        // Checking if the variable is typed
        if (typeToken.Type == TokenType.TYPE)
        {
            type = TokenToType(typeToken);
        }

        Token name = Consume(TokenType.IDENTIFIER)!;

        Expr? initializer = Match(TokenType.EQUAL) ? Expression() : null;

        Consume(TokenType.SEMICOLON);

        return type != null
            ? new Statement.VariableStatement(type, name, initializer)
            : new Statement.VariableStatement(typeToken, name, initializer);
    }

    private Statement.Function Function()
    {
        Type type = TokenToType(Consume(TokenType.TYPE)!);
        Token name = Consume(TokenType.IDENTIFIER)!;

        Consume(TokenType.LEFT_PAREN);
        List<Statement.Argument> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {

                if (arguments.Count >= 255)
                {
                    Console.WriteLine("Function can't have more than 255 arguments.");
                }

                arguments.Add(new Statement.Argument(TokenToType(Consume(TokenType.TYPE)!), Consume(TokenType.IDENTIFIER)!));

            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN);

        Consume(TokenType.LEFT_BRACE);
        List<Statement> body = Block();

        return new Statement.Function(name, type, arguments, body);
    }

    private Statement Statement()
    {
        if (Match(TokenType.LOG)) return LogStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Statement.Block(Block());
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.BREAK)) return BreakStatement();
        if (Match(TokenType.CONTINUE)) return ContinueStatement();
        return ExpressionStatement();
    }

    private Statement.Continue ContinueStatement()
    {
        Consume(TokenType.SEMICOLON);
        return new Statement.Continue();
    }

    private Statement.Break BreakStatement()
    {
        Consume(TokenType.SEMICOLON);
        return new Statement.Break();
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

        // Checking for initializer
        Statement? initializer;

        // The initializer is null
        if (Match(TokenType.SEMICOLON)) initializer = null;

        // The initializer is a variable declaration
        else if (Match(TokenType.VAR, TokenType.TYPE)) initializer = VarDeclaration();

        // The initializer is an expression
        else initializer = ExpressionStatement();

        Expr? condition = null;

        // Checking for condition
        if (!Check(TokenType.SEMICOLON))
            condition = Expression();

        Consume(TokenType.SEMICOLON);

        // Checking for increment
        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
            increment = Expression();

        Consume(TokenType.RIGHT_PAREN);

        // Checking for body
        Statement body = Statement();

        // If there is an increment, execute it after the body in each iteration
        if (increment != null)
        {
            List<Statement> newBody = [body, new Statement.Expression(increment)];
            // replacing the body with "newBody", a block that contains the original body followed by an expression statement that evaluates the increment
            body = new Statement.Block(newBody);
        }

        // If the condition is omitted, we put true to trigger an infinite loop
        condition ??= new Expr.Literal(new Boolean(), true);

        // Building the loop with the condition and the body using a primitive while loop
        body = new Statement.While(condition, body);

        // If there is an initializer, execute it before the loop
        if (initializer != null) body = new Statement.Block([initializer, body]);

        return body;
    }

    private Statement.If IfStatement()
    {
        Consume(TokenType.LEFT_PAREN);
        Expr condition = Expression();
        Consume(TokenType.RIGHT_PAREN);

        Statement thenBranch = Statement();
        Statement? elseBranch = null;

        if (Match(TokenType.ELSE))
            elseBranch = Statement();

        return new Statement.If(condition, thenBranch, elseBranch!); // to fix null warning later
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
        string keyword = Previous().Lexeme;
        Expr? expr = Expression();
        Consume(TokenType.SEMICOLON);

        return new Statement.Log(keyword, expr);
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
        // Parse left-hand side of assignment
        Expr expression = Or();

        if (Match(TokenType.EQUAL))
        {
            // Parse right-hand side of assignment
            Expr value = Assignment();

            // If the left-hand side is a variable, return an assignment
            if (expression is Expr.VariableExpression varExpr)
            {
                Token name = varExpr.Name;
                return new Expr.Assign(name, value);
            }
            Console.WriteLine("Invalid assignment target: " + Previous().Lexeme);
        }

        // If there is no equal sign, return the left-hand side expression
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

        Token paren = Consume(TokenType.RIGHT_PAREN)!;

        return new Expr.Call((Expr.VariableExpression)expr, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(new Boolean(), true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(new Boolean(), false);
        if (Match(TokenType.NIL)) return new Expr.Literal(new Void(), null);

        if (Match(TokenType.NUMBER)) return new Expr.Literal(new Integer(), Previous().Literal);
        if (Match(TokenType.STRING)) return new Expr.Literal(new Chars(), Previous().Literal);

        if (Match(TokenType.IDENTIFIER)) return new Expr.VariableExpression(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN);
            return new Expr.Grouping(expr);
        }

        return new Expr.Literal(new Void(), null);
    }

    private Token? Consume(TokenType type)
    {
        if (Check(type)) return Advance();
        throw new InvalidOperationException($"Expected token of type {type}, but got {Peek().Type}.");
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
        return !IsAtEnd() && Peek().Type == type;
    }

    private Token Look()
    {
        return Tokens![current];
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

    private static Type TokenToType(Token token)
    {
        return token.Lexeme switch
        {
            "int" => new Integer(),
            "bool" => new Boolean(),
            "string" => new Chars(),
            _ => new Void(),
        };
    }
}