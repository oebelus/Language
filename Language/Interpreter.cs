class Interpreter : Expr.IVisitor<object>, Statement.IVisitor<Action>
{
    private readonly Environment environment = new();

    public void Interpret(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            Execute(statement);
        }
    }

    private void Execute(Statement statement) {
        statement.Accept(this);
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

        return unary.Operation.Type switch
        {
            TokenType.MINUS => -(float)right,
            TokenType.BANG => !IsTruthy(right),
            _ => "null",
        };

    }

    public object VisitLiteral(Expr.Literal literal)
    {
        return literal.Value!;
    }

    public object VisitGrouping(Expr.Grouping grouping)
    {
        return Evaluate(grouping.Expression);
    }

    public object VisitAssign(Expr.Assign expression)
    {
        throw new NotImplementedException();
    }

    public object VisitVariable(Statement.Variable statement)
    {
        object value = null!;
        if ()
    }

    public Action VisitExpression(Statement.Expression statement) {
        Evaluate(statement.expression);
        return null!;
    }

    public Action VisitLog(Statement.Log statement) {
        object value = Evaluate(statement.expression);
        Console.WriteLine(Stringify(value));
        return null!;
    }

    public Action VisitBlock(Statement.Block statement) {
        return null!;
    }

    public Action VisitFunction(Statement.Function statement) {
        return null!;
    }

    public Action VisitIf(Statement.If statement) {
        return null!;
    }

    public Action VisitWhile(Statement.While statement) {
        return null!;
    }

    public Action VisitReturn(Statement.Return statement) {
        return null!;
    }

    public Action VisitVar(Statement.Var statement) {
        return null!;
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