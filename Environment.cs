using Type = Language.Typer.Type;

interface IEnvironment<T>
{
    void Define(string name, T value);
    T? Get(Token name);
    void Assign(Token name, T value);
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

    public object? Get(Token name)
    {
        if (IsDeclared(name.Lexeme)) return addresses[name.Lexeme];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return $"Undefined Variable '{name}'.";
    }

    public void Assign(Token name, object value)
    {
        if (IsDeclared(name.Lexeme))
        {
            addresses[name.Lexeme] = value;
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
        Enclosing = null;
    }

    public readonly Dictionary<string, object> values = [];

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

    public object Get(Token name)
    {
        if (IsDeclared(name.Lexeme)) return values[name.Lexeme];

        else if (Enclosing != null) return Enclosing.Get(name);

        else throw new Exception($"Undefined Variable '{name.Lexeme}' in line {name.Line}.");
    }

    public void Assign(Token name, object value)
    {
        if (IsDeclared(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new Exception($"Undefined Variable '{name.Lexeme} in line {name.Line}'.");
    }

    public object GetAt(int distance, string name)
    {
        return Ancestor(distance).values[name];
    }

    public void AssignAt(int distance, string name, object value)
    {
        Ancestor(distance).values[name] = value;
    }

    public bool IsDeclared(string name)
    {
        if (values.TryGetValue(name, out object? _)) return true;
        return false;
    }

    private InterpreterEnv Ancestor(int distance)
    {
        InterpreterEnv environment = this;

        for (int i = 0; i < distance; i++)
        {
            if (environment.Enclosing == null)
            {
                throw new Exception($"Invalid scope access at distance {distance}.");
            }
            environment = environment.Enclosing;
        }
        return environment;
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

    public List<Type> Get(Token name)
    {
        if (IsDeclared(name.Lexeme)) return values[name.Lexeme];

        else if (Enclosing != null) return Enclosing.Get(name);

        else throw new Exception($"Undefined Variable '{name.Lexeme}' at line {name.Line}.");
    }

    public void Assign(Token name, List<Type> value)
    {
        if (IsDeclared(name.Lexeme))
        {
            values[name.Lexeme] = value;
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