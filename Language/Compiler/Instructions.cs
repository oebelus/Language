enum Instructions : byte
{
    PUSH,
    POP,

    ADD,
    SUB,
    MUL,
    DIV,
    NEG,
    EXP,
    MOD,
    LT,
    GT,
    EQ,

    AND,
    OR,
    NOT,
    XOR,
    LS,
    RS,

    LOAD, // load local val or fct arg
    GLOAD,
    STORE,
    GSTORE,

    JUMP,
    CJUMP,

    CALL,
    RET,

    HALT
}

class Instruction
{
    public static Dictionary<TokenType, byte> instruction = new()
    {
        {TokenType.PLUS, 2},
        {TokenType.MINUS, 3},
        {TokenType.STAR, 4},
        {TokenType.SLASH, 5},
        {TokenType.LESS, 9},
        {TokenType.GREATER, 10},
        {TokenType.EQUAL_EQUAL, 11},
        {TokenType.AND, 12},
        {TokenType.OR, 13},
        {TokenType.BANG, 14},
    };
}