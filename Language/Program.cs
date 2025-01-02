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
                foreach (string code in CodeSnippets.Snippets)
                {
                    Console.WriteLine(code);
                    RunCode(code);
                }
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

            Resolver resolver = new(interpreter);
            resolver.Resolve(statements);

            interpreter.Interpret(statements);

            Console.WriteLine();

            Compiler compiler = new();
            string mnemo = compiler.Compile(statements);
            Console.WriteLine(mnemo);

            var mnemonics = Mnemonics.Mnemonic(mnemo);

            VirtualMachine virtualMachine = new(mnemonics);

            Console.WriteLine();

            // List<string> toMnemo = Utils.ByteCodeToMnemonics(mnemonics);

            // foreach (var item in toMnemo)
            // {
            //     Console.WriteLine(item);
            // }

            // VirtualMachine vm = new(mnemonics);

            // vm.Execute();

            // vm.Logger();
        }
        // LanguageTest test = new();
        // test.RunTests();

    }
}
