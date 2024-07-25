namespace vm
{
    class Program
    {
        public static void Main()
        {
            // byte[] program = [0, 0, 0, 0, 25,
            // 0, 0, 0, 0, 25,
            // ];

            // var mn = Mnemonics.Mnemonic("PUSH 5 PUSH 7 CALL <add> PUSH 4 PUSH 8 CALL <sub> HALT </add> ADD RET </sub> SUB RET");

            // VirtualMachine vm = new(mn);

            // vm.Execute();

            // vm.Logger();

            //             string code = @"Main:
            //     Add 5, 7
            //     Sub 4, 8
            // Add: x, y -> x + y
            // Sub: x, y -> x - y
            //             ";

            // List<Function> list = Parse.ParseFunctions("Main: \n  \nAdd: x, y -> x + y;");

            // "Main: \n\tAdd 5, 7\n\tSub 4, 8\nAdd: x, y -> x + y\nSub: x, y -> x - y"

            // string program = Parse.ToByteCode(code);

            // Console.WriteLine(program);

            // var mne = Mnemonics.Mnemonic(program);

            // max of 4 8

            var mn = Mnemonics.Mnemonic("PUSH -23");

            string str = "PUSH 4 PUSH 8 CALL <func> HALT </func> PUSH 0 STORE PUSH 1 STORE PUSH 0 LOAD PUSH 1 LOAD GT PUSH 53 CJUMP PUSH 0 LOAD RET";
            string[] index = str.Split(' ');
            int inc = 0;

            for (int i = 0; i < index.Length; i++)
            {
                if (int.TryParse(index[i], out _)) inc += 4;
                else
                {
                    if (index[i].StartsWith("</")) inc += 4;
                    else inc += 1;
                }
            }
            Console.WriteLine(inc);

            VirtualMachine vm = new(mn);

            vm.Execute();

            vm.Logger();

        }
    }
}

/*
Main: 
    Add 5, 7
    Sub 4, 9
    
Add: x, y -> x + y;
Sub: x, y -> x - y;
*/

/* CODES:

Equality: PUSH 5 PUSH 5 EQ PUSH 23 CJUMP PUSH 0 HALT PUSH 1 HALT
Even: PUSH 2 PUSH 7 MOD PUSH 23 CJUMP PUSH 1 HALT PUSH 0 HALT
Max: PUSH 23 PUSH 7 PUSH 4 PUSH 69 CALL <func> CALL <func> CALL <func> HALT </func> PUSH 0 STORE PUSH 1 STORE PUSH 0 LOAD PUSH 1 LOAD GT PUSH 74 CJUMP PUSH 0 LOAD RET PUSH 1 LOAD RET
Min: PUSH 23 PUSH 7 PUSH 4 PUSH 0 CALL <func> CALL <func> CALL <func> HALT </func> PUSH 0 STORE PUSH 1 STORE PUSH 0 LOAD PUSH 1 LOAD GT PUSH 74 CJUMP PUSH 1 LOAD RET PUSH 0 LOAD RET

*/
