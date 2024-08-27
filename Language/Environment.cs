interface IEnvironment<T>
{
    void Define(string name, T value);
    object? Get(string name);
    void Assign(string name, T value);
    bool IsDeclared(string name);
}


class CompilerEnv : IEnvironment<object>
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


class InterpreterEnv : IEnvironment<object>
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

class TypeEnvironment : IEnvironment<Language.TypeChecker.Type>
{
    readonly TypeEnvironment Enclosing;

    // local scope nested inside an outer one
    public TypeEnvironment(TypeEnvironment enclosing)
    {
        Enclosing = enclosing;
    }

    // global scope’s environment
    public TypeEnvironment()
    {
        Enclosing = null!;
    }

    private readonly Dictionary<string, Language.TypeChecker.Type> values = [];

    public void Define(string name, Language.TypeChecker.Type value)
    {
        values.Add(name, value);
    }

    public object Get(string name)
    {
        if (IsDeclared(name)) return values[name];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return "Undefined Variable '" + name + "'.";
    }

    public void Assign(string name, Language.TypeChecker.Type value)
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
        if (values.TryGetValue(name, out Language.TypeChecker.Type? _)) return true;
        return false;
    }
}