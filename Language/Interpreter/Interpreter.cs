class Interpreter : Expr.IVisitor<object>, Statement.IVisitor
{
    public static readonly InterpreterEnv Globals = new();
    private InterpreterEnv Environment = Globals;

    public void Interpret(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            Execute(statement);
        }
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

    public void ExecuteBlock(List<Statement> statements, InterpreterEnv environment)
    {
        InterpreterEnv previous = Environment;

        try
        {
            Environment = environment;

            foreach (var statement in statements) Execute(statement);
        }
        finally
        {
            Environment = previous;
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
        object value = Evaluate(expression.Value);
        Environment.Assign(expression.Name.Lexeme, value);
        return value; // log a = 2; assign can be nested inside other expressions
    }

    public void VisitExpression(Statement.Expression statement)
    {
        Evaluate(statement.expression);
    }

    public void VisitLog(Statement.Log statement)
    {
        object value = Evaluate(statement.expression);
        Console.WriteLine(Stringify(value));
    }

    public object VisitCall(Expr.Call expression)
    {
        object callee = Evaluate(expression.Callee); // Callee()()

        List<object> arguments = [];

        foreach (Expr argument in expression.Arguments)
            arguments.Add(Evaluate(argument)); // Ayman caught this bug :3 

        LangFunction function = (LangFunction)callee;

        if (arguments.Count != function.Arity())
        {
            Console.WriteLine("Expected " + function.Arity() + "arguments but got " + arguments.Count + ".");
        }

        return function.Call(this, arguments)!;
    }

    public void VisitFunction(Statement.Function statement)
    {
        LangFunction function = new(statement);
        Environment.Define(statement.Name.Lexeme, function);
    }

    public void VisitIf(Statement.If statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
            Execute(statement.ThenBranch);
        else if (statement.ElseBranch != null)
            Execute(statement.ElseBranch);
    }

    public void VisitWhile(Statement.While statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
            Execute(statement.Body);
    }

    public void VisitReturn(Statement.Return statement)
    {
        object? value = null;

        if (statement.Value != null) value = Evaluate(statement.Value);

        throw new Return(value!);
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        object? value = null;

        /* 
         * sends the expression back into the interpreter’s visitor implementation, 
         * nested evaluation, 
         * for example, `if var x = 5-4;, 5-4 will get evaluated to 1; 
        */

        if (variable.Initializer != null)
        {
            value = Evaluate(variable.Initializer);
        }

        Environment.Define(variable.Name.Lexeme, value!);
    }

    // smol hint: Evaluate(variable.Initializer) goes to this
    public object VisitVariableExpression(Expr.VariableExpression expression)
    {
        return Environment.Get(expression.Name.Lexeme);
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