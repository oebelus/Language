class Instruction
{
    public static Dictionary<TokenType, string> operation = new()
    {
        {TokenType.PLUS, "ADD"},
        {TokenType.MINUS, "SUB"},
        {TokenType.STAR, "MUL"},
        {TokenType.SLASH, "DIV"},
        {TokenType.LESS, "LT"},
        {TokenType.GREATER, "GT"},
        {TokenType.EQUAL_EQUAL, "EQ"},
        {TokenType.AND, "AND"},
        {TokenType.OR, "OR"},
        {TokenType.BANG, "NEG"},
    };

    public static Dictionary<Instructions, string> instruction = new()
    {
        {Instructions.PUSH, "PUSH"},
        {Instructions.POP, "POP"},
        {Instructions.LOAD, "LOAD"},
        {Instructions.GLOAD, "GLOAD"},
        {Instructions.STORE, "STORE"},
        {Instructions.GSTORE, "GSTORE"},
        {Instructions.RET, "RET"},
        {Instructions.CJUMP, "CJUMP"},
        {Instructions.JUMP, "JUMP"},
    };
}