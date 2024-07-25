class Function(char[] operations, int arguments, string name)
{
    public char[] Operations { get; } = operations;
    public int Arguments { get; } = arguments;
    public string Name { get; } = name;

    public void Print()
    {
        Console.Write($"Name: {Name}, Arguments: {Arguments}, Operations: {string.Join(' ', Operations)}\n");
    }

}