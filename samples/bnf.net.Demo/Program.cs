using Bnf;

Console.WriteLine("=== BNF.NET Demo ===");
Console.WriteLine("This demo shows parsing ABNF grammar and validating strings against it.\n");

// Define a simple ABNF grammar for basic arithmetic expressions
var abnfGrammar = """
    expr = term *( ("+" / "-") term )
    term = factor *( ("*" / "/") factor )
    factor = number / "(" expr ")"
    number = 1*DIGIT
    DIGIT = %x30-39
    """;

Console.WriteLine("ABNF Grammar:");
Console.WriteLine(abnfGrammar);
Console.WriteLine();

try
{
    // Parse the ABNF grammar and convert to Grammar in one step
    Console.WriteLine("Step 1: Parsing ABNF grammar...");
    var grammar = Abnf.Parse(abnfGrammar);
    Console.WriteLine($"        ✓ Successfully parsed {grammar.Rules.Count} rules");

    // Validate example expressions
    Console.WriteLine("\nStep 2: Validating example expressions:");
    
    var examples = new[] { "123", "1+2", "(1+2)*3", "1+", "(1+2" };
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

