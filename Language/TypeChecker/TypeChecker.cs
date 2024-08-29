using Type = Language.TypeChecker.Type;
using Boolean = Language.TypeChecker.Boolean;
using Language.TypeChecker;

class TypeChecker : Expr.IVisitor<Type>, Statement.IVisitor
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

    public Type? VisitBinary(Expr.Binary expression)
    {
        Type right = expression.Right.Accept(this);
        Type left = expression.Right.Accept(this);

        if (right != left || left is not Number || right is not Number)
        {
            throw new Exception($"Right (${right}) and Left (${left}) should be of the same type");
        }
        else
        {
            return left;
        }
    }

    public void VisitBlock(Statement.Block Statement)
    {
        foreach (var statement in Statement.Statements)
        {
            statement.Accept(this);
        }
    }

    public Type? VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
    }

    public void VisitExpression(Statement.Expression Statement)
    {
        Statement.Accept(this);
    }

    public void VisitFunction(Statement.Function Statement)
    {
    }

    public Type? VisitGrouping(Expr.Grouping expression)
    {
        return expression.Accept(this);
    }

    public void VisitIf(Statement.If Statement)
    {
        Type condition = Statement.Condition.Accept(this);
        if (condition is not Language.TypeChecker.Boolean)
        {
            throw new Exception($"Condition (${condition}) should be Boolean");
        }
        else
        {
            Statement.ThenBranch.Accept(this);
        }
    }

    public Type? VisitLiteral(Expr.Literal expression)
    {
        if (expression.Value is double) return new Number();
        else if (expression.Value is bool) return new Language.TypeChecker.Boolean();
        else throw new Exception("Unsupported literal type.");
    }

    public void VisitLog(Statement.Log Statement)
    {

    }

    public Type? VisitLogical(Expr.Logical expression)
    {
        Type right = expression.Right.Accept(this);
        Type left = expression.Right.Accept(this);

        if (left is not Language.TypeChecker.Boolean)
        {
            throw new Exception($"Left (${left}) should be Boolean");
        }
        else
        {
            if (right is not Language.TypeChecker.Boolean)
            {
                throw new Exception($"Right (${right}) should be Boolean");
            }
            else
            {
                return new Language.TypeChecker.Boolean();
            }
        }
    }

    public void VisitReturn(Statement.Return Statement)
    {
        throw new NotImplementedException();
    }

    public Type? VisitUnary(Expr.Unary expression)
    {
        Type type = expression.Right.Accept(this);

        switch (expression.Operation.Type)
        {
            case TokenType.MINUS:
                if (type is Number) return new Number();
                else throw new Exception("Unsupported unary operation.");
            case TokenType.BANG:
                if (type is Language.TypeChecker.Boolean) return new Language.TypeChecker.Boolean();
                else if (type is Number) return new Language.TypeChecker.Boolean();
                else throw new Exception("Unsupported unary operation.");
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
            // If type has an initial value, use it
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