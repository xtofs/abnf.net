using Bnf.Parsing;
using Bnf.Conversion;

Console.WriteLine("=== BNF.NET Demo ===");
Console.WriteLine("This demo shows parsing ABNF grammar and validating strings against it.\n");

// Define a simple ABNF grammar for basic arithmetic expressions
var abnfGrammar = @"
expr = term *( (""+"" / ""-"") term )
term = factor *( (""*"" / ""/"") factor )
factor = number / ""("" expr "")""
number = 1*DIGIT
DIGIT = %x30-39
";

Console.WriteLine("ABNF Grammar:");
Console.WriteLine(abnfGrammar);
Console.WriteLine();

try
{
    // Parse the ABNF grammar
    Console.WriteLine("Step 1: Parsing ABNF grammar...");
    var tokens = Scanner.Scan(abnfGrammar);
    var parser = new Parser(tokens);
    var ast = parser.ParseRuleList();
    Console.WriteLine($"        ✓ Successfully parsed {ast.Rules.Count} rules");

    // Convert AST to Grammar for validation
    Console.WriteLine("\nStep 2: Converting AST to Grammar representation...");
    var grammar = AstToGrammarConverter.ToGrammar(ast);
    Console.WriteLine($"        ✓ Grammar created with {grammar.Rules.Count} rules");

    // Validate example expressions
    Console.WriteLine("\nStep 3: Validating example expressions:");
    
    var examples = new[] { "123", "1+2", "(1+2)*3" };
    foreach (var example in examples)
    {
        if (grammar.TryValidate(example, "expr", out var errorPos, out var errorMsg))
        {
            Console.WriteLine($"        ✓ \"{example}\" → Valid");
        }
        else
        {
            Console.WriteLine($"        ✗ \"{example}\" → Error at position {errorPos}: {errorMsg}");
        }
    }

    Console.WriteLine("\nDemo completed successfully!");
    Console.WriteLine("\nFor comprehensive tests, see the test project.");
    Console.WriteLine("For interactive testing, run: dotnet run --project samples/bnf.net.Interactive");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;

