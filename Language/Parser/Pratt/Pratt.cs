using Precedence = Binding.Precedence;
using Number = Language.Typer.Number;
using Void = Language.Typer.Void;
using Boolean = Language.Typer.Boolean;
using String = Language.Typer.String;
using Type = Language.Typer.Type;
using System.Data;

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
            [TokenType.ELSE] = Precedence.NONE,
            [TokenType.FALSE] = Precedence.NONE,
            [TokenType.FOR] = Precedence.NONE,
            [TokenType.FUN] = Precedence.NONE,
            [TokenType.IF] = Precedence.NONE,
            [TokenType.NIL] = Precedence.NONE,
            [TokenType.OR] = Precedence.OR,
            [TokenType.LOG] = Precedence.NONE,
            [TokenType.RETURN] = Precedence.NONE,
            [TokenType.SUPER] = Precedence.NONE,
            [TokenType.THIS] = Precedence.NONE,
            [TokenType.TRUE] = Precedence.NONE,
            [TokenType.VAR] = Precedence.NONE,
            [TokenType.WHILE] = Precedence.NONE,
            [TokenType.EOF] = Precedence.NONE,
        };
    }

    public List<Expr> Parse()
    {
        List<Expr> statements = [];
        while (!IsAtEnd()) statements.Add(ParseExpression(0));
        return statements;
    }


    public Expr ParseExpression(Precedence precedence)
    {
        Expr left = ParseAtom(); // NUD

        while (precedences[Look().Type] > precedence)
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

        if (Match(TokenType.IDENTIFIER)) return new Expr.VariableExpression(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            return Grouping();
        }

        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            return Unary();
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
        else
        {
            throw new SyntaxErrorException();
        }
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

    private Expr Equality(Expr left, Token operation)
    {
        Precedence precedence = precedences[operation.Type];
        Expr right = ParseExpression(precedence);
        return new Expr.Binary(left, operation, right);
    }

    private Expr Comparison(Expr left, Token operation)
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

    private void Consume(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return;
        }
        throw new InvalidOperationException($"Expected token of type {type}, but got {Peek().Type}.");
    }

}