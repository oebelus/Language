using Binding = Precedence.Binding;

class ParseRule
{
    Delegate prefix;
    Delegate infix;
    public Binding precedence;
}