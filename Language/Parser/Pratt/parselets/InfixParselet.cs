using Binding = Precedence.Binding;

interface IInfixParselet
{
    Expr Parse(Pratt parser, Expr left, Token token);
    Binding GetPrecedence();
}

class PostfixExpression(Expr left, TokenType type) : Expr
{
    public Expr Left => left;
    public TokenType Type => type;

    public override T Accept<T>(IVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }
}

class PostfixOperatorParselet(Binding precedence) : IInfixParselet
{
    private readonly Binding Precedence = precedence;

    public Expr Parse(Pratt parser, Expr left, Token token)
    {
        return new PostfixExpression(left, token.Type);
    }

    public Binding GetPrecedence() => Precedence;
}