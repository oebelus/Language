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
            function bool isPrime(num a) {
                if (a < 2) {
                    return false;
                }
                else {
                    num i = 2;
                    while (i * i <= a) {
                        if (a % i == 0) {
                            return false;
                        }
                        i = i + 1;
                    }
                }
                return true;
            }

            out isPrime(23);
            ";

            string code_2 =
            @"
            num x = 5;
            x = ""Hi"";

            out x;
            ";

            Scanner _ = new(code_1);

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Console.WriteLine();

            Pratt parser = new(tokens);

            List<Statement> statements = parser.Parse();

            AstPrinter astPrinter = new();
            foreach (var statement in statements)
            {
                astPrinter.PrintStatement(statement);
            }

            // Console.WriteLine();
            // foreach (var statement in statements)
            // {
            //     Console.WriteLine(statement);
            // }

            // Console.WriteLine();
            // TypeChecker typeChecker = new();
            // typeChecker.TypeCheck(statements);

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
