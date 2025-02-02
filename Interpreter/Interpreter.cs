class Interpreter : Expr.IVisitor<object>, Statement.IVisitor
{
    public static readonly InterpreterEnv Globals = new();
    public InterpreterEnv Environment = Globals;
    public object LastResult { get; private set; } = "";
    private readonly Dictionary<string, int> Locals = [];
    private readonly Stack<Dictionary<string, object>> scopes = new(1024);

    public void Interpret(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            if (statement is Statement.Expression expressionStatement)
            {
                LastResult = Evaluate(expressionStatement.expression);
                continue;
            }
            Execute(statement);
        }
    }

    public void Reset()
    {
        Environment.Clear();
        Globals.Clear();
        Locals.Clear();
        LastResult = "";
        scopes.Clear();
    }

    public List<object> InterpretExpressions(List<Expr> expressions)
    {
        List<object> results = [];

        foreach (var expression in expressions)
        {
            object result = Evaluate(expression);
            results.Add(result);
            Console.WriteLine(Stringify(result));
        }
        return results;
    }

    private void Execute(Statement statement)
    {
        statement.Accept(this);
    }

    public void Resolve(Token name, int depth)
    {
        if (Locals.TryGetValue(name.Lexeme, out int local))
        {
            Environment.AssignAt(depth, name.Lexeme, local);
        }
        else
        {
            Environment.Assign(name, depth);
        }
    }

    public void ExecuteBlock(List<Statement> statements, InterpreterEnv environment)
    {
        BeginScope();
        InterpreterEnv previous = Environment;

        try
        {
            Environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            Environment = previous;

            EndScope();
        }
    }

    public void VisitBlock(Statement.Block statement)
    {
        ExecuteBlock(statement.Statements, new InterpreterEnv(Environment));
    }


    public object VisitBinary(Expr.Binary binary)
    {
        object left = Evaluate(binary.Left);
        object right = Evaluate(binary.Right);

        if (left is string leftString && float.TryParse(leftString, out float leftFloat))
        {
            left = leftFloat;
        }
        if (right is string rightString && float.TryParse(rightString, out float rightFloat))
        {
            right = rightFloat;
        }

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
                return left.Equals(right);
            case TokenType.MOD:
                return (float)left % (float)right;
            default:
                break;
        }
        return "null";
    }

    public object VisitLogical(Expr.Logical expression)
    {
        object left = Evaluate(expression.Left);

        if (expression.Operation.Type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expression.Right);
    }

    public object VisitUnary(Expr.Unary unary)
    {
        object right = Evaluate(unary.Right);

        if (right is string rightString && float.TryParse(rightString, out float rightFloat))
        {
            right = rightFloat;
        }

        return unary.Operation.Type switch
        {
            TokenType.MINUS => -(float)right,
            TokenType.BANG => !IsTruthy(right),
            _ => "null",
        };
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        return literal.Value;
    }

    public object VisitGrouping(Expr.Grouping grouping)
    {
        return Evaluate(grouping.Expression);
    }

    public object VisitAssign(Expr.Assign expression)
    {
        CheckExpressionExistence(expression.Name);

        object value = Evaluate(expression.Value);

        if (scopes.Count() == 0) Environment.Assign(expression.Name, value);

        else
        {
            if (scopes.Peek().ContainsKey(expression.Name.Lexeme))
                scopes.Peek()[expression.Name.Lexeme] = value;
            else
                Environment.Assign(expression.Name, value);
        }

        return value;
    }

    public void VisitExpression(Statement.Expression statement)
    {
        Evaluate(statement.expression);
    }

    public void VisitLog(Statement.Log statement)
    {
        object? value;

        if (statement.expression != null)
        {
            value = Evaluate(statement.expression);

            if (statement.Keyword == "println")
                Console.WriteLine(Stringify(value));
            else
                Console.Write(Stringify(value));
        }

        else
        {
            if (statement.Keyword == "println")
                Console.WriteLine();
            else
                throw new Exception("Expected an expression.");
        }
    }

    public object VisitCall(Expr.Call expression)
    {
        BeginScope();

        object callee = Evaluate(expression.Callee); // Callee()()

        List<object> arguments = [];

        foreach (Expr argument in expression.Arguments)
            arguments.Add(Evaluate(argument)); // Ayman caught this bug :3 

        LangFunction function = (LangFunction)callee;

        if (arguments.Count != function.Arity())
        {
            Console.WriteLine($"Expected {function.Arity()} argument(s) but got {arguments.Count} in function '{expression.Callee.Name.Lexeme}'.");
        }

        var result = function.Call(this, arguments)!;

        EndScope();

        return result;
    }

    public void VisitFunction(Statement.Function statement)
    {
        LangFunction function = new(statement);
        Environment.Define(statement.Name.Lexeme, function);
    }

    public void VisitIf(Statement.If statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.ThenBranch);
        }
        else if (statement.ElseBranch != null)
        {
            Execute(statement.ElseBranch);
        }
    }

    public void VisitWhile(Statement.While statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
            try
            {
                Execute(statement.Body);
            }
            catch (Break)
            {
                return;
            }
            catch (Continue)
            {
                continue;
            }
    }

    public void VisitReturn(Statement.Return statement)
    {
        object? value = null;

        if (statement.Value != null) value = Evaluate(statement.Value);

        throw new Return(value!);
    }

    public void VisitBreak(Statement.Break statement)
    {
        throw new Break();
    }

    public void VisitContinue(Statement.Continue statement)
    {
        throw new Continue();
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        if (scopes.head > 0)
            StatementExistence(variable.Name);

        if (variable.Initializer != null)
        {
            object value = Evaluate(variable.Initializer);

            Environment.Define(variable.Name.Lexeme, value);

            if (scopes.Count() > 0)
            {
                Resolve(variable.Name, scopes.Pook().Count);

                scopes.Peek()[variable.Name.Lexeme] = value;
            }
        }
        else
        {
            Environment.Define(variable.Name.Lexeme, "");
        }
    }

    public object VisitVariableExpression(Expr.VariableExpression expression)
    {
        return LookUpVariable(expression.Name);
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

        if (obj is string str)
        {
            return str.Replace("\"", "");
        }

        return obj.ToString()!;
    }

    private object LookUpVariable(Token name)
    {
        if (scopes.head > 0)
        {
            for (int i = scopes.head - 1; i >= 0; i--)
            {
                if (scopes.ElementAt(i) != null)
                    if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                        return scopes.ElementAt(i)[name.Lexeme];
            }
        }

        return Environment.Get(name);
    }

    private void StatementExistence(Token name)
    {
        if (scopes.ElementAt(scopes.head) != null)
            if (scopes.ElementAt(scopes.head).ContainsKey(name.Lexeme))
                throw new Exception($"Variable '{name.Lexeme}' is already defined in this scope.");

        for (int i = scopes.head - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i) != null)
                if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                    throw new Exception($"A local parameter named '{name.Lexeme}' is used in an enclosing local scope to define a local.");
        }

        return;
    }

    private void CheckExpressionExistence(Token name)
    {
        if (Environment.Get(name) != null) return;

        else if (scopes.head > 0)
        {
            for (int i = scopes.head; i >= 0; i--)
            {
                if (scopes.ElementAt(i) != null)
                    if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                        return;
            }
        }
    }

    private void BeginScope()
    {
        scopes.Push([]);
    }

    private void EndScope()
    {
        scopes.Pop();
    }
}