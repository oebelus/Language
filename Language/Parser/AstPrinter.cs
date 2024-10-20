using System.Linq.Expressions;
using System.Text;

class AstPrinter : Expr.IVisitor<string>, Statement.IVisitor
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public void PrintStatement(Statement statement)
    {
        statement.Accept(this);
    }

    public string VisitAssign(Expr.Assign expression)
    {
        return Parenthesize(expression.Name.Lexeme, expression.Value);
    }


    public string VisitBinary(Expr.Binary expr)
    {
        return Parenthesize(expr.Operation.Lexeme, expr.Left, expr.Right);
    }

    public void VisitBlock(Statement.Block Statement)
    {
        throw new NotImplementedException();
    }

    public string VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
    }

    public void VisitExpression(Statement.Expression Statement)
    {
        Console.WriteLine(Parenthesize("expression", Statement.expression));
    }

    public void VisitFunction(Statement.Function Statement)
    {
        Console.WriteLine(ParenthesizeFunction($"function {Statement.Name}", Statement.Args));

        foreach (Statement statement in Statement.Body)
        {
            statement.Accept(this);
        }
    }

    public string VisitGrouping(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public void VisitIf(Statement.If Statement)
    {
        throw new NotImplementedException();
    }

    public string VisitLiteral(Expr.Literal expr)
    {
        if (expr.Value == null) return "nil";
        return expr.Value.ToString()!;
    }

    public void VisitLog(Statement.Log Statement)
    {
        Console.WriteLine(Parenthesize("out", Statement.expression));
    }

    public string VisitLogical(Expr.Logical expression)
    {
        throw new NotImplementedException();
    }

    public void VisitReturn(Statement.Return Statement)
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
        return Parenthesize(expression.Name.Lexeme);
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        if (statement.Initializer != null)
            Console.WriteLine(Parenthesize($"{statement.Type?.ToString()} {statement.Name.Lexeme}", statement.Initializer));
        else
            Console.WriteLine(Parenthesize(statement.Name.Lexeme));
    }

    public void VisitWhile(Statement.While Statement)
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

    private string ParenthesizeFunction(string name, List<Statement.Argument> args)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);

        foreach (Statement.Argument argument in args)
        {
            builder.Append(' ');
            builder.Append(argument.Type.ToString());
            builder.Append(' ');
            builder.Append(argument.Name.Lexeme);
        }

        builder.Append(')');

        return builder.ToString();
    }
}