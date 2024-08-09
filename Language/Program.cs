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

            /* string code =
            @"
            
            var a = 0;
            var temp;

            for (var b = 1; a < 1000; b = temp + b) {
                log a;
                temp = a;
                a = b;
            }

            "; */

            string code_1 = 
            @"
                function sayHi(first, last) {
                    log ""first: "" + first + "", last: "" + last;
                }
S
                sayHi(""Imane"", 23);
            ";

            Scanner _ = new(code_1);

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
