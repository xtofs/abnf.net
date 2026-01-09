using Abnf;

Console.WriteLine("=== ABNF.NET Demo with Whitespace ===");
Console.WriteLine("This demo shows ABNF grammar with explicit whitespace handling.\n");

// Define ABNF grammar with optional whitespace (BWS = Bad WhiteSpace)
var abnfGrammar = """
    expr = term *( BWS ("+" / "-") BWS term )
    term = factor *( BWS ("*" / "/") BWS factor )
    factor = number / "(" BWS expr BWS ")"
    number = 1*DIGIT
    DIGIT = %x30-39
    BWS = *( SP / HTAB )
    SP = %x20
    HTAB = %x09
    """;

Console.WriteLine("ABNF Grammar (with optional whitespace):");
Console.WriteLine(abnfGrammar);
Console.WriteLine();

try
{
    // Parse the ABNF grammar and convert to Grammar in one step
    Console.WriteLine("Step 1: Parsing ABNF grammar...");
    var grammar = Abnf.Abnf.Parse(abnfGrammar);
    Console.WriteLine($"        ✓ Successfully parsed {grammar.Rules.Count} rules");

    // Validate example expressions (including ones with whitespace)
    Console.WriteLine("\nStep 2: Validating example expressions:");
    Console.WriteLine("(Now accepts whitespace around operators!)\n");

    var examples = new[]
    {
        "123",           // Simple number
        "1+2",           // No whitespace
        "1 + 2",         // With whitespace
        "(1+2)*3",       // No whitespace in complex expression
        "( 1 + 2 ) * 3", // With whitespace everywhere
        "(  1  +  2  )",  // Multiple spaces
        "1+",            // Invalid: incomplete
        "(1+2",          // Invalid: unclosed parenthesis
        "%4",            // Invalid: not a digit
    };

    foreach (var example in examples)
    {
        var result = grammar.Validate(example, "expr");
        if (result.IsSuccess)
        {
            Console.WriteLine($"        ✅ \"{example}\" → Valid");
        }
        else
        {
            var (line, column) = result.GetLineColumn(example);
            Console.WriteLine($"        ❌ \"{example}\"  → Invalid");
            Console.WriteLine($"           ({line}, {column}): {result.ErrorMessage}");
        }
    }

    Console.WriteLine("\nDemo completed successfully!");
    Console.WriteLine("\nKey insight: ABNF requires explicit whitespace handling.");
    Console.WriteLine("Compare with the basic demo (abnf.net.Demo) which doesn't allow whitespace.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;
