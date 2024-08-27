using Language.TypeChecker;

class TypeChecker : Expr.IVisitor<Language.TypeChecker.Type>, Statement.IVisitor
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
    public Language.TypeChecker.Type? VisitAssign(Expr.Assign expression)
    {
        Language.TypeChecker.Type value = expression.Value.Accept(this);
        Environment.Assign(expression.Name.Lexeme, value);
        return value;
    }

    public Language.TypeChecker.Type? VisitBinary(Expr.Binary expression)
    {
        Language.TypeChecker.Type right = expression.Right.Accept(this);
        Language.TypeChecker.Type left = expression.Right.Accept(this);

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

    public Language.TypeChecker.Type? VisitCall(Expr.Call expression)
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

    public Language.TypeChecker.Type? VisitGrouping(Expr.Grouping expression)
    {
        return expression.Accept(this);
    }

    public void VisitIf(Statement.If Statement)
    {
        Language.TypeChecker.Type condition = Statement.Condition.Accept(this);
        if (condition is not Language.TypeChecker.Boolean)
        {
            throw new Exception($"Condition (${condition}) should be Boolean");
        }
        else
        {
            Statement.ThenBranch.Accept(this);
        }
    }

    public Language.TypeChecker.Type? VisitLiteral(Expr.Literal expression)
    {
        if (expression.Value is double) return new Language.TypeChecker.Number();
        else if (expression.Value is bool) return new Language.TypeChecker.Boolean();
        else throw new Exception("Unsupported literal type.");
    }

    public void VisitLog(Statement.Log Statement)
    {

    }

    public Language.TypeChecker.Type? VisitLogical(Expr.Logical expression)
    {
        Language.TypeChecker.Type right = expression.Right.Accept(this);
        Language.TypeChecker.Type left = expression.Right.Accept(this);

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

    public Language.TypeChecker.Type? VisitUnary(Expr.Unary expression)
    {
        Language.TypeChecker.Type type = expression.Right.Accept(this);

        switch (expression.Operation.Type)
        {
            case TokenType.MINUS:
                if (type is Language.TypeChecker.Number) return new Language.TypeChecker.Number();
                else throw new Exception("Unsupported unary operation.");
            case TokenType.BANG:
                if (type is Language.TypeChecker.Boolean) return new Language.TypeChecker.Boolean();
                else if (type is Language.TypeChecker.Number) return new Language.TypeChecker.Boolean();
                else throw new Exception("Unsupported unary operation.");
            default:
                throw new Exception("Unsupported unary operation.");
        }
    }

    public Language.TypeChecker.Type? VisitVariableExpression(Expr.VariableExpression expression)
    {
        return Environment.Get(expression.Name.Lexeme);
    }

    public void VisitVariableStatement(Statement.VariableStatement statement)
    {
        Language.TypeChecker.Type? type = null;

        if (statement.Initializer != null)
        {
            type = statement.Initializer.Accept(this);
        }

        Environment.Define(statement.Name.Lexeme, type!);
    }

    public void VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }
}