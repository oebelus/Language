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
            function isPrime(a) {
                if (a < 2) {return false;}
                else {
                    let i = 2;
                    while (i < a) {
                        if (a % i == 0) {return false;}
                        i = i + 1;
                    }
                }
                return true;
            }

            isPrime(23);
            ";

            string code_2 =
            @"
            function isOdd(a) {
                if (a % 2 == 0) return false;
                else return true;
            }

            isOdd(23);
            ";

            Scanner _ = new(code_1);

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Parser parser = new(tokens);

            List<Statement> statements = parser.Parse();

            // Console.WriteLine();
            // foreach (var statement in statements)
            // {
            //     Console.WriteLine(statement);
            // }

            // Console.WriteLine();

            // Interpreter interpreter = new();
            // interpreter.Interpret(statements);

            Console.WriteLine();

            Compiler compiler = new();
            string mnemo = compiler.Compile(statements);
            Console.WriteLine(mnemo);

        }
    }
}
