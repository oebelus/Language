abstract class Expr
{
    public interface IVisitor<T>
    {
        T VisitBinary(Binary expression);
        T VisitUnary(Unary expression);
        T VisitLiteral(Literal expression);
        T VisitGrouping(Grouping expression);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Binary(Expr left, Token operation, Expr right) : Expr
    {
        private readonly Expr Left = left;
        private readonly Token Operation = operation;
        private readonly Expr Right = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public class Unary(Expr right, Token operation) : Expr
    {
        private readonly Expr Right = right;
        private readonly Token Operation = operation;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

    public class Literal(object value) : Expr
    {
        private readonly object Value = value;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public class Grouping(Expr expression) : Expr
    {
        private readonly Expr Expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }
}