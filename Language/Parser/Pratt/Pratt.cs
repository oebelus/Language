using Precedence = Binding.Precedence;
using Number = Language.Typer.Number;
using Void = Language.Typer.Void;
using Boolean = Language.Typer.Boolean;
using String = Language.Typer.String;
using Type = Language.Typer.Type;
using System.Data;
using System.Text.RegularExpressions;

class Pratt
{
    private List<Token>? Tokens;
    private int current = 0;
    private readonly Dictionary<TokenType, Precedence> precedences;
    public TokenType[] comparison = [TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL];

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
            [TokenType.CLASS] = Precedence.NONE,
            [TokenType.ELSE] = Precedence.CONDITIONAL,
            [TokenType.FALSE] = Precedence.NONE,
            [TokenType.FOR] = Precedence.CONDITIONAL,
            [TokenType.FUN] = Precedence.DECLARATION,
            [TokenType.IF] = Precedence.CONDITIONAL,
            [TokenType.NIL] = Precedence.NONE,
            [TokenType.OR] = Precedence.OR,
            [TokenType.LOG] = Precedence.STATEMENT,
            [TokenType.RETURN] = Precedence.STATEMENT,
            [TokenType.SUPER] = Precedence.NONE,
            [TokenType.THIS] = Precedence.NONE,
            [TokenType.TRUE] = Precedence.NONE,
            [TokenType.VAR] = Precedence.DECLARATION,
            [TokenType.WHILE] = Precedence.CONDITIONAL,
            [TokenType.EOF] = Precedence.NONE,
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
        if (Match(TokenType.WHILE)) return While();
        if (Match(TokenType.FOR)) return For();
        if (Match(TokenType.IF)) return If();
        if (Match(TokenType.LOG)) return Log();

        return new Statement.Expression(ParseExpression(0));
    }

    private Statement.Log Log()
    {
        Expr expr = ParseExpression(0);
        Consume(TokenType.SEMICOLON);
        return new Statement.Log(expr);
    }

    private Statement.If If()
    {
        Expr condition = ParseExpression(Precedence.CONDITIONAL);

        Statement thenBranch = ParseStatement();

        Statement? elseBranch = Match(TokenType.ELSE) ? ParseStatement() : null;

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
        else if (Match(TokenType.VAR))
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
            increment = ParseExpression(0);
        }

        Consume(TokenType.RIGHT_PAREN);

        Statement body = ParseStatement();

        if (increment != null)
        {
            body = new Statement.Block([body, new Statement.Expression(increment)]);
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
        Expr left = ParseAtom(); // NUD

        while (precedences[Look().Type] != Precedence.STATEMENT && precedences[Look().Type] > precedence && !Check(TokenType.SEMICOLON))
        {
            left = ParseInfix(left); // LED
        }

        return left;
    }

    private Expr ParseAtom()
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(new Boolean(), true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(new Boolean(), false);
        if (Match(TokenType.NIL)) return new Expr.Literal(new Void(), null);

        if (Match(TokenType.NUMBER)) return new Expr.Literal(new Number(), Previous().Type == TokenType.NUMBER ? Previous().Lexeme : Look().Lexeme);
        if (Match(TokenType.STRING)) return new Expr.Literal(new String(), Previous().Lexeme);

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
                Consume(TokenType.SEMICOLON);
                return new Expr.Assign(name, value);
            }
            return new Expr.VariableExpression(Previous());
        }

        throw new Exception();
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
            "num" => new Number(),
            "bool" => new Boolean(),
            _ => new Void(),
        };
    }
}