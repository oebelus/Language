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
                foreach (var snippet in CodeSnippets.Snippets)
                {
                    Console.WriteLine(snippet);
                    try
                    {
                        RunCode(snippet);
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ResetColor();
                    }
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

            RDParser rDParser = new(tokens);

            Pratt parser = new(tokens);

            List<Statement> statements = rDParser.Parse();

            // AstPrinter astPrinter = new();
            // foreach (var statement in statements)
            // {
            //     astPrinter.PrintStatement(statement);
            // }

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

            // var mnemonics = Mnemonics.Mnemonic(mnemo);

            // VirtualMachine virtualMachine = new(mnemonics);

            // Console.WriteLine();

            // List<string> toMnemo = Utils.ByteCodeToMnemonics(mnemonics);

            // foreach (var item in toMnemo)
            // {
            //     Console.WriteLine(item);
            // }

            // VirtualMachine vm = new(mnemonics);

            // vm.Execute();

            // vm.Logger();

            scanner.Reset();
            interpreter.Reset();
            compiler.Reset();
        }
        // LanguageTest test = new();
        // test.RunTests();

    }
}
