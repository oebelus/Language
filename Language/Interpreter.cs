class Interpreter : Expr.IVisitor<object>
{
    public void Interpret(Expr expression)
    {
        Console.WriteLine(Stringify(Evaluate(expression)));
    }

    public object VisitBinary(Expr.Binary binary)
    {
        object left = Evaluate(binary.Left);
        object right = Evaluate(binary.Right);

        switch (binary.Operation.Type)
        {
            case TokenType.MINUS:
                return (float)left - (float)right;
            case TokenType.PLUS:
                if (left is float a && right is float b) return a + b;
                if (left is string c && right is string d) return c + d;
                break;
            case TokenType.STAR:
                return (float)left * (float)right;
            case TokenType.SLASH:
                return (float)left / (float)right;
            case TokenType.GREATER:
                return (float)left > (float)right;
            case TokenType.GREATER_EQUAL:
                return (float)left >= (float)right;
            case TokenType.LESS:
                return (float)left < (float)right;
            case TokenType.LESS_EQUAL:
                return (float)left <= (float)right;
            case TokenType.BANG_EQUAL:
                return left != right;
            case TokenType.EQUAL_EQUAL:
                return left == right;
        }
        return "null";
    }

    public object VisitUnary(Expr.Unary unary)
    {
        object right = Evaluate(unary.Right);

        switch (unary.Operation.Type)
        {
            case TokenType.MINUS:
                return -(float)right;
            case TokenType.BANG:
                return !IsTruthy(right);
        }
        return "null";
    }

    public object VisitLiteral(Expr.Literal literal)
    {
        return literal.Value!;
    }

    public object VisitGrouping(Expr.Grouping grouping)
    {
        return Evaluate(grouping.Expression);
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private static bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }

    private static string Stringify(object obj)
    {
        if (obj == null) return "nil";

        if (obj is float)
        {
            string text = obj.ToString()!;
            if (text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
            return text;
        }

        return obj.ToString()!;
    }
}