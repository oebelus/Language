abstract class Expr
{
    public interface IVisitor<T>
    {
        T? VisitBinary(Binary expression);
        T? VisitUnary(Unary expression);
        T? VisitLiteral(Literal expression);
        T? VisitGrouping(Grouping expression);
        T? VisitAssign(Assign expression);
        T? VisitVariableExpression(VariableExpression expression);
        T? VisitLogical(Logical expression);
        T? VisitCall(Call expression);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Binary(Expr left, Token operation, Expr right) : Expr
    {
        public readonly Expr Left = left;
        public readonly Token Operation = operation;
        public readonly Expr Right = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinary(this)!;
        }
    }

    public class Logical(Expr left, Token operation, Expr right) : Expr
    {
        public readonly Expr Left = left;
        public readonly Token Operation = operation;
        public readonly Expr Right = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLogical(this)!;
        }
    }

    public class Unary(Expr right, Token operation) : Expr
    {
        public readonly Expr Right = right;
        public readonly Token Operation = operation;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnary(this)!;
        }
    }

    public class Literal(object? value) : Expr
    {
        public readonly object? Value = value;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this)!;
        }
    }

    public class Grouping(Expr expression) : Expr
    {
        public readonly Expr Expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this)!;
        }
    }

    public class Assign(Token name, Expr value) : Expr
    {
        public readonly Token Name = name;
        public readonly Expr Value = value;
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssign(this)!;
        }
    }

    public class VariableExpression(Token name) : Expr
    {
        public readonly Token Name = name;
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this)!;
        }
    }

    public class Call(Expr callee, Token paren, List<Expr> arguments) : Expr
    {
        public readonly Expr Callee = callee;
        public readonly Token Paren = paren;
        public readonly List<Expr> Arguments = arguments;
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitCall(this)!;
        }
    }
}