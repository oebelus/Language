using System.ComponentModel;
using System.Text;
using Instruction = Language.ICompiler.CInstruction;
using Instructions = stackVM.Instructions;

class Compiler : Expr.IVisitor<object>, Statement.IVisitor
{
    public string ByteCode = "";
    public string functions = "";
    private readonly Stack<string> breakLabels = new(24);
    private readonly Stack<string> continueLabels = new(24);
    private int AddressCount = 0;
    private bool isFunction = false;
    private bool isLog = false;

    private bool isString = false;
    private int stringSize = 0;

    public static readonly CompilerEnv Globals = new();
    public CompilerEnv Environment = Globals;

    private static int HEAP_INDEX = 0;
    private static Dictionary<int, int> Allocated = [];

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
        if (literal.Value is bool b) Append($" {Instruction.CInstructions[Instructions.PUSH]} {(b ? 1 : 0)}");
        else if (literal.Value is string s)
        {
            isString = true;

            int len = s.Length;

            if (len == 1)
            {
                Append($" {Instruction.CInstructions[Instructions.PUSH_CHAR]} {s}");
            }
            else
            {
                int length = Encoding.UTF8.GetBytes(s).Length;

                Append($" PUSH {HEAP_INDEX} PUSH {length} {Instruction.CInstructions[Instructions.GSTORE_STR]} \"{s}\"");

                int offset = HEAP_INDEX + length;

                HEAP_INDEX += offset % 4 == 0 ? offset : offset + (4 - offset % 4);

                isString = true;

                stringSize = length;
            }
        }
        else Append($" {Instruction.CInstructions[Instructions.PUSH]} {literal.Value}");

        return null;
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        if (binary.Operation.Type == TokenType.LESS_EQUAL)
            Append($" {Instruction.CInstructions[Instructions.GT]} {Instruction.CInstructions[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.GREATER_EQUAL)
            Append($" {Instruction.CInstructions[Instructions.LT]} {Instruction.CInstructions[Instructions.NOT]}");

        else if (binary.Operation.Type == TokenType.BANG_EQUAL)
        {
            Append($" {Instruction.CInstructions[Instructions.EQ]} {Instruction.CInstructions[Instructions.NOT]}");
        }
        else if (binary.Operation.Type == TokenType.PLUS)
        {
            if (isString)
            {
                Append($" CONCAT");
                isString = false;
            }
            else
            {
                Append($" {Instruction.COperation[binary.Operation.Type]}");
            }
        }

        else
        {
            Console.WriteLine("HEEERE appeeend not");
            Append($" {Instruction.COperation[binary.Operation.Type]}");

        }

        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        CompileExpr(unary.Right);

        Append($" {Instruction.COperation[unary.Operation.Type]}");

        return null;
    }

    public object? VisitLogical(Expr.Logical logical)
    {
        CompileExpr(logical.Left);
        CompileExpr(logical.Right);

        Append($" {Instruction.COperation[logical.Operation.Type]}");

        return null;
    }

    public object? VisitGrouping(Expr.Grouping expression)
    {
        CompileExpr(expression.Expression);

        return null;
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        if (Environment.Get(expression.Name) != null && int.TryParse(Environment.Get(expression.Name)!.ToString(), out int address))
        {
            if (Allocated.TryGetValue(address, out int value)) // Strings
            {
                Append($" {Instruction.CInstructions[Instructions.PUSH]} {address} {Instruction.CInstructions[Instructions.PUSH]} {value}");
            }

            else Append($" {Instruction.CInstructions[Instructions.PUSH]} {Environment.Get(expression.Name)?.ToString()} {Instruction.CInstructions[Instructions.LOAD]}");
        }

        return null;
    }

    public void VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.Name.Lexeme, AddressCount);
        CompileExpr(variable.Initializer!);

        Append($" {Instruction.CInstructions[Instructions.PUSH]} {AddressCount} {Instruction.CInstructions[Instructions.STORE]}");

        if (isString)
        {
            Allocated.Add(AddressCount, stringSize);
        }

        AddressCount++;
    }

    public object? VisitAssign(Expr.Assign expression)
    {
        CompileExpr(expression.Value);

        object address = Environment.Get(expression.Name)!;

        Append($" {Instruction.CInstructions[Instructions.PUSH]} {address} {Instruction.CInstructions[Instructions.STORE]}");

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
                Append($" {Instruction.CInstructions[Instructions.PUSH]} {address} {Instruction.CInstructions[Instructions.STORE]}");
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
        Append($" {Instruction.CInstructions[Instructions.CJUMP]} <{label_1}>");

        // if false: compiling ElseBranch
        Statement.ElseBranch?.Accept(this);

        Append($" {Instruction.CInstructions[Instructions.JUMP]} <{label_2}>");

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
        Append($" {Instruction.CInstructions[Instructions.PUSH]} 0 {Instruction.CInstructions[Instructions.EQ]}");

        // Jump to end if condition is False
        Append($" {Instruction.CInstructions[Instructions.CJUMP]} <{end_label}>");

        Statement.Body.Accept(this);

        // Jump back to the start of the loop
        Append($" {Instruction.CInstructions[Instructions.JUMP]} <{start_label}>");

        // End label for the loop
        Append($" {end_label}:");
    }

    public void VisitReturn(Statement.Return statement)
    {
        if (statement.Value != null)
            CompileExpr(statement.Value);

        Append($" {Instruction.CInstructions[Instructions.RET]}");
    }

    public void VisitBreak(Statement.Break statement)
    {
        if (breakLabels.Length() > 0)
        {
            string address = breakLabels.Pop();
            Append($" {Instruction.CInstructions[Instructions.JUMP]} <{address}>");
        }
    }

    public void VisitContinue(Statement.Continue statement)
    {
        if (continueLabels.Length() > 0)
        {
            string address = continueLabels.Pop();
            Append($" {Instruction.CInstructions[Instructions.JUMP]} <{address}>");
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
