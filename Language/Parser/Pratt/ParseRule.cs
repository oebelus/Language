using Binding = Precedence.Binding;

class ParseRule(Delegate? prefix, Delegate? infix, Binding precedence)
{
    public readonly Delegate? Prefix = prefix;
    public readonly Delegate? Infix = infix;
    public Binding Precedence = precedence;
}

