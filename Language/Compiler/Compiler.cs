class Compiler : Expr.IVisitor<object>, Statement.IVisitor
{
    public string ByteCode = "";
    public string labels = "";
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

        return $"{ByteCode.Trim()} HALT {labels.Trim()}";
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        Append($" {Instruction.instruction[Instructions.PUSH]} {literal.Value}");

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        Append($" {Instruction.operation[binary.Operation.Type]}");

        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        CompileExpr(unary.Right);

        Append($" {Instruction.operation[unary.Operation.Type]}");

        return null;
    }

    public object? VisitLogical(Expr.Logical logical)
    {
        CompileExpr(logical.Left);
        CompileExpr(logical.Right);

        Append($" {Instruction.operation[logical.Operation.Type]}");

        return null;
    }

    public object VisitGrouping(Expr.Grouping expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        Append($" {Instruction.instruction[Instructions.PUSH]} {Environment.Get(expression.Name, isFunction)?.ToString()} {Instruction.instruction[Instructions.LOAD]}");

        return null;
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.name.Lexeme, AddressCount);

        CompileExpr(variable.initializer);

        Append($" {Instruction.instruction[Instructions.PUSH]} {AddressCount} {Instruction.instruction[Instructions.STORE]}");

        AddressCount++;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name, isFunction)!;

        Append($" {Instruction.instruction[Instructions.PUSH]} {address} {Instruction.instruction[Instructions.STORE]}");

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
        Environment.Define(function.Name.Lexeme, function);

        foreach (var arg in function.Args)
        {
            Environment.Define(arg.Lexeme, AddressCount);
            AddressCount++;
        }

        int argsLength = function.Args.Count;

        isFunction = true;
        Append($" {function.Name.Lexeme}:");
        for (int i = 0; i < argsLength; i++)
        {
            object address = Environment.Get(function.Args[i], isFunction)!;
            Append($" {Instruction.instruction[Instructions.PUSH]} {address} {Instruction.instruction[Instructions.STORE]}");
            AddressCount++;
        }
        CompileBlock(function.Body, Environment);
        isFunction = false;
    }

    public object? VisitCall(Expr.Call call)
    {
        call.Arguments.Reverse();

        foreach (Expr argument in call.Arguments)
        {
            CompileExpr(argument);
        }

        Append($" CALL <{call.Callee.Name.Lexeme}>");
        return null;
    }

    public void VisitIf(Statement.If Statement)
    {
        string label_1 = GenerateRandomString();
        string label_2 = GenerateRandomString();

        // Compiling condition
        CompileExpr(Statement.Condition);

        // CJUMP label_1
        Append($" {Instruction.instruction[Instructions.CJUMP]} <{label_1}>");

        // if false: compiling ElseBranch
        Statement.ElseBranch.Accept(this);

        Append($" {Instruction.instruction[Instructions.JUMP]} <{label_2}>");

        // if true: CJUMP here
        Append($" {label_1}:");
        Statement.ThenBranch.Accept(this);

        Append($" {Instruction.instruction[Instructions.JUMP]} <{label_2}>");

        // Label_2
        Append($" {label_2}:");
    }

    public void VisitLog(Statement.Log Statement)
    {
        throw new NotImplementedException();
    }

    public void VisitWhile(Statement.While Statement)
    {
        string start_label = GenerateRandomString();
        string end_label = GenerateRandomString();

        Append($" {start_label}:");
        CompileExpr(Statement.Condition);

        Append($" {Instruction.instruction[Instructions.PUSH]} 0 {Instruction.instruction[Instructions.EQ]}");
        Append($" {Instruction.instruction[Instructions.CJUMP]} <{end_label}>");

        Statement.Body.Accept(this);

        Append($" {Instruction.instruction[Instructions.JUMP]} <{start_label}>");

        Append($" {end_label}:");
    }

    public void VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        Append($" {Instruction.instruction[Instructions.RET]}");
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

    private void Append(string code)
    {
        if (isFunction)
            labels += code;
        else
            ByteCode += code;
    }
}
