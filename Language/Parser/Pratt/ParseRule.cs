using Precedence = Binding.Precedence;

class ParseRule(Func<Pratt, Expr>? prefix, Func<Pratt, Expr, Expr>? infix, Precedence precedence)
{
    public readonly Func<Pratt, Expr>? Prefix = prefix;
    public readonly Func<Pratt, Expr, Expr>? Infix = infix;
    public Precedence Precedence = precedence;
}

