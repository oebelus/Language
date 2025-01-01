using Precedence = Binding.Precedence;
using Void = Language.Typer.Void;
using Boolean = Language.Typer.Boolean;
using Integer = Language.Typer.Number;
using Chars = Language.Typer.String;
using Type = Language.Typer.Type;
using System.Data;

class Pratt
{
    private readonly List<Token>? Tokens;
    private int current = 0;
    private readonly Dictionary<TokenType, Precedence> precedences;
    public TokenType[] comparison = [TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL];
    private bool isForLoop = false;

    public Pratt(List<Token> tokens)
    {
        Tokens = tokens;
        precedences = new()
        {
            [TokenType.LEFT_PAREN] = Precedence.NONE,
            [TokenType.RIGHT_PAREN] = Precedence.NONE,
            [TokenType.LEFT_BRACE] = Precedence.NONE,
            [TokenType.RIGHT_BRACE] = Precedence.NONE,
            [TokenType.COMMA] = Precedence.NONE,
            [TokenType.DOT] = Precedence.NONE,
            [TokenType.MINUS] = Precedence.TERM,
            [TokenType.MOD] = Precedence.TERM,
            [TokenType.PLUS] = Precedence.TERM,
            [TokenType.SEMICOLON] = Precedence.NONE,
            [TokenType.SLASH] = Precedence.FACTOR,
            [TokenType.STAR] = Precedence.FACTOR,
            [TokenType.BANG] = Precedence.UNARY,
            [TokenType.BANG_EQUAL] = Precedence.EQUALITY,
            [TokenType.EQUAL] = Precedence.NONE,
            [TokenType.EQUAL_EQUAL] = Precedence.EQUALITY,
            [TokenType.GREATER] = Precedence.COMPARISON,
            [TokenType.GREATER_EQUAL] = Precedence.COMPARISON,
            [TokenType.LESS] = Precedence.COMPARISON,
            [TokenType.LESS_EQUAL] = Precedence.COMPARISON,
            [TokenType.IDENTIFIER] = Precedence.NONE,
            [TokenType.STRING] = Precedence.NONE,
            [TokenType.NUMBER] = Precedence.NONE,
            [TokenType.AND] = Precedence.AND,
            [TokenType.FALSE] = Precedence.NONE,
            [TokenType.OR] = Precedence.OR,
            [TokenType.LOG] = Precedence.NONE,
            [TokenType.BREAK] = Precedence.NONE,
            [TokenType.CONTINUE] = Precedence.NONE,
            [TokenType.TYPE] = Precedence.NONE,
        };
    }

    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!IsAtEnd()) statements.Add(ParseStatement());
        return statements;
    }


    public Statement ParseStatement()
    {
        //if (Match(TokenType.CLASS)) return Class();
        if (Match(TokenType.FUN)) return Function();
        if (Match(TokenType.VAR, TokenType.TYPE)) return Var();
        if (Match(TokenType.RETURN)) return ReturnStatement();
        if (Match(TokenType.BREAK)) return Break();
        if (Match(TokenType.CONTINUE)) return Continue();
        if (Match(TokenType.WHILE)) return While();
        if (Match(TokenType.FOR)) return For();
        if (Match(TokenType.IF)) return If();
        if (Match(TokenType.LOG)) return Log();
        if (Match(TokenType.LEFT_BRACE)) return new Statement.Block(Block());

        return new Statement.Expression(ParseExpression(0));
    }

    private Statement.Continue Continue()
    {
        Consume(TokenType.SEMICOLON);
        return new Statement.Continue();
    }

    private Statement.Break Break()
    {
        Consume(TokenType.SEMICOLON);
        return new Statement.Break();
    }

    private Statement.Log Log()
    {
        string keyword = Previous().Lexeme;
        Expr? expr = ParseExpression(0);
        Consume(TokenType.SEMICOLON);
        return new Statement.Log(keyword, expr);
    }

    private Statement.If If()
    {
        Expr condition = ParseExpression(Precedence.CONDITIONAL);

        Statement thenBranch = ParseStatement();

        Statement? elseBranch = null;

        if (Match(TokenType.ELSE))
        {
            elseBranch = ParseStatement();
        }

        return new Statement.If(condition, thenBranch, elseBranch!); // to fix null warning later
    }

    private Statement For()
    {
        Consume(TokenType.LEFT_PAREN);

        Statement? initializer;

        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }

        else if (Match(TokenType.VAR, TokenType.TYPE))
        {
            initializer = Var();
        }

        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;

        if (!Check(TokenType.RIGHT_PAREN))
        {
            condition = ParseExpression(0);
        }

        Consume(TokenType.SEMICOLON);

        Expr? increment = null;

        if (!Check(TokenType.RIGHT_PAREN))
        {
            isForLoop = true;
            increment = ParseExpression(precedences[TokenType.RIGHT_PAREN]);
            isForLoop = false;
        }

        Consume(TokenType.RIGHT_PAREN);

        Statement body = ParseStatement();

        if (increment != null)
        {
            List<Statement> bodyList = [body, new Statement.Expression(increment)];
            body = new Statement.Block(bodyList);
        }

        condition ??= new Expr.Literal(new Boolean(), true);

        body = new Statement.While(condition, body);

        if (initializer != null) body = new Statement.Block([initializer, body]);

        return body;
    }

    private Statement.Expression ExpressionStatement()
    {
        Expr value = ParseExpression(0);
        Consume(TokenType.SEMICOLON);
        return new Statement.Expression(value);
    }

    private Statement.While While()
    {
        Consume(TokenType.LEFT_PAREN);
        Expr condition = ParseExpression(0);
        Consume(TokenType.RIGHT_PAREN);

        Statement body = ParseStatement();

        return new Statement.While(condition, body);
    }

    private Statement.Return ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;

        if (!Check(TokenType.SEMICOLON))
            value = ParseExpression(0);

        Consume(TokenType.SEMICOLON);
        return new Statement.Return(keyword, value!);
    }

    private Statement.VariableStatement Var()
    {
        Token typeToken = Previous();
        Type? type = null;

        if (typeToken.Type == TokenType.TYPE)
        {
            type = TokenToType(typeToken);
        }

        Token name = Consume(TokenType.IDENTIFIER);

        Expr? initializer = null;

        if (Match(TokenType.EQUAL))
        {
            initializer = ParseExpression(Precedence.ASSIGNMENT);
        }

        Consume(TokenType.SEMICOLON);

        return type != null
            ? new Statement.VariableStatement(type, name, initializer)
            : new Statement.VariableStatement(typeToken, name, initializer);
    }

    private Statement.Function Function()
    {
        Token typeToken = Consume(TokenType.TYPE);
        Token name = Consume(TokenType.IDENTIFIER);

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

                arguments.Add(new Statement.Argument(TokenToType(Consume(TokenType.TYPE)), Consume(TokenType.IDENTIFIER)));
            }
            while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN);

        Consume(TokenType.LEFT_BRACE);

        List<Statement> body = Block();

        return new Statement.Function(name, TokenToType(typeToken), arguments, body);
    }

    private List<Statement> Block()
    {
        List<Statement> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(ParseStatement());
        }

        Consume(TokenType.RIGHT_BRACE);

        return statements;
    }

    public Expr ParseExpression(Precedence precedence)
    {
        Expr? left = ParseAtom(); // NUD

        while (left != null && precedences[Look().Type] != Precedence.STATEMENT && precedences[Look().Type] > precedence && !Check(TokenType.SEMICOLON))
        {
            left = ParseInfix(left); // LED
        }

        if (left != null) return left;
        else throw new Exception("Failed to parse expression.");
    }

    private Expr? ParseAtom()
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(new Boolean(), true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(new Boolean(), false);
        if (Match(TokenType.NIL)) return new Expr.Literal(new Void(), null);

        if (Match(TokenType.NUMBER)) return new Expr.Literal(new Integer(), Previous().Type == TokenType.NUMBER ? Previous().Lexeme : Look().Lexeme);
        if (Match(TokenType.STRING)) return new Expr.Literal(new Chars(), Previous().Lexeme);

        if (Match(TokenType.LEFT_PAREN))
        {
            return Grouping();
        }

        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            return Unary();
        }

        if (Look().Type == TokenType.IDENTIFIER)
        {
            Token name = Look();
            Advance();
            if (Match(TokenType.EQUAL))
            {
                Expr value = ParseExpression(Precedence.NONE);
                if (!isForLoop) Consume(TokenType.SEMICOLON);
                return new Expr.Assign(name, value);
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                List<Expr> args = [];
                if (!Check(TokenType.RIGHT_PAREN))
                {
                    do
                    {
                        args.Add(ParseExpression(Precedence.NONE));
                    }
                    while (Match(TokenType.COMMA));
                }

                Token paren = Consume(TokenType.RIGHT_PAREN);
                Consume(TokenType.SEMICOLON);
                return new Expr.Call(new Expr.VariableExpression(name), paren, args);
            }
            return new Expr.VariableExpression(Previous());
        }

        if (Previous().Type == TokenType.LOG && Look().Type == TokenType.SEMICOLON)
        {
            return null;
        }


        throw new Exception($"Failed to parse atom. {Look().Type}");
    }

    private Expr ParseInfix(Expr left)
    {
        Token operation = Look();

        if (Match(TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.MOD))
        {
            return Binary(left, operation);
        }
        if (Match(TokenType.AND, TokenType.OR))
        {
            return Logical(left, operation);
        }
        if (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
        {
            return Equality(left, operation);
        }
        if (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            return Comparison(left, operation);
        }

        throw new SyntaxErrorException();
    }

    private Expr.Logical Logical(Expr left, Token operation)
    {
        Precedence precedence = precedences[operation.Type];
        Expr right = ParseExpression(precedence);
        return new Expr.Logical(left, operation, right);
    }

    private Expr.Grouping Grouping()
    {
        Expr expr = ParseExpression(precedences[TokenType.LEFT_PAREN]);
        Consume(TokenType.RIGHT_PAREN);
        return new Expr.Grouping(expr);
    }

    private Expr.Unary Unary()
    {
        Token operation = Previous();
        Expr right = ParseExpression(Precedence.UNARY);

        return new Expr.Unary(right, operation);
    }

    private Expr.Binary Binary(Expr left, Token operation)
    {
        Precedence precedence = precedences[operation.Type];
        Expr right = ParseExpression(precedence);
        return new Expr.Binary(left, operation, right);
    }

    private Expr.Binary Equality(Expr left, Token operation)
    {
        Precedence precedence = precedences[operation.Type];
        Expr right = ParseExpression(precedence);
        return new Expr.Binary(left, operation, right);
    }

    private Expr.Binary Comparison(Expr left, Token operation)
    {
        Precedence precedence = precedences[operation.Type];
        Expr right = ParseExpression(precedence);
        return new Expr.Binary(left, operation, right);
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
        return Look().Type == type;
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

    private Token Consume(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return Previous();
        }
        throw new InvalidOperationException($"Expected token of type {type}, but got {Look().Type}.");
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