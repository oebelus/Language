class Compiler : Expr.IVisitor<object>, Statement.IVisitor<Action>
{
    public List<byte> ByteCode = [];
    private int AddressCount = 0;

    public static readonly CompilerEnv Globals = new();
    private readonly CompilerEnv Environment = Globals;

    public void Compile(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            statement.Accept(this);
        }
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        ByteCode.Add(Instruction.instruction[Instructions.PUSH]);
        ByteCode.AddRange(ToByteArray(literal.Value!.ToString()!));

        return null;
    }

    public object VisitAssign(Expr.Assign expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        CompileExpr(binary.Left);
        CompileExpr(binary.Right);

        ByteCode.Add(Instruction.operation[binary.Operation.Type]);
        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        CompileExpr(unary.Right);

        ByteCode.Add(Instruction.operation[unary.Operation.Type]);

        return null;
    }

    public object VisitLogical(Expr.Logical expression)
    {
        return 0;
    }

    public object VisitCall(Expr.Call expression)
    {
        throw new NotImplementedException();
    }

    public object VisitGrouping(Expr.Grouping expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitVariableExpression(Expr.VariableExpression expression)
    {
        ByteCode.Add(Instruction.instruction[Instructions.PUSH]);
        ByteCode.AddRange(ToByteArray(Environment.Get(expression.Name).ToString()!));
        ByteCode.Add(Instruction.instruction[Instructions.LOAD]);
        return null;
    }

    public Action? VisitVariableStatement(Statement.VariableStatement variable)
    {
        Environment.Define(variable.name.Lexeme, AddressCount);

        CompileExpr(variable.initializer);

        ByteCode.Add(Instruction.instruction[Instructions.PUSH]);
        ByteCode.AddRange(ToByteArray(AddressCount.ToString()));

        ByteCode.Add(Instruction.instruction[Instructions.STORE]);
        AddressCount++;
        return null;
    }

    public Action? VisitBlock(Statement.Block Statement)
    {
        throw new NotImplementedException();
    }

    public Action? VisitFunction(Statement.Function Statement)
    {
        throw new NotImplementedException();
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

    public Action? VisitReturn(Statement.Return Statement)
    {
        throw new NotImplementedException();
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

    private static byte[] ToByteArray(string str)
    {
        int nbr = int.Parse(str);

        byte[] nbrArray = BitConverter.GetBytes(nbr);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(nbrArray);
        }

        return nbrArray;
    }

    private static bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }
}
