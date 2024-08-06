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

            string code =
            @"
            
            var a = ""global a"";
            var b = ""global b"";
            var c = ""global c"";
            {
                var a = ""outer a"";
                var b = ""outer b"";
                {
                    var a = ""inner a"";
                    log a;
                    log b;
                    log c;
                }
                log a;
                log b;
                log c;
            }
            log a;
            log b;
            log c;

            ";
            Scanner _ = new(code);

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Parser parser = new(tokens);

            List<Statement> statements = parser.Parse();

            Console.WriteLine();
            foreach (var statement in statements)
            {
                Console.WriteLine(statement);
            }

            Console.WriteLine();
            Interpreter interpreter = new();
            interpreter.Interpret(statements);
        }
    }
}
