using Abnf;

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
    var grammar = global::Abnf.Abnf.Parse(abnfGrammar);
    Console.WriteLine($"        ✓ Successfully parsed {grammar.Rules.Count} rules");

    // Show the parsed grammar as S-expressions
    Console.WriteLine("\nStep 2: Grammar Structure (S-expressions):");
    Console.WriteLine(grammar.ToSExpression());

    // Validate example expressions
    Console.WriteLine("\nStep 3: Validating example expressions:");
    Console.WriteLine("(Using structured ValidationResult API with line/column support)\n");

    var examples = new[] { "123", "1+2", "(1+2)*3", "( 1 + 2 ) * 3", "1+", "(1+2", "%4", };
    foreach (var example in examples)
    {
        var result = grammar.Validate(example, "expr");
        if (result.IsSuccess)
        {
            Console.WriteLine($"        {$"\"{example}\"",-20} → 🟢 Valid");
        }
        else
        {
            
            var (line, column) = result.GetLineColumn(example);
            Console.WriteLine($"        {$"\"{example}\"",-20} → 🔴 Invalid ({line}, {column}): {result.ErrorMessage}");
        }
    }

    Console.WriteLine("\nDemo completed successfully!");
    Console.WriteLine("\nNote: This grammar does NOT allow whitespace around operators.");    
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;

