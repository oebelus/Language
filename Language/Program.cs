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
                if (a < 2) {
                    log (a + "" is not a prime"");
                    return false;
                }
                else {
                    let i = 2;
                    while (i * i <= a) {
                        if (a % i == 0) {
                            log (a + "" is not a prime"");
                            return false;
                        }
                        i = i + 1;
                    }
                }
                log (a + "" is a prime"");
                return true;
            }

            isPrime(23);
            ";

            string code_2 =
            @"
            let a = 5;
            for (let i = 0; i < 10; i = i + 1) {
                if (i == 9) {
                    a = 7;
                }
            }
            ";

            string code_3 =
            @"
            num x = 11;
            
            function bool isEven(num x) {
                if (x % 2 == 0) {
                    return true;
                }
                else {
                    return false;
                }
            }

            isEven(true);
            isEven(5);
            ";

            Scanner _ = new(code_3);

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
            TypeChecker typeChecker = new();
            typeChecker.TypeCheck(statements);

            Interpreter interpreter = new();
            interpreter.Interpret(statements);

            Console.WriteLine();

            // Compiler compiler = new();
            // string mnemo = compiler.Compile(statements);
            // Console.WriteLine(mnemo);

        }
    }
}
