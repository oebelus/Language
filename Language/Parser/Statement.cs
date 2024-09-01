using Type = Language.Typer.Type;

abstract class Statement
{
    public interface IVisitor
    {
        void VisitBlock(Block Statement);
        void VisitFunction(Function Statement);
        void VisitIf(If Statement);
        void VisitLog(Log Statement);
        void VisitWhile(While Statement);
        void VisitReturn(Return Statement);
        void VisitExpression(Expression Statement);
        void VisitVariableStatement(VariableStatement statement);
    }

    public abstract void Accept(IVisitor visitor);

    public class Block(List<Statement> statements) : Statement
    {

        public readonly List<Statement> Statements = statements;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitBlock(this);
        }
    }

    public class Expression(Expr expression) : Statement
    {
        public readonly Expr expression = expression;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitExpression(this);
        }
    }

    public class Function(Token name, Type type, List<Argument> args, List<Statement> body) : Statement
    {
        public readonly Token Name = name;
        public readonly Type Type = type;
        public readonly List<Argument> Args = args;
        public readonly List<Statement> Body = body;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitFunction(this);
        }
    }

    public class Argument(Type type, Token name)
    {
        public readonly Type Type = type;
        public readonly Token Name = name;
    }

    public class If(Expr condition, Statement thenBranch, Statement elseBranch) : Statement
    {
        public readonly Expr Condition = condition;
        public readonly Statement ThenBranch = thenBranch;
        public readonly Statement ElseBranch = elseBranch;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitIf(this);
        }
    }

    public class Log(Expr expression) : Statement
    {

        public readonly Expr expression = expression;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitLog(this);
        }
    }

    public class While(Expr condition, Statement body) : Statement
    {

        public readonly Expr Condition = condition;
        public readonly Statement Body = body;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitWhile(this);
        }
    }

    public class Return(Token keyword, Expr? value) : Statement
    {
        public readonly Token Keyworkd = keyword;
        public readonly Expr? Value = value;

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitReturn(this);
        }
    }

    public class VariableStatement : Statement
    {
        public readonly Type? Type;
        public readonly Token? Keyword;
        public readonly Token Name;
        public readonly Expr? Initializer;

        // Constructor with Type
        public VariableStatement(Type type, Token name, Expr? initializer)
        {
            Type = type;
            Name = name;
            Initializer = initializer;
        }

        // Constructor with Keyword
        public VariableStatement(Token keyword, Token name, Expr? initializer)
        {
            Keyword = keyword;
            Name = name;
            Initializer = initializer;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.VisitVariableStatement(this);
        }
    }
}