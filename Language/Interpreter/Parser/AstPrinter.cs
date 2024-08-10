using System.Linq.Expressions;
using System.Text;

class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitAssign(Expr.Assign expression)
    {
        throw new NotImplementedException();
    }


    public string VisitBinary(Expr.Binary expr)
    {
        return Parenthesize(expr.Operation.Lexeme, expr.Left, expr.Right);
    }

    public string VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
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

    public string VisitLogical(Expr.Logical expression)
    {
        throw new NotImplementedException();
    }


    public string VisitUnary(Expr.Unary expr)
    {
        return Parenthesize(expr.Operation.Lexeme, expr.Right);
    }

    public string VisitVariable(Expr.VariableExpression variable)
    {
        throw new NotImplementedException();
    }

    public string VisitVariableExpression(Expr.VariableExpression expression)
    {
        throw new NotImplementedException();
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