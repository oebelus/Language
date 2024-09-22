using Binding = Precedence.Binding;

class ParseRule(Func<Pratt, Expr>? prefix, Func<Pratt, Expr, Expr>? infix, Binding precedence)
{
    public readonly Func<Pratt, Expr>? Prefix = prefix;
    public readonly Func<Pratt, Expr, Expr>? Infix = infix;
    public Binding Precedence = precedence;
}

