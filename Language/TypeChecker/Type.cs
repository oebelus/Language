namespace Language.TypeChecker {
    public record Type { }

    public record Number() : Type;

    public record Void() : Type;

    public record Boolean() : Type;
    
    public record Function(Type[] Arguments, Type Return) : Type;
}
