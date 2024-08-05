namespace Language
{
    class Program
    {
        public static void Main()
        {

            /*
            var x = stdout(9);
            var result = stdout(func(23))/20)
            */
            Scanner _ = new("log \"hello\"; log 5/20; var x = \"55\"; log x");

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
