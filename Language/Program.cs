using vm;

namespace Language
{
    class Program
    {
        public static void Main()
        {
            Scanner scanner = new("!*+-/=<> <= ==");

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }
        }
    }
}
