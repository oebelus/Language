namespace Language
{
    class Program
    {
        public static void Main()
        {
            Scanner _ = new("log \"hello\";");

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Parser parser = new(tokens);

            List<Statement> statements = parser.Parse();

            foreach (var statement in statements)
            {
                Console.WriteLine(statement);
            }

            Interpreter interpreter = new();
            interpreter.Interpret(statements);
        }
    }
}
