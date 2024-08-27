class TypeChecker : Expr.IVisitor<object>, Statement.IVisitor
{
    public static readonly TypeEnvironment Globals = new();
    private TypeEnvironment Environment = Globals;
    public void TypeCheck(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }
    }
    public object? VisitAssign(Expr.Assign expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitBinary(Expr.Binary expression)
    {
        throw new NotImplementedException();
    }

    public void VisitBlock(Statement.Block Statement)
    {
        foreach (var statement in Statement.Statements)
        {
            statement.Accept(this);
        }
    }

    public object? VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
    }

    public void VisitExpression(Statement.Expression Statement)
    {
        throw new NotImplementedException();
    }

    public void VisitFunction(Statement.Function Statement)
    {
        throw new NotImplementedException();
    }

    public object? VisitGrouping(Expr.Grouping expression)
    {
        throw new NotImplementedException();
    }

    public void VisitIf(Statement.If Statement)
    {
        throw new NotImplementedException();
    }

    public object? VisitLiteral(Expr.Literal expression)
    {
        if (expression.Value is double) return new Language.TypeChecker.Number();
        else if (expression.Value is bool) return new Language.TypeChecker.Boolean();
        else throw new Exception("Unsupported literal type.");
    }

    public void VisitLog(Statement.Log Statement)
    {

    }

    public object? VisitLogical(Expr.Logical expression)
    {
        throw new NotImplementedException();
    }

    public void VisitReturn(Statement.Return Statement)
    {
        throw new NotImplementedException();
    }

    public object? VisitUnary(Expr.Unary expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        return Environment.Get(expression.Name.Lexeme);
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        Language.TypeChecker.Type? type = null;

        if (statement.Initializer != null)
        {
            type = (Language.TypeChecker.Type?)statement.Initializer.Accept(this);
        }

        Environment.Define(statement.Name.Lexeme, type!);
    }

    public void VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }
}