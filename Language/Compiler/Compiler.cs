using Instruction = Language.stackVM.Instruction;
using Instructions = Language.stackVM.Instructions;

class Compiler : Expr.IVisitor<object>, Statement.IVisitor
{
    public string ByteCode = "";
    public string functions = "";
    private readonly Stack<string> breakLabels = new(24);
    private readonly Stack<string> continueLabels = new(24);
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
        if (literal.Value is bool b) Append($" {Instruction.cInstruction[Instructions.PUSH]} {(b ? 1 : 0)}");
        else Append($" {Instruction.cInstruction[Instructions.PUSH]} {literal.Value}");

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        if (binary.Operation.Type == TokenType.LESS_EQUAL)
            Append($" {Instruction.cInstruction[Instructions.GT]} {Instruction.cInstruction[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.GREATER_EQUAL)
            Append($" {Instruction.cInstruction[Instructions.LT]} {Instruction.cInstruction[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.BANG_EQUAL)
        {
            Append($" {Instruction.cInstruction[Instructions.EQ]} {Instruction.cInstruction[Instructions.NOT]}");
        }

        else Append($" {Instruction.cOperation[binary.Operation.Type]}");

        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        CompileExpr(unary.Right);

        Append($" {Instruction.cOperation[unary.Operation.Type]}");

        return null;
    }

    public object? VisitLogical(Expr.Logical logical)
    {
        CompileExpr(logical.Left);
        CompileExpr(logical.Right);

        Append($" {Instruction.cOperation[logical.Operation.Type]}");

        return null;
    }

    public object? VisitGrouping(Expr.Grouping expression)
    {
        CompileExpr(expression.Expression);

        return null;
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        Append($" {Instruction.cInstruction[Instructions.PUSH]} {Environment.Get(expression.Name)?.ToString()} {Instruction.cInstruction[Instructions.LOAD]}");

        return null;
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.Name.Lexeme, AddressCount);

        CompileExpr(variable.Initializer!);

        Append($" {Instruction.cInstruction[Instructions.PUSH]} {AddressCount} {Instruction.cInstruction[Instructions.STORE]}");

        AddressCount++;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name)!;

        Append($" {Instruction.cInstruction[Instructions.PUSH]} {address} {Instruction.cInstruction[Instructions.STORE]}");

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
                object address = Environment.Get(function.Args[i].Name)!;
                Append($" {Instruction.cInstruction[Instructions.PUSH]} {address} {Instruction.cInstruction[Instructions.STORE]}");
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
        Append($" {Instruction.cInstruction[Instructions.CJUMP]} <{label_1}>");

        // if false: compiling ElseBranch
        Statement.ElseBranch?.Accept(this);

        Append($" {Instruction.cInstruction[Instructions.JUMP]} <{label_2}>");

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

        breakLabels.Push(end_label);
        continueLabels.Push(start_label);

        Append($" {start_label}:");

        CompileExpr(Statement.Condition);

        // Check if condition is False
        Append($" {Instruction.cInstruction[Instructions.PUSH]} 0 {Instruction.cInstruction[Instructions.EQ]}");

        // Jump to end if condition is False
        Append($" {Instruction.cInstruction[Instructions.CJUMP]} <{end_label}>");

        Statement.Body.Accept(this);

        // Jump back to the start of the loop
        Append($" {Instruction.cInstruction[Instructions.JUMP]} <{start_label}>");

        // End label for the loop
        Append($" {end_label}:");
    }

    public void VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        Append($" {Instruction.cInstruction[Instructions.RET]}");
    }

    public void VisitBreak(Statement.Break statement)
    {
        if (breakLabels.Length() > 0)
        {
            string address = breakLabels.Pop();
            Append($" {Instruction.cInstruction[Instructions.JUMP]} <{address}>");
        }
    }

    public void VisitContinue(Statement.Continue statement)
    {
        if (continueLabels.Length() > 0)
        {
            string address = continueLabels.Pop();
            Append($" {Instruction.cInstruction[Instructions.JUMP]} <{address}>");
        }
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

    public void Reset()
    {
        ByteCode = "";
        functions = "";
        AddressCount = 0;
        isFunction = false;
        isLog = false;
        Environment.Clear();
    }
}
