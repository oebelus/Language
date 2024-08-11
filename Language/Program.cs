﻿namespace Language
{
    class Program
    {
        public static void Main()
        {

            /*
            let x = stdout(9);
            let result = stdout(func(23))/20)
            */

            /* string code =
            @"
            
            let a = 0;
            let temp;

            for (let b = 1; a < 1000; b = temp + b) {
                log a;
                temp = a;
                a = b;
            }

            "; */

            /* string code_1 =
            @"
                function sayHi(first, last) {
                    log ""first: "" + first + "", last: "" + last;
                }
S
                sayHi(""Imane"", ""23"");
            "; */

            Scanner _ = new("a + 10");

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
            Compiler compiler = new();
            compiler.Compile(statements);

            List<byte> code = compiler.ByteCode;

            Console.Write("[ ");
            foreach (var item in code)
            {
                Console.Write(item + " ");
            }
            Console.Write("]");
        }
    }
}
