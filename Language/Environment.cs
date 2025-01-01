using Type = Language.Typer.Type;

interface IEnvironment<T>
{
    void Define(string name, T value);
    T? Get(string name);
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

    public readonly Dictionary<string, object> addresses = [];

    public void Define(string name, object value)
    {
        try
        {
            addresses.Add(name, value);
        }
        catch (ArgumentException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Variable '" + name + "' already declared.");
            Console.ResetColor();
        }
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

    public void Clear()
    {
        addresses.Clear();
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
        try
        {
            values.Add(name, value);
        }
        catch (ArgumentException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Variable '" + name + "' already declared.");
            Console.ResetColor();
        }
    }

    public object Get(string name)
    {
        if (IsDeclared(name)) return values[name];

        else if (Enclosing != null) return Enclosing.Get(name);

        else throw new Exception("Undefined Variable '" + name + "'.");
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

    internal void Clear()
    {
        values.Clear();
    }
}

class TypeEnvironment : IEnvironment<List<Type>>
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

    private readonly Dictionary<string, List<Type>> values = [];

    public void Define(string name, List<Type> types)
    {
        values.Add(name, types);
    }

    public List<Type> Get(string name)
    {
        if (IsDeclared(name)) return values[name];

        else if (Enclosing != null) return Enclosing.Get(name);

        else throw new Exception("Undefined Variable '" + name + "'.");
    }

    public void Assign(string name, List<Type> value)
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
        if (values.TryGetValue(name, out List<Type>? _)) return true;
        return false;
    }
}