class Rpn : Expr.IVisitor<string>
{
    private Stack<char> Stack = [];
    private List<string> Output = [];

    public string Print()
    {
        return string.Join(" ", Output);
    }
    public string VisitBinary(Expr.Binary expr)
    {
        return "a";
    }

    public string VisitUnary(Expr.Unary expr)
    {
        return "a";
    }

    public string VisitLiteral(Expr.Literal expr)
    {
        return "a";
    }
    string Expr.IVisitor<string>.VisitGrouping(Expr.Grouping expression)
    {
        throw new FormatException($"Bad character encountered: {expression}");
    }
}