using Precedence = Binding.Precedence;
using Number = Language.Typer.Number;
using Void = Language.Typer.Void;
using Boolean = Language.Typer.Boolean;
using String = Language.Typer.String;
using System.Linq.Expressions;
using System.Security.AccessControl;
using System.Data;

class Pratt
{
    private List<Token>? Tokens;
    private int current = 0;
    private readonly Dictionary<TokenType, Precedence> precedences;
    public TokenType[] binary = [TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.MOD];
    public TokenType[] unary = [TokenType.BANG, TokenType.MINUS];

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
            [TokenType.BANG] = Precedence.NONE,
            [TokenType.BANG_EQUAL] = Precedence.NONE,
            [TokenType.EQUAL] = Precedence.NONE,
            [TokenType.EQUAL_EQUAL] = Precedence.NONE,
            [TokenType.GREATER] = Precedence.NONE,
            [TokenType.GREATER_EQUAL] = Precedence.NONE,
            [TokenType.LESS] = Precedence.NONE,
            [TokenType.LESS_EQUAL] = Precedence.NONE,
            [TokenType.IDENTIFIER] = Precedence.NONE,
            [TokenType.STRING] = Precedence.NONE,
            [TokenType.NUMBER] = Precedence.NONE,
            [TokenType.AND] = Precedence.NONE,
            [TokenType.CLASS] = Precedence.NONE,
            [TokenType.ELSE] = Precedence.NONE,
            [TokenType.FALSE] = Precedence.NONE,
            [TokenType.FOR] = Precedence.NONE,
            [TokenType.FUN] = Precedence.NONE,
            [TokenType.IF] = Precedence.NONE,
            [TokenType.NIL] = Precedence.NONE,
            [TokenType.OR] = Precedence.NONE,
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
        
        while (precedences[Look().Type] > precedence) {
            left = ParseInfix(left); // LED
        }
        return left; 
    }

    private Expr ParseAtom() 
    {
        if (Match(TokenType.TRUE)) return new Expr.Literal(new Boolean(), true);
        if (Match(TokenType.FALSE)) return new Expr.Literal(new Boolean(), false);
        if (Match(TokenType.NIL)) return new Expr.Literal(new Void(), null);

        if (Match(TokenType.NUMBER)) return new Expr.Literal(new Number(), Previous().Lexeme);
        if (Match(TokenType.STRING)) return new Expr.Literal(new String(), Previous().Lexeme);

        if (Match(TokenType.IDENTIFIER)) return new Expr.VariableExpression(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = ParseExpression(precedences[TokenType.LEFT_PAREN]);
            Consume(TokenType.RIGHT_PAREN);
            return new Expr.Grouping(expr);
        }

        if (Match(TokenType.BANG, TokenType.MINUS)) {
            Expr right = ParseExpression(precedences[Advance().Type]);
            return new Expr.Unary(right, Look());
        }

        throw new Exception();
    }

    private Expr ParseInfix(Expr expression) {
        Token token = Look();

        if (binary.Contains(token.Type)) {
            Precedence precedence = precedences[token.Type];
            Advance();
            Expr right = ParseExpression(precedence);
            return new Expr.Binary(expression, token, right);
        } else if (unary.Contains(token.Type)) {
            Advance();
            return new Expr.Unary(expression, token);
        } else {
            throw new SyntaxErrorException();
        }
    }

    private Expr.Literal Number(Pratt _)
    {
        return new Expr.Literal(new Number(), Previous().Lexeme);
    }

    private Expr.Grouping Grouping(Pratt _)
    {
        // Consume(TokenType.LEFT_PAREN);
        Expr expression = ParseExpression(precedences[Advance().Type]);
        Consume(TokenType.RIGHT_PAREN);
        return new Expr.Grouping(expression);
    }

    private Expr.Unary Unary(Pratt _)
    {
        Token operation = Previous();
        Expr right = ParseExpression(Precedence.UNARY);

        return new Expr.Unary(right, operation);
    }

    private Expr.Binary Binary(Pratt _, Expr left)
    {
        Token operation = Previous();
        Precedence precedence = GetRule(operation.Type);
        Expr right = ParseExpression(precedence + 1);

        return new Expr.Binary(left, operation, right);
    }

    private Precedence GetRule(TokenType type)
    {
        return precedences[type];
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
        }
        throw new InvalidOperationException($"Expected token of type {type}, but got {Peek().Type}.");
    }

}