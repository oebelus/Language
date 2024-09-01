namespace Language.Typer
{
    public record Type
    {
        public override string ToString() => GetType().Name;
    }

    public record Number() : Type
    {
        public override string ToString() => GetType().Name;
    }

    public record String() : Type
    {
        public override string ToString() => GetType().Name;
    }

    public record Void() : Type
    {
        public override string ToString() => GetType().Name;
    }

    public record Boolean() : Type
    {
        public override string ToString() => GetType().Name;
    }

    public record Function(Type[] Arguments, Type Return) : Type
    {
        public override string ToString() => GetType().Name;
    }
}
