using System.Text;

class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    public string VisitBinary(Expr.Binary expr)
    {
        return Parenthesize(expr.Operation.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGrouping(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteral(Expr.Literal expr)
    {
        if (expr.Value == null) return "nil";
        return expr.Value.ToString()!;
    }

    public string VisitUnary(Expr.Unary expr)
    {
        return Parenthesize(expr.Operation.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);

        foreach (Expr expr in exprs)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }

        builder.Append(')');

        return builder.ToString();
    }
}