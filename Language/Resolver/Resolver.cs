using Language.Typer;

class Resolver(Interpreter interpreter) : Expr.IVisitor<object>, Statement.IVisitor
{
    private readonly Interpreter interpreter = interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new(1024);

    private void Resolve(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        Resolve(expression.Value);
        ResolveLocal(expression, expression.Name);

        return null;
    }

    public object? VisitBinary(Expr.Binary expression)
    {
        throw new NotImplementedException();
    }

    public void VisitBlock(Statement.Block Statement)
    {
        BeginScope();
        Resolve(Statement.Statements);
        EndScope();
    }

    public void VisitBreak(Statement.Break statement)
    {
        throw new NotImplementedException();
    }

    public object? VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
    }

    public void VisitContinue(Statement.Continue statement)
    {
        throw new NotImplementedException();
    }

    public void VisitExpression(Statement.Expression statement)
    {
        Resolve(statement.expression);
    }

    public void VisitFunction(Statement.Function function)
    {
        Declare(function.Name);
        Define(function.Name);

        BeginScope();

        foreach (Statement.Argument arg in function.Args)
        {
            Declare(arg.Name);
            Define(arg.Name);
        }

        Resolve(function.Body);

        EndScope();
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
        if (!scopes.IsEmpty() && scopes.Peek()[expression.Name.Lexeme] == false)
        {
            // Declared but not yet defined
            throw new Exception("Can't read local variable in its own initializer.");
        }

        ResolveLocal(expression, expression.Name);

        return null;
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        Declare(statement.Name);

        if (statement.Initializer != null) Resolve(statement.Initializer);

        Define(statement.Name);
    }

    public void VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }

    private void Resolve(Statement statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expr expression)
    {
        expression.Accept(this);
    }

    private void BeginScope()
    {
        scopes.Push([]);
    }

    private void EndScope()
    {
        scopes.Pop();
    }

    private void Declare(Token token)
    {
        if (scopes.IsEmpty()) return;

        Dictionary<string, bool> scope = scopes.Peek();

        scope.Add(token.Lexeme, false);
    }

    private void Define(Token token)
    {
        if (scopes.IsEmpty()) return;

        scopes.Peek().Add(token.Lexeme, true);
    }

    private void ResolveLocal(Expr expression, Token name)
    {
        for (int i = scopes.Length() - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expression, scopes.Length() - 1 - i);
                return;
            }
        }
    }
}