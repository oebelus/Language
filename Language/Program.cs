namespace Language
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                RunCode(File.ReadAllText(args[0]));
            }
            else
            {
                RunCode(@"int x = 0;
for (int i = 1; i < 5; i = i + 1) {
    x = x + i;
    out x;
}");
            }
        }

        public static void RunCode(string code)
        {
            Scanner scanner = new(code);

            List<Token> tokens = scanner.ScanTokens();

            // foreach (var item in tokens)
            // {
            //     Token.TokenLogger(item);
            // }

            Console.WriteLine();

            RDParser rDParser = new(tokens);
            rDParser.Parse();

            Pratt parser = new(tokens);

            List<Statement> statements = parser.Parse();

            // AstPrinter astPrinter = new();
            // foreach (var statement in statements)
            // {
            //     astPrinter.PrintStatement(statement);
            // }

            Console.WriteLine();
            foreach (var statement in statements)
            {
                Console.WriteLine(statement);
            }

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
        // LanguageTest test = new();
        // test.RunTests();

    }
}
