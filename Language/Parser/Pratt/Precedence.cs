class Precedence
{
    public enum Binding
    {
        //COMMA,
        NONE,
        ASSIGNMENT,
        CONDITIONAL,
        OR,
        AND,
        EQUALITY,
        COMPARISON,
        SHIFT,
        TERM,
        FACTOR,
        UNARY,
        CALL,
        PRIMARY
    }
}