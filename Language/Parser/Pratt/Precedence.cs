class Binding
{
    public enum Precedence
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