class Compiler : Expr.IVisitor<object>, Statement.IVisitor
{
    public string ByteCode = "";
    public string functions = "";
    private int AddressCount = 0;
    private bool isFunction = false;
    private bool isLog = false;

    public static readonly CompilerEnv Globals = new();
    public CompilerEnv Environment = Globals;

    public string Compile(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }

        return $"{ByteCode.Trim()} HALT {functions.Trim()}";
    }

    public string CompileExpressions(List<Expr> expressions)
    {
        foreach (var expression in expressions)
        {
            expression.Accept(this);
        }
        return $"{ByteCode.Trim()} HALT {functions.Trim()}";
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        if (isLog) return null;
        if (literal.Value is bool b) Append($" {Instruction.instruction[Instructions.PUSH]} {(b ? 1 : 0)}");
        else Append($" {Instruction.instruction[Instructions.PUSH]} {literal.Value}");

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        if (binary.Operation.Type == TokenType.LESS_EQUAL)
            Append($" {Instruction.instruction[Instructions.GT]} {Instruction.instruction[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.GREATER_EQUAL)
            Append($" {Instruction.instruction[Instructions.LT]} {Instruction.instruction[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.BANG_EQUAL)
        {
            Append($" {Instruction.instruction[Instructions.EQ]} {Instruction.instruction[Instructions.NOT]}");
        }

        else Append($" {Instruction.operation[binary.Operation.Type]}");

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

    public object? VisitGrouping(Expr.Grouping expression)
    {
        CompileExpr(expression.Expression);

        return null;
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        Append($" {Instruction.instruction[Instructions.PUSH]} {Environment.Get(expression.Name.Lexeme)?.ToString()} {Instruction.instruction[Instructions.LOAD]}");

        return null;
    }

    /*************  ✨ Codeium Command ⭐  *************/
    /// <summary>
    /// Compile a variable declaration statement.
    /// </summary>
    /// <param name="variable">The variable declaration statement.</param>
    /// <remarks>
    /// The variable is added to the environment with the current address count value.
    /// The initializer expression is compiled and its value is stored at the address.
    /// Then the address count is incremented.
    /// </remarks>
    /******  2a373763-d9f3-4f1d-9505-14a65dcdda5c  *******/
    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.Name.Lexeme, AddressCount);

        CompileExpr(variable.Initializer!);

        Append($" {Instruction.instruction[Instructions.PUSH]} {AddressCount} {Instruction.instruction[Instructions.STORE]}");

        AddressCount++;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name.Lexeme)!;

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

        // Create a local environment
        CompilerEnv previous = Environment;

        try
        {
            Environment = new CompilerEnv(Environment);

            foreach (var arg in function.Args)
            {
                Environment.Define(arg.Name.Lexeme, AddressCount);
                AddressCount++;
            }

            int argsLength = function.Args.Count;

            isFunction = true;

            Append($" {function.Name.Lexeme}:");
            for (int i = 0; i < argsLength; i++)
            {
                object address = Environment.Get(function.Args[i].Name.Lexeme)!;
                Append($" {Instruction.instruction[Instructions.PUSH]} {address} {Instruction.instruction[Instructions.STORE]}");
            }
            CompileBlock(function.Body, Environment);
        }
        finally
        {
            Environment = previous;
        }


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
        Statement.ElseBranch?.Accept(this);

        Append($" {Instruction.instruction[Instructions.JUMP]} <{label_2}>");

        // if true: CJUMP here
        Append($" {label_1}:");

        Statement.ThenBranch.Accept(this);

        // Label_2
        Append($" {label_2}:");
    }

    public void VisitLog(Statement.Log Statement)
    {
        if (Statement.expression != null)
        {
            isLog = true;
            CompileExpr(Statement.expression);
            isLog = false;
        }
    }

    public void VisitWhile(Statement.While Statement)
    {
        string start_label = GenerateRandomString();
        string end_label = GenerateRandomString();

        Append($" {start_label}:");

        CompileExpr(Statement.Condition);

        // Check if condition is False
        Append($" {Instruction.instruction[Instructions.PUSH]} 0 {Instruction.instruction[Instructions.EQ]}");

        // Jump to end if condition is False
        Append($" {Instruction.instruction[Instructions.CJUMP]} <{end_label}>");

        Statement.Body.Accept(this);

        // Jump back to the start of the loop
        Append($" {Instruction.instruction[Instructions.JUMP]} <{start_label}>");

        // End label for the loop
        Append($" {end_label}:");
    }

    public void VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        Append($" {Instruction.instruction[Instructions.RET]}");
    }

    public void VisitBreak(Statement.Break statement)
    {
        return;
    }

    public void VisitContinue(Statement.Continue statement)
    {
        throw new NotImplementedException();
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
            functions += code;
        else
            ByteCode += code;
    }
}
