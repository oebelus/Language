abstract class Expression
{
    static class Binary : Expression
    {
        private readonly Expression Left;
        private readonly Token Operator;
        private readonly Expression Right;

        Binary(Expression left, Token operator, Expression right)
        {
            this.Left = left;
            this.Operator = operator;
            this.Right = right;
        }
    }

    static class Unary : Expression
    {
        private readonly Expression Operand;
        private readonly Token Operator;

        Binary(Expression operand, Token operator)
        {
            this.Operand = operand;
            this.Operator = operator;
        }
    }

    static class Literal : Expression
    {
        private readonly Expression Operand;

        Binary(Expression operand)
        {
            this.Operand = operand;
        }
    }
}