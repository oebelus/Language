abstract class Expr
{
    class Binary(Expr left, Token operation, Expr right) : Expr
    {
        private readonly Expr Left = left;
        private readonly Token Operation = operation;
        private readonly Expr Right = right;
    }

    class Unary(Expr right, Token operation) : Expr
    {
        private readonly Expr Right = right;
        private readonly Token Operation = operation;
    }

    class Literal(object value) : Expr
    {
        private readonly object Value = value;
    }

    class Grouping(Expr expression) : Expr
    {
        private readonly Expr Expression = expression;
    }
}