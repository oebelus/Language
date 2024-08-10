interface ICallable {
    int Arity();
    object? Call(Interpreter interpreter, List<object> arguments);
}

class Callable(int arity, object call, string representation) : ICallable {
    public int Arity()
    {
        return arity;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        return call;
    }

    public override string ToString() {
        return representation; // representation of the callable
    }
}

class LangFunction(Statement.Function declaration) : ICallable
{
    private readonly Statement.Function Declaration = declaration;

    public int Arity()
    {
        return Declaration.Args.Count;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new(Interpreter.Globals);

        for (int i = 0; i < Declaration.Args.Count; i++) {
            environment.Define(Declaration.Args[i].Lexeme
                , arguments[i]);
        }

        interpreter.ExecuteBlock(Declaration.Body, environment);

        return null;
    }

    public override string ToString() {
        return "<fn " + Declaration.Name.Lexeme + ">";
    }
}