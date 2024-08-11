class Return(object value) : Exception()
{
    public object Value { get; } = value;
}