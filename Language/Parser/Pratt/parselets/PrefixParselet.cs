using Binding = Precedence.Binding;

interface IPrefixParselet
{
    Expr Parse(Pratt parser, Token token);
}

class PrefixOperatorParselet(Binding precedence) : IPrefixParselet
{
    private readonly Binding Precedence = precedence;

    public Expr Parse(Pratt parser, Token token)
    {
        Expr right = parser.ParseExpression(Precedence);

        return new Expr.Unary(right, token);
    }

    public Binding GetPrecedence() => Precedence;
}

class NameParselet(Binding precedence) : IPrefixParselet
{
    private readonly Binding Precedence = precedence;

    public Expr Parse(Pratt parser, Token token)
    {
        throw new NotImplementedException();
    }
}
