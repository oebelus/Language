class TypeChecker : Expr.IVisitor<object>, Statement.IVisitor
{
    public void TypeCheck(List<Statement> statements) {
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public void VisitLog(Statement.Log Statement)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        throw new NotImplementedException();
    }

    public void VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }
}