class Resolver(Interpreter interpreter) : Expr.IVisitor<object>, Statement.IVisitor
{
    private readonly Interpreter interpreter = interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new(1024);
    private ScopeType currentScope = ScopeType.NONE;

    public void Resolve(List<Statement> statements)
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
        Resolve(expression.Left);
        Resolve(expression.Right);

        return null;
    }

    public void VisitBlock(Statement.Block Statement)
    {
        BeginScope();
        Resolve(Statement.Statements);
        EndScope();
    }

    public void VisitBreak(Statement.Break statement)
    {
        if (currentScope != ScopeType.WHILE)
        {
            throw new Exception("Can't break outside of a loop.");
        }
    }

    public object? VisitCall(Expr.Call expression)
    {
        Resolve(expression.Callee);

        foreach (Expr argument in expression.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public void VisitContinue(Statement.Continue statement)
    {
        if (currentScope != ScopeType.WHILE)
        {
            throw new Exception("Can't continue outside of a loop.");
        }
    }

    public void VisitExpression(Statement.Expression statement)
    {
        Resolve(statement.expression);
    }

    public void VisitFunction(Statement.Function function)
    {
        Declare(function.Name);
        Define(function.Name);

        ScopeType enclosingFunction = currentScope;
        currentScope = ScopeType.FUNCTION;

        BeginScope();

        foreach (Statement.Argument arg in function.Args)
        {
            Declare(arg.Name);
            Define(arg.Name);
        }

        EndScope();

        currentScope = enclosingFunction;
    }

    public object? VisitGrouping(Expr.Grouping expression)
    {
        Resolve(expression.Expression);

        return null;
    }

    public void VisitIf(Statement.If statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.ThenBranch);

        if (statement.ElseBranch != null) Resolve(statement.ElseBranch);
    }

    public object? VisitLiteral(Expr.Literal expression)
    {
        return null;
    }

    public void VisitLog(Statement.Log statement)
    {
        if (statement.expression != null) Resolve(statement.expression);
    }

    public object? VisitLogical(Expr.Logical expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);

        return null;
    }

    public void VisitReturn(Statement.Return Statement)
    {
        if (currentScope == ScopeType.NONE)
        {
            throw new Exception("Can't return from top-level code.");
        }

        if (Statement.Value != null) Resolve(Statement.Value);
    }

    public object? VisitUnary(Expr.Unary expression)
    {
        Resolve(expression.Right);

        return null;
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        if (!scopes.IsEmpty() && scopes.Peek()[expression.Name.Lexeme] == false)
        {
            throw new Exception("Can't read local variable in its own initializer."); // Declared but not yet defined
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
        Resolve(Statement.Condition);

        ScopeType enclosingWhile = currentScope;
        currentScope = ScopeType.WHILE;

        Resolve(Statement.Body);

        currentScope = enclosingWhile;
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

        if (scope.ContainsKey(token.Lexeme))
        {
            throw new Exception("Variable with this name already declared in this scope.");
        }

        scope.Add(token.Lexeme, false);
    }

    private void Define(Token token)
    {
        if (scopes.IsEmpty()) return;

        scopes.Peek()[token.Lexeme] = true;
    }

    private void ResolveLocal(Expr expression, Token name)
    {
        for (int i = scopes.head - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(name.Lexeme, scopes.Length() - 1 - i);
                return;
            }
        }
    }

    private enum ScopeType
    {
        NONE,
        FUNCTION,
        WHILE
    }
}