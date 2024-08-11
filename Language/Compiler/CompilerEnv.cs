
class CompilerEnv
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

    public object Get(Token name)
    {
        if (IsDeclared(name)) return addresses[name.Lexeme];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return "Undefined Variable '" + name.Lexeme + "'.";
    }

    public void Assign(Token name, object value)
    {
        if (IsDeclared(name))
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

    public bool IsDeclared(Token name)
    {
        if (addresses.TryGetValue(name.Lexeme, out object? _)) return true;
        return false;
    }
}