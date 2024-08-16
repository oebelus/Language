class Compiler : Expr.IVisitor<object>, Statement.IVisitor
{
    public string ByteCode = "";
    public string labels = "";
    private int AddressCount = 0;
    private bool isFunction = false;
    private bool isCondition = false;

    public static readonly CompilerEnv Globals = new();
    private static CompilerEnv Environment = Globals;

    public string Compile(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }

        return $"{ByteCode.Trim()} HALT {labels.Trim()}";
    }

    public object? VisitLiteral(Expr.Literal literal)
    {

        ByteCode += !isFunction && !isCondition ? $" {Instruction.instruction[Instructions.PUSH]} {literal.Value}" : string.Empty;

        labels += isCondition ? $" {Instruction.instruction[Instructions.PUSH]} {literal.Value}" : string.Empty;

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        if (!isFunction) ByteCode += $" {Instruction.operation[binary.Operation.Type]}";
        else labels += $" {Instruction.operation[binary.Operation.Type]}";
        
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
            ByteCode += $" {Instruction.instruction[Instructions.PUSH]} {Environment.Get(expression.Name, isFunction)?.ToString()} {Instruction.instruction[Instructions.GLOAD]}";

        return null;        
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.name.Lexeme, AddressCount);

        CompileExpr(variable.initializer);

        ByteCode += $" {Instruction.instruction[Instructions.PUSH]} {AddressCount} {Instruction.instruction[Instructions.GSTORE]}";

        AddressCount++;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name, isFunction)!;

        if (!isFunction && !isCondition)
            ByteCode += $" {Instruction.instruction[Instructions.PUSH]} {address} {Instruction.instruction[Instructions.GSTORE]}";

        else
            labels += $" {Instruction.instruction[Instructions.PUSH]} {address} {Instruction.instruction[Instructions.GSTORE]}";

        return null;
    }

    public void VisitBlock(Statement.Block block)
    {
        CompileBlock(block.Statements, new CompilerEnv(Environment));
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

    public void VisitFunction(Statement.Function function)
    {
        labels += $" </{function.Name.Lexeme}>";

        isFunction = true;
        CompileBlock(function.Body, Environment);
        isFunction = false;

        Environment.Define(function.Name.Lexeme, function);
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

    public void VisitIf(Statement.If Statement)
    {
        string label_1 = GenerateRandomString();
        string label_2 = GenerateRandomString();
        CompileExpr(Statement.Condition);

        ByteCode += $" {Instruction.instruction[Instructions.CJUMP]} <{label_1}>";

        Statement.ThenBranch.Accept(this);

        ByteCode += $" {Instruction.instruction[Instructions.CJUMP]} <{label_2}>";

        Statement.ElseBranch.Accept(this);

        isCondition = true;
        labels += $" {label_1}:";

        Statement.ThenBranch.Accept(this);

        labels += $" {label_2}:";

        Statement.ElseBranch.Accept(this);
        isCondition = false;
    }

    public void VisitLog(Statement.Log Statement)
    {
        throw new NotImplementedException();
    }

    public void VisitWhile(Statement.While Statement)
    {
        throw new NotImplementedException();
    }

    public void VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        labels += $" {Instruction.instruction[Instructions.RET]}";
    }

    public void VisitExpression(Statement.Expression statement)
    {
        CompileExpr(statement.expression);
    }

    private object CompileExpr(Expr expr)
    {
        return expr.Accept(this);
    }

    private static string GenerateRandomString()
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(Enumerable.Repeat(chars, 5)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
