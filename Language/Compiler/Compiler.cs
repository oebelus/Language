class Compiler : Expr.IVisitor<object>, Statement.IVisitor<Action>
{
    public string ByteCode = "";
    public string functions = "";
    private int AddressCount = 0;
    private bool isFunction = false;

    public static readonly CompilerEnv Globals = new();
    private static CompilerEnv Environment = Globals;

    public string Compile(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }

        return $"{ByteCode.Trim()} HALT {functions.Trim()}";
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        if (!isFunction)
        {
            ByteCode += $" {Instruction.instruction[Instructions.PUSH]}";
            ByteCode += $" {literal.Value}";
        }

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        if (!isFunction) ByteCode += $" {Instruction.operation[binary.Operation.Type]}";
        else functions += $" {Instruction.operation[binary.Operation.Type]}";
        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        CompileExpr(unary.Right);

        ByteCode += $" {Instruction.operation[unary.Operation.Type]}";

        return null;
    }

    public object? VisitLogical(Expr.Logical logical)
    {
        CompileExpr(logical.Left);
        CompileExpr(logical.Right);

        ByteCode += $" {Instruction.operation[logical.Operation.Type]}";

        return null;
    }

    public object VisitGrouping(Expr.Grouping expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        if (!isFunction)
        {
            ByteCode += $" {Instruction.instruction[Instructions.PUSH]}";
            ByteCode += $" {Environment.Get(expression.Name, isFunction)?.ToString()}";
            ByteCode += $" {Instruction.instruction[Instructions.GLOAD]}";
        }

        return null;
    }

    public Action? VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.name.Lexeme, AddressCount);

        CompileExpr(variable.initializer);

        ByteCode += $" {Instruction.instruction[Instructions.PUSH]}";
        ByteCode += $" {AddressCount}";

        ByteCode += " " + Instruction.instruction[Instructions.GSTORE];
        AddressCount++;
        return null;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name, isFunction)!;

        ByteCode += $" {Instruction.instruction[Instructions.PUSH]}";
        ByteCode += $" {address}";

        ByteCode += $" {Instruction.instruction[Instructions.GSTORE]}";

        return null;
    }

    public Action? VisitBlock(Statement.Block block)
    {
        CompileBlock(block.Statements, new CompilerEnv(Environment));
        return null;
    }

    public void CompileBlock(List<Statement> statements, CompilerEnv environment)
    {
        CompilerEnv previous = Environment;

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

    public Action? VisitFunction(Statement.Function function)
    {
        functions += $" </{function.Name.Lexeme}>";

        isFunction = true;
        CompileBlock(function.Body, Environment);
        isFunction = false;

        Environment.Define(function.Name.Lexeme, function);

        return null;
    }

    public object? VisitCall(Expr.Call call)
    {
        foreach (Expr argument in call.Arguments)
        {
            CompileExpr(argument);
        }

        ByteCode += $" CALL <{call.Callee.Name.Lexeme}>";
        return null;
    }

    public Action? VisitIf(Statement.If Statement)
    {
        throw new NotImplementedException();
    }

    public Action? VisitLog(Statement.Log Statement)
    {
        throw new NotImplementedException();
    }

    public Action? VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }

    public Action? VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        functions += $" {Instruction.instruction[Instructions.RET]}";

        return null;
    }

    public Action? VisitExpression(Statement.Expression statement)
    {
        CompileExpr(statement.expression);

        return null;
    }

    private object CompileExpr(Expr expr)
    {
        return expr.Accept(this);
    }
}
