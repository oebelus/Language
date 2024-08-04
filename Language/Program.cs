using System.Linq.Expressions;

namespace Language
{
    class Program
    {
        public static void Main()
        {
            Scanner _ = new("7*4-(5+4)");

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Expr expression = new Expr.Binary(
                new Expr.Unary(
                    new Expr.Literal(123),
                    new Token(TokenType.MINUS, "-", "null", 1)),
                new Token(TokenType.STAR, "*", "null", 1),
                new Expr.Grouping(
                    new Expr.Literal(45.67)));

            Parser parser = new(tokens);

            Expr expr = parser.Parse();

            Console.WriteLine(new AstPrinter().Print(expr));

            Interpreter interpreter = new();
            interpreter.Interpret(expr);
        }
    }
}
