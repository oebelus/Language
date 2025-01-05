using Type = Language.Typer.Type;
using Boolean = Language.Typer.Boolean;
using Void = Language.Typer.Void;
using Integer = Language.Typer.Number;
using Chars = Language.Typer.String;

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
        Type? type = Environment.Get(expression.Name)[0];
        Type? valueType = expression.Value.Accept(this);

        // Compare the type of the assigned value to the type of the variable
        if (valueType != type)
        {
            throw new Exception($"Wrong type for variable: {expression.Name.Lexeme} ({type}) and ({valueType})");
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

            if ((rightType is not Integer && rightType is not Chars) || (leftType is not Integer && leftType is not Chars))
            {
                throw new Exception($"Right ({rightType}) and Left ({leftType}) should be of type Number or String");
            }
        }

        else if (booleanOps.Contains(binaryExpression.Operation.Type))
        {
            if ((rightType is not Integer && rightType is not Boolean) || (leftType is not Integer && leftType is not Boolean))
            {
                throw new Exception($"Right ({rightType}) and Left ({leftType}) should be of type Number or Boolean");
            }

            else return new Boolean();
        }

        else
        {
            if (rightType is not Integer || leftType is not Integer)
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
        // Retrieve the callee type
        Type calleeType = call.Callee.Accept(this);

        // Retrieve the function types from the environment with the callee name
        List<Type> types = Environment.Get(call.Callee.Name);

        Type returnType = types[0];

        List<Type> argumentsType = types[1..];

        // Check the return type
        if (calleeType != returnType)
        {
            throw new Exception($"Expected \"{returnType}\" but got {calleeType}");
        }

        // Check for number of arguments
        if (call.Arguments.Count != argumentsType.Count)
        {
            throw new Exception($"Expected \"{argumentsType.Count}\" arguments but got \"{call.Arguments.Count}\"");
        }

        // Evaluate the arguments of the call 
        List<Type> calleeArguments = [];

        for (int i = 0; i < call.Arguments.Count; i++)
        {
            Type currentType = call.Arguments[i].Accept(this);

            if (currentType != argumentsType[i])
            {
                throw new Exception($"Expected \"{argumentsType[i]}\" but got \"{currentType}\"");
            }
            else
            {
                calleeArguments.Add(currentType);
            }
        }

        return returnType;
    }

    public void VisitExpression(Statement.Expression Statement)
    {
        Statement.expression.Accept(this);
    }

    public void VisitFunction(Statement.Function function)
    {
        Type functionType = function.Type;

        // Getting the arguments' types
        List<Type> types = [function.Type];

        foreach (var arg in function.Args)
        {
            types.Add(arg.Type);
        }

        // Define the function in the gloabal environment
        Environment.Define(function.Name.Lexeme, types);

        // Create a local environment
        TypeEnvironment previous = Environment;
        Environment = new(Environment);

        // Add function arguments to the local environment
        foreach (var arg in function.Args)
        {
            Environment.Define(arg.Name.Lexeme, [arg.Type]);
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

        // Restore previous environment
        Environment = previous;
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

        Statement.ThenBranch.Accept(this);

        Statement.ElseBranch?.Accept(this);
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
                if (type is Integer) return new Integer();
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
        return Environment.Get(expression.Name)[0];
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

        Environment.Define(statement.Name.Lexeme, [type]!);
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

    public void VisitBreak(Statement.Break statement)
    {
        throw new NotImplementedException();
    }

    public void VisitContinue(Statement.Continue statement)
    {
        throw new NotImplementedException();
    }
}
