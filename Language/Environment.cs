class Environment {
    private readonly Dictionary<string, object> values = [];

    public void Define(string name, object value) {
        values.Add(name, value);
    }

    public object Get(Token name) {
        if (values.TryGetValue(name.Lexeme, out object? value)) return value;
        else {
            Console.WriteLine("Undefined Variable '" + name.Lexeme + "'.");
            return "Undefined Variable '" + name.Lexeme + "'.";
        };
    }
}