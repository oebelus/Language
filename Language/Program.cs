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
            let a = 5;
            let b = 5;
            let c = 7;

            if (a == b) c = 23;
            else c = 69;
            ";

            /*
                PUSH 5
                PUSH 0
                GSTORE

                PUSH 5
                PUSH 1
                GSTORE

                PUSH 7
                PUSH 2
                GSTORE

                PUSH 0
                GLOAD

                PUSH 1
                GLOAD

                EQ

                PUSH 1
                PUSH 2
                GSTORE

                CJUMP <LOC>

                HALT

                LOC:
                PUSH 0
                PUSH 2
                GSTORE
            */

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
