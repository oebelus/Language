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

            // string code_1 =
            // @"
            // function bool isPrime(num a) {
            //     if (a < 2) {
            //         return false;
            //     }
            //     else {
            //         let i = 2;
            //         while (i * i <= a) {
            //             if (a % i == 0) {
            //                 return false;
            //             }
            //             i = i + 1;
            //         }
            //     }
            //     return true;
            // }

            // log isPrime(23);
            // ";

            // string code_2 =
            // @"
            // num x = 5;
            // x = ""Hi"";

            // log x;
            // ";

            Scanner _ = new("1+5*10");

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }

            Console.WriteLine();

            PParser parser = new(tokens);

            parser.Parse();

            Console.WriteLine();

            AstPrinter astPrinter = new();

            Console.WriteLine(astPrinter.Print(parser.Parse()));

            // Console.WriteLine();
            // foreach (var statement in statements)
            // {
            //     Console.WriteLine(statement);
            // }

            // Console.WriteLine();
            // TypeChecker typeChecker = new();
            // typeChecker.TypeCheck(statements);

            // Interpreter interpreter = new();
            // interpreter.Interpret(statements);

            // Console.WriteLine();

            // Compiler compiler = new();
            // string mnemo = compiler.Compile(statements);
            // Console.WriteLine(mnemo);

        }
    }
}
