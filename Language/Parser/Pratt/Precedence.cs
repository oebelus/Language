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
        EQUALITY, // TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL
        COMPARISON,
        SHIFT,
        TERM,
        FACTOR,
        UNARY,
        CALL,
        PRIMARY
    }
}