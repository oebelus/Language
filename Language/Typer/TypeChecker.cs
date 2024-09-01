using Type = Language.Typer.Type;
using Boolean = Language.Typer.Boolean;
using Void = Language.Typer.Void;
using Function = Language.Typer.Function;
using Language.Typer;

class TypeChecker : Expr.IVisitor<Type>, Statement.IVisitor
{
    public static readonly TypeEnvironment Globals = new();
    private TypeEnvironment Environment = Globals;
    private readonly List<TokenType> booleanOps = [TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL, TokenType.GREATER, TokenType.GREATER_EQUAL];

    private bool hasReturn = false;
    private Type returnType = new Void();

    public void TypeCheck(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }
    }
    public Type? VisitAssign(Expr.Assign expression)
    {
        Type? type = Environment.Get(expression.Name.Lexeme);
        Type? valueType = expression.Value.Accept(this);

        // Compare the type of the assigned value to the type of the variable
        if (valueType != type)
        {
            throw new Exception($"Wrong type for variable: {expression.Name.Lexeme} ({type}) and ({valueType})");
        }
        else
        {
            Environment.Assign(expression.Name.Lexeme, valueType);
        }

        return valueType;
    }

    public Type? VisitBinary(Expr.Binary binaryExpression)
    {
        Type rightType = binaryExpression.Right.Accept(this);
        Type leftType = binaryExpression.Left.Accept(this);

        if (binaryExpression.Operation.Type == TokenType.PLUS)
        {
            if (rightType != leftType)
            {
                throw new Exception($"Right ({rightType}) and Left ({leftType}) should be of the same type");
            }

            if (rightType is Boolean && leftType is Boolean)
            {
                throw new Exception($"Boolean cannot be concatenated");
            }
        }

        else if (booleanOps.Contains(binaryExpression.Operation.Type))
        {
            return new Boolean();
        }

        else
        {
            if (rightType is not Number || leftType is not Number)
            {
                throw new Exception($"Right ({rightType}) and Left ({leftType}) should be of type Number");
            }

            return rightType;
        }

        return rightType;
    }

    public void VisitBlock(Statement.Block Statement)
    {
        TypeCheckBlock(Statement.Statements, new TypeEnvironment(Environment));
    }

    public void TypeCheckBlock(List<Statement> statements, TypeEnvironment environment)
    {
        TypeEnvironment previous = Environment;

        try
        {
            Environment = environment;

            foreach (var statement in statements) statement.Accept(this);
        }
        finally
        {
            Environment = previous;
        }
    }

    public Type? VisitCall(Expr.Call call)
    {
        Type calleeType = call.Callee.Accept(this);

        List<Type> calleeArguments = [];

        foreach (Expr argument in call.Arguments)
        {
            calleeArguments.Add(argument.Accept(this));
        }

        Function function = new([.. calleeArguments], calleeType);

        // Check if the callee is a function
        if (function is not Function functionType)
        {
            throw new Exception($"Can only call functions, instead got \"{function}\"");
        }

        // Check for number of arguments
        if (call.Arguments.Count != functionType.Arguments.Length)
        {
            throw new Exception($"Expected {functionType.Arguments.Length} arguments but got {call.Arguments.Count}");
        }

        List<Type> arguments = [];

        for (int i = 0; i < call.Arguments.Count; i++)
        {
            Type currentType = call.Arguments[i].Accept(this);
            Type toMatchType = functionType.Arguments[i];

            Console.WriteLine(currentType + " " + toMatchType);

            if (currentType != toMatchType)
            {
                throw new Exception($"Expected {toMatchType} but got {currentType}");
            }
            else
            {
                arguments.Add(currentType);
            }
        }

        return function;
    }

    public void VisitExpression(Statement.Expression Statement)
    {
        Statement.expression.Accept(this);
    }

    public void VisitFunction(Statement.Function function)
    {
        // Define the function in the global environment
        Environment.Define(function.Name.Lexeme, function.Type);

        TypeEnvironment prev = Environment;

        // Creating a local environment
        TypeEnvironment localEnvironment = new();

        Environment = localEnvironment;

        Type functionType = function.Type;

        // Defining my arguments in the local environment
        foreach (var arg in function.Args)
        {
            localEnvironment.Define(arg.Name.Lexeme, arg.Type);
        }

        foreach (var statement in function.Body)
        {
            statement.Accept(this);
        }

        if (hasReturn && returnType != functionType)
        {
            throw new Exception($"Function {function.Name.Lexeme} should return \"{functionType}\" but returns \"{returnType}\"");
        }

        if (!hasReturn && returnType is not Void)
        {
            throw new Exception($"Function {function.Name.Lexeme} must return a value of type {functionType}");
        }

        // Switching back to the previous environment
        Environment = prev;
    }

    public Type? VisitGrouping(Expr.Grouping expression)
    {
        return expression.Accept(this);
    }

    public void VisitIf(Statement.If Statement)
    {
        Type condition = Statement.Condition.Accept(this);

        if (condition is not Boolean)
        {
            throw new Exception($"Condition ({condition}) should be Boolean");
        }
        else
        {
            Statement.ThenBranch.Accept(this);
        }
    }

    public Type VisitLiteral(Expr.Literal expression)
    {
        return expression.Type;
    }

    public void VisitLog(Statement.Log Statement)
    {
        Statement.expression?.Accept(this);
    }

    public Type? VisitLogical(Expr.Logical expression)
    {
        Type right = expression.Right.Accept(this);
        Type left = expression.Right.Accept(this);

        if (left is not Boolean)
        {
            throw new Exception($"Left ({left}) should be Boolean");
        }
        else
        {
            if (right is not Boolean)
            {
                throw new Exception($"Right ({right}) should be Boolean");
            }
            else
            {
                return new Boolean();
            }
        }
    }

    public void VisitReturn(Statement.Return statement)
    {
        hasReturn = true;

        returnType = statement.Value?.Accept(this)!;
    }

    public Type? VisitUnary(Expr.Unary expression)
    {
        Type type = expression.Right.Accept(this);

        switch (expression.Operation.Type)
        {
            case TokenType.MINUS:
                if (type is Number) return new Number();
                else throw new Exception($"Operator should be of type Number, instead got \"{type}\"");
            case TokenType.BANG:
                if (type is Boolean) return new Boolean();
                else throw new Exception($"Operator should be of type Boolean, instead got \"{type}\"");
            default:
                throw new Exception("Unsupported unary operation.");
        }
    }

    public Type? VisitVariableExpression(Expr.VariableExpression expression)
    {
        return Environment.Get(expression.Name.Lexeme);
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        Type? type;

        if (statement.Type != null)
        {
            if (statement.Initializer != null)
            {
                Type valueType = statement.Initializer.Accept(this);

                /* Compare the type of the assigned value to the type of the variable
                if they are not the same, throw an error */

                if (valueType != statement.Type)
                {
                    throw new Exception($"Type mismatch in variable declaration \"{statement.Name.Lexeme}\". Expected \"{statement.Type}\" but got \"{valueType}\"");
                }
            }

            type = statement.Type;
        }
        else
        {
            // If no type, infer it from the initializer
            type = statement.Initializer!.Accept(this);
        }

        Environment.Define(statement.Name.Lexeme, type!);
    }

    public void VisitWhile(Statement.While Statement)
    {
        Type condition = Statement.Condition.Accept(this);
        if (condition is not Boolean)
        {
            throw new Exception($"Condition (${condition}) should be Boolean");
        }
        else
        {
            Statement.Body.Accept(this);
        }
    }
}