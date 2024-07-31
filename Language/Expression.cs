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
        public readonly Expr Left = left;
        public readonly Token Operation = operation;
        public readonly Expr Right = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public class Unary(Expr right, Token operation) : Expr
    {
        public readonly Expr Right = right;
        public readonly Token Operation = operation;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

    public class Literal(object value) : Expr
    {
        public readonly object Value = value;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public class Grouping(Expr expression) : Expr
    {
        public readonly Expr Expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }
}