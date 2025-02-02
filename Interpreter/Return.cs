class Return(object? value) : Exception()
{
    public object? Value { get; } = value;
}

class Break : Exception
{
    public Break() : base() { }
}

class Continue : Exception
{
    public Continue() : base() { }
}