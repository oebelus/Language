using vm;

namespace Language
{
    class Program
    {
        public static void Main()
        {

            /*
            let x = stdout(9);
            let result = stdout(func(23))/20)
            */

            string code_1 =
            @"
            let a = 0;
            let b = 1;
            let c = 0;

            if (a < b) c = 99;
            else c = 55;
            
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

            Console.WriteLine();

            Compiler compiler = new();
            string mnemo = compiler.Compile(statements);
            Console.WriteLine(mnemo);

        }
    }
}
