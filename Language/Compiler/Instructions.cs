class Instruction
{
    public static Dictionary<TokenType, byte> operation = new()
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

    public static Dictionary<Instructions, byte> instruction = new()
    {
        {Instructions.PUSH, 0},
        {Instructions.POP, 1},
        {Instructions.LOAD, 18},
        {Instructions.GLOAD, 19},
        {Instructions.STORE, 20},
        {Instructions.GSTORE, 21},
    };
}