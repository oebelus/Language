
using Binding = Precedence.Binding;

class PParser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    private int current = 0;

    public void Parse()
    {
        Advance();
        Expression();
        Consume(TokenType.EOF);
    }

    private void Expression()
    {
        ParsePrecedence(Binding.ASSIGNMENT);
    }

    private void Number()
    {
        ParsePrecedence(Binding.PRIMARY);
    }

    private void Grouping()
    {

    }

    private Expr.Unary Unary()
    {
        Token operation = Previous();
        Expr right = ParsePrecedence(Binding.UNARY);

        return new Expr.Unary(right, operation);
    }

    private Expr.Binary Binary(Expr left)
    {
        Token operation = Previous();
        TokenType operatorType = operation.Type;
        ParseRule rule = GetRule(operatorType);
        Expr right = ParsePrecedence((Binding)(rule.precedence + 1));

        return new Expr.Binary(left, operation, right);
    }

    private Expr ParsePrecedence(Binding binding)
    {
        throw new NotImplementedException();
    }

    private ParseRule GetRule(TokenType type)
    {

        throw new NotImplementedException();
    }

    // NUD handler (prefix and unary)

    // LED handler (infix and binary)

    // Postfix handler

    private void Consume(TokenType type)
    {
        if (Check(type)) Advance();

        throw new InvalidOperationException($"Expected token of type {type}, but got {Peek().Type}.");
    }

    private Token Peek()
    {
        return Tokens[current];
    }

    private void Advance()
    {
        if (!IsAtEnd()) current++;
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

    private Token Previous()
    {
        return Tokens[current - 1];
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }
}