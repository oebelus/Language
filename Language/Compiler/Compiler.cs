class Compiler : Expr.IVisitor<object>, Statement.IVisitor<Action>
{
    public List<byte> ByteCode = [];

    public void Compile(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            Execute(statement);
        }
    }

    private void Execute(Statement statement)
    {
        statement.Accept(this);
    }

    public object? VisitLiteral(Expr.Literal literal)
    {
        ByteCode.AddRange(ToByteArray(literal.Value!.ToString()!));

        return null;
    }

    public object VisitAssign(Expr.Assign expression)
    {
        throw new NotImplementedException();
    }

    public object? VisitBinary(Expr.Binary binary)
    {
        Compile(binary.Left);
        Compile(binary.Right);

        ByteCode.Add(Instruction.instruction[binary.Operation.Type]);
        return null;
    }

    public object? VisitUnary(Expr.Unary unary)
    {
        Compile(unary.Right);

        ByteCode.Add(Instruction.instruction[unary.Operation.Type]);

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
        Compile(statement.expression);
        return null;
    }

    public Action? VisitVariableStatement(Statement.VariableStatement statement)
    {
        throw new NotImplementedException();
    }

    private object Compile(Expr expr)
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
