interface ICallable
{
    int Arity();
    string Name();
    object? Call(Interpreter interpreter, List<object> arguments);
    object? Call(Compiler compiler, List<object> arguments);
}

class LangFunction(Statement.Function declaration) : ICallable
{
    public readonly Statement.Function Declaration = declaration;

    public int Arity()
    {
        return Declaration.Args.Count;
    }

    public string Name()
    {
        return Declaration.Name.Lexeme;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        InterpreterEnv environment = new(Interpreter.Globals);

        for (int i = 0; i < Declaration.Args.Count; i++)
        {
            environment.Define(Declaration.Args[i].Name.Lexeme
                , arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(Declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public object? Call(Compiler compiler, List<object> arguments)
    {
        CompilerEnv environment = new(Compiler.Globals);

        for (int i = 0; i < Declaration.Args.Count; i++)
        {
            environment.Define(Declaration.Args[i].Name.Lexeme
                , arguments[i]);
        }

        try
        {
            compiler.CompileBlock(Declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return "<fn " + Declaration.Name.Lexeme + ">";
    }
}