interface IEnvironment {
    void Define(string name, object value);
    object? Get(string name);
    void Assign(string name, object value);
    bool IsDeclared(string name);
}


class CompilerEnv : IEnvironment
{
    readonly CompilerEnv Enclosing;

    // local scope nested inside an outer one
    public CompilerEnv(CompilerEnv enclosing)
    {
        Enclosing = enclosing;
    }

    // global scope’s environment
    public CompilerEnv()
    {
        Enclosing = null!;
    }

    private readonly Dictionary<string, object> addresses = [];

    public void Define(string name, object value)
    {
        addresses.Add(name, value);
    }

    public object? Get(string name)
    {
        if (IsDeclared(name)) return addresses[name];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return $"Undefined Variable '{name}'.";
    }

    public void Assign(string name, object value)
    {
        if (IsDeclared(name))
        {
            addresses[name] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }
    }

    public bool IsDeclared(string name)
    {
        if (addresses.TryGetValue(name, out object? _)) return true;
        return false;
    }    
}


class InterpreterEnv : IEnvironment
{
    readonly InterpreterEnv Enclosing;

    // local scope nested inside an outer one
    public InterpreterEnv(InterpreterEnv enclosing)
    {
        Enclosing = enclosing;
    }

    // global scope’s environment
    public InterpreterEnv()
    {
        Enclosing = null!;
    }

    private readonly Dictionary<string, object> values = [];

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public object Get(string name)
    {
        if (IsDeclared(name)) return values[name];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return "Undefined Variable '" + name + "'.";
    }

    public void Assign(string name, object value)
    {
        if (IsDeclared(name))
        {
            values[name] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }
    }

    public bool IsDeclared(string name)
    {
        if (values.TryGetValue(name, out object? _)) return true;
        return false;
    }
}