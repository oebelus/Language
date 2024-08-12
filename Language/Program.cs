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

            let a = 10;

            {
                let a = 5;
                let y = 7;
            }

            a = 23;
            
            ";

            /*

                PUSH 10
                PUSH ads
                STORE

                PUSH 7
                PUSH ads
                STORE

            */

            // string code_1 =
            // @"
            //     function sayHi(first, last) {
            //         return ""first: "" + first + "", last: "" + last;
            //     }

            //     log sayHi(""Imane"", ""23"");
            // ";

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
