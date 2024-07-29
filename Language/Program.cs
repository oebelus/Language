using vm;

namespace Language
{
    class Program
    {
        public static void Main()
        {
            Scanner _ = new("!*+-/\"Hello this is a string!\"54618.5457 =< var if   dffgd> \n <= ==54898.111 \"sedsdfsdf\"");

            List<Token> tokens = Scanner.ScanTokens();

            foreach (var item in tokens)
            {
                Token.TokenLogger(item);
            }
        }
    }
}
