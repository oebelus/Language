abstract class Statement
{
    public interface IVisitor<T>
    {
        T? VisitBlock(Block Statement);
        T? VisitFunction(Function Statement);
        T? VisitIf(If Statement);
        T? VisitLog(Log Statement);
        T? VisitWhile(While Statement);
        T? VisitReturn(Return Statement);
        T? VisitExpression(Expression Statement);
        T? VisitVariableStatement(VariableStatement variable);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Block(List<Statement> statements) : Statement
    {

        public readonly List<Statement> Statements = statements;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBlock(this)!;
        }
    }

    public class Expression(Expr expression) : Statement
    {

        public readonly Expr expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExpression(this)!;
        }
    }

    public class Function(Token name, List<Token> args, List<Statement> body) : Statement
    {

        public readonly Token Name = name;
        public readonly List<Token> Args = args;
        public readonly List<Statement> Body = body;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFunction(this)!;
        }
    }

    public class If(Expr condition, Statement thenBlock, Statement elseBlock) : Statement
    {

        public readonly Expr Condition = condition;
        public readonly Statement ThenBlock = thenBlock;
        public readonly Statement ElseBlock = elseBlock;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitIf(this)!;
        }
    }

    public class Log(Expr expression) : Statement
    {

        public readonly Expr expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLog(this)!;
        }
    }

    public class While(Expr condition, Statement body) : Statement
    {

        public readonly Expr Condition = condition;
        public readonly Statement Body = body;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitWhile(this)!;
        }
    }

    public class Return(Token keyword, Expr expression) : Statement
    {
        public readonly Token Keyworkd = keyword;
        public readonly Expr expression = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitReturn(this)!;
        }
    }

    public class VariableStatement(Token name, Expr initializer) : Statement
    {
        public readonly Token name = name;
        public readonly Expr initializer = initializer;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableStatement(this)!;
        }
    }
}