
class Environment
{
    readonly Environment Enclosing;

    // local scope nested inside an outer one
    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    // global scope’s environment
    public Environment()
    {
        Enclosing = null!;
    }

    private readonly Dictionary<string, object> values = [];

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public object Get(Token name)
    {
        if (IsVarDeclared(name)) return values[name.Lexeme];

        else if (Enclosing != null) return Enclosing.Get(name);

        else return "Undefined Variable '" + name.Lexeme + "'.";
    }

    public void Assign(Token name, object value)
    {
        if (IsVarDeclared(name))
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

    public bool IsVarDeclared(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out object? _)) return true;
        return false;
    }
}