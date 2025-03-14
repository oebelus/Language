namespace Language.ICompiler;
using Instruction = stackVM.Instructions;

class CInstruction
{
    public static Dictionary<TokenType, string> COperation = new()
    {
        {TokenType.PLUS, "ADD"},
        {TokenType.MINUS, "SUB"},
        {TokenType.MOD, "MOD"},
        {TokenType.STAR, "MUL"},
        {TokenType.SLASH, "DIV"},
        {TokenType.LESS, "LT"},
        {TokenType.GREATER, "GT"},
        {TokenType.EQUAL_EQUAL, "EQ"},
        {TokenType.AND, "AND"},
        {TokenType.OR, "OR"},
        {TokenType.BANG, "NEG"},
    };

    public static Dictionary<Instruction, string> CInstructions = new()
    {
        {Instruction.PUSH, "PUSH"},
        {Instruction.PUSH_STR, "PUSH_STR"},
        {Instruction.PUSH_CHAR, "PUSH_CHAR"},
        {Instruction.POP, "POP"},
        {Instruction.LOAD, "LOAD"},
        {Instruction.GLOAD, "GLOAD"},
        {Instruction.STORE, "STORE"},
        {Instruction.GSTORE, "GSTORE"},
        {Instruction.RET, "RET"},
        {Instruction.CJUMP, "CJUMP"},
        {Instruction.JUMP, "JUMP"},
        {Instruction.EQ, "EQ"},
        {Instruction.GT, "GT"},
        {Instruction.LT, "LT"},
        {Instruction.NEG, "LT"},
        {Instruction.NOT, "NOT"},
        {Instruction.GSTORE_STR, "GSTORE_STR"},
    };
}

