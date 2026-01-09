using Abnf;

// Check for whitespace mode argument
var allowWhitespace = args.Length > 0 && args[0].ToLower() is "ws" or "whitespace" or "--whitespace";

Console.WriteLine("=== ABNF.NET Demo ===");
Console.WriteLine($"Mode: {(allowWhitespace ? "Allow Whitespace in input" : "No whitespace in input allowed")}\n");

// Define ABNF grammar - with or without whitespace support
var abnfGrammar = allowWhitespace switch
{
    true => """
        expr = term *( OWS ("+" / "-") OWS term )
        term = factor *( OWS ("*" / "/") OWS factor )
        factor = [sign] number / "(" OWS expr OWS ")"
        number = 1*DIGIT
        sign = "+" / "-"
        DIGIT = %x30-39
        OWS = *( SP / HTAB )
        SP = %x20
        HTAB = %x09
        """,
    false => """
        expr = term *( ("+" / "-") term )
        term = factor *( ("*" / "/") factor )
        factor = [sign] number / "(" expr ")"
        number = 1*DIGIT
        sign = "+" / "-"
        DIGIT = %x30-39
        """
};

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
    Console.WriteLine($"(Validating against start rule: '{grammar.FirstRule?.Name}')\n");

    var examples = new[] { "123", "+42", "-7", "1+2", "(1+2)*3", "+5*-3", "( 1 + 2 ) * 3", "1+", "(1+2", "%4", };
    foreach (var example in examples)
    {
        var result = grammar.Validate(example);
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

    if (allowWhitespace)
    {
        Console.WriteLine("\nKey insight: ABNF requires explicit whitespace handling.");
        Console.WriteLine("The OWS rule allows optional spaces and tabs around operators.");
    }
    else
    {
        Console.WriteLine("\nNote: This grammar does NOT allow whitespace around operators.");
        Console.WriteLine("Run with 'ws' argument to see whitespace-aware version.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

return 0;

