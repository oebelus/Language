class CodeSnippets
{
    static readonly string isPrimeCode = @"
function bool isPrime(int a) {
    if (a < 2) {
        return false;
    }
    else {
        int i = 2;
            if (a % i == 0) {
                return false;
            }
            i = i + 1;
    }
    return true;
}

println isPrime(23);
";

    static readonly string addCode = @"
function int add(int a, int b) {
    return a + b;
}

println add(5, 5);
";

    static readonly string subCode = @"
function int sub(int a, int b) { 
    return a - b; 
}

println sub(10, 5);
";

    static readonly string variableDeclarationCode = @"
int x = 42;
string name = ""John"";
bool isValid = true;
int pi = 3.14159;

println x;
println name;
println isValid;
println pi;
";

    static readonly string comparisonOperatorsCode = @"
int value = 42;
if (value >= 0 and value <= 100) {
    println ""Value is in range"";
}
";

    static readonly string complexExpressionCode = @"
int result = (5 + 3) * 2 - (10 / 2);
println result;
";

    static readonly string unaryOperatorsCode = @"
int negativeNum = -42;
bool notTrue = !true;

println negativeNum;
println notTrue;
";

    static readonly string functionCallWithArgumentsCode = @"
function string formatName(string a, string b) {
    return a + "" "" + b;
}

string fullName = formatName(""Jane"", ""Smith"");
println fullName;
";

    static readonly string nestedIfStatementsCode = @"
int one = -10;
int two = -6;
if (one > 0) {
    if (two > 0) {
        println ""Both positive"";
    } else {
        println ""Only the first is positive"";
    }
}
";

    static readonly string complexWhileWithBreakCode = @"
int number = 0;
while (true) {
    if (number >= 10) {
        break;
    }
    println number;
    number = number + 1;
}
";

    static readonly string complexWhileWithBreakAndContinueCode = @"
int number_x = 0;
while (true) {
    if (number_x % 2 == 0) {
        number_x = number_x + 1;
        continue;
    }
    if (number_x >= 10) {
        break;
    }
    println number_x;
    number_x = number_x + 1;
}
";

    static readonly string functionWithComplexLogicCode = @"
function int findMax(int a, int b, int c) {
    int max = a;
    if (b > max) {
        max = b;
    }
    if (c > max) {
        max = c;
    }
    return max;
}
";

    static readonly string nestedBlockScopesCode = @"
{
    int x = 1;
    {
        int y = 2;
        {
            int z = 3;
            print x + y + z;
        }
    }
}
";

    static readonly string complexExpressionWithMultipleOperatorsCode = @"
int complexCalc = (5 + 3 * 2) / (1 - 4 % 3) * -2;
println complexCalc;
";

    static readonly string multipleVariableDeclarationsCode = @"
int a = 1;
int b = 2;
int c = 3;
println a + b + c;
";

    static readonly string functionWithMultipleReturnsCode = @"
function int absoluteValue(int y) {
    if (y < 0) {
        return -y;
    }
    return y;
}

println absoluteValue(-42);
";

    static readonly string complexLogicalExpressionCode = @"
bool isError = true;
int w = 1;
int y = 2;
int z = 3;
bool valid = (w > 0 and y < 100) or (z == 0 and !isError);

println valid;
";

    static readonly string addFunctionCode = @"
function int add(int a, int b) {
    return a + b;
}

int sum = add(1, 2);
println(sum + 4);
";

    static readonly string showACode1 = @"
int a = 4;
{
  function void showA() {
    print a;
  }

  showA();
  int a = 5;
  showA();
}
";

    static readonly string showACode2 = @"
string a = ""global"";
{
    function void showA() {
        println(a);
    }

    showA();

    int a = 10;

    showA();
}
";

    static readonly string testFunctionScopeCode = @"
function void testFunctionScope() {
    int a = 10;
    function void innerFunction() {
        println(a); // Expected output: 10
    }
    innerFunction();
}
testFunctionScope();
";

    static readonly string testBlockScopeCode = @"
function void testBlockScope() {
    int a = 5;
    {
        int a = 10;
        println a; // Expected output: 10
    }
    println(a); // Expected output: 5
}
testBlockScope();
";

    static readonly string testNestedBlockScopeCode = @"
function void testNestedBlockScope() {
    int a = 1;
    {
        int b = 2;
        {
            int c = 3;
            println a + b + c; // Expected output: 6
        }
    }
}
testNestedBlockScope();
";

    static readonly string testGlobalLocalScopeCode = @"
int a = 20;
function void testGlobalLocalScope() {
    println a; // Expected output: 20
    int a = 30;
    println a; // Expected output: 30
}
testGlobalLocalScope();
println a; // Expected output: 20
";

    static readonly string testParameterScopeCode = @"
function void testParameterScope(int a) {
    println a; // Expected output: 50
}
testParameterScope(50);
";

    public readonly static string[] Snippets = [
        isPrimeCode,
        addCode,
        subCode,
        variableDeclarationCode,
        comparisonOperatorsCode,
        complexExpressionCode,
        unaryOperatorsCode,
        functionCallWithArgumentsCode,
        nestedIfStatementsCode,
        complexWhileWithBreakCode,
        complexWhileWithBreakAndContinueCode,
        functionWithComplexLogicCode,
        nestedBlockScopesCode,
        complexExpressionWithMultipleOperatorsCode,
        multipleVariableDeclarationsCode,
        functionWithMultipleReturnsCode,
        complexLogicalExpressionCode,
        addFunctionCode,
        showACode1,
        showACode2,
        testFunctionScopeCode,
        testBlockScopeCode,
        testNestedBlockScopeCode,
        testGlobalLocalScopeCode,
        testParameterScopeCode
    ];
}