using Bnf;

Console.WriteLine("=== BNF.NET Interactive Demo ===");
Console.WriteLine("This is an interactive REPL for testing ABNF grammars.\n");

// Define a default ABNF grammar for basic arithmetic expressions
var defaultGrammar = """
    expr = term *( ("+" / "-") term )
    term = factor *( ("*" / "/") factor )
    factor = number / "(" expr ")"
    number = 1*DIGIT
    DIGIT = %x30-39
    """;

Console.WriteLine("Default ABNF Grammar (arithmetic expressions):");
Console.WriteLine(defaultGrammar);

try
{
    // Parse the ABNF grammar
    Console.WriteLine("Parsing ABNF grammar...");
    var grammar = Abnf.Parse(defaultGrammar);
    Console.WriteLine($"✓ Successfully loaded grammar with {grammar.Rules.Count} rules");
    Console.WriteLine();

    // Display available rules
    Console.WriteLine("Available rules:");
    foreach (var rule in grammar.Rules)
    {
        Console.WriteLine($"  - {rule.Name}");
    }
    Console.WriteLine();

    // Interactive mode
    Console.WriteLine("Interactive mode - Enter expressions to validate against 'expr' rule");
    Console.WriteLine("Commands:");
    Console.WriteLine("  :quit or empty line - exit");
    Console.WriteLine("  :rule <name> - change the start rule (default: expr)");
    Console.WriteLine("  :grammar - display the current grammar");
    Console.WriteLine("  :load <abnf> - load a new grammar (multiline, end with empty line)");
    Console.WriteLine();

    string startRule = "expr";

    while (true)
    {
        Console.Write($"[{startRule}]> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            break;

        // Handle commands
        if (input.StartsWith(":"))
        {
            var parts = input.Split(' ', 2);
            var command = parts[0].ToLower();

            switch (command)
            {
                case ":quit":
                case ":q":
                case ":exit":
                    return 0;

                case ":rule":
                    if (parts.Length > 1)
                    {
                        var newRule = parts[1].Trim();
                        if (grammar.TryGetRule(newRule, out _))
                        {
                            startRule = newRule;
                            Console.WriteLine($"✓ Start rule changed to '{startRule}'");
                        }
                        else
                        {
                            Console.WriteLine($"✗ Rule '{newRule}' not found in grammar");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Current start rule: {startRule}");
                    }
                    break;

                case ":grammar":
                case ":g":
                    Console.WriteLine("\nCurrent grammar:");
                    Console.WriteLine(defaultGrammar);
                    break;

                case ":load":
                    Console.WriteLine("Enter ABNF grammar (empty line to finish):");
                    var lines = new List<string>();
                    while (true)
                    {
                        var line = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            break;
                        lines.Add(line);
                    }
                    
                    if (lines.Any())
                    {
                        defaultGrammar = string.Join('\n', lines);
                        try
                        {
                            grammar = Abnf.Parse(defaultGrammar);
                            Console.WriteLine($"✓ Successfully loaded new grammar with {grammar.Rules.Count} rules");
                            
                            // Reset start rule if it doesn't exist in new grammar
                            if (!grammar.TryGetRule(startRule, out _))
                            {
                                startRule = grammar.Rules.First().Name;
                                Console.WriteLine($"  Start rule reset to '{startRule}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"✗ Error loading grammar: {ex.Message}");
                        }
                    }
                    break;

                case ":help":
                case ":h":
                case ":?":
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  :quit, :q, :exit - exit the program");
                    Console.WriteLine("  :rule <name> - change the start rule");
                    Console.WriteLine("  :grammar, :g - display the current grammar");
                    Console.WriteLine("  :load - load a new grammar");
                    Console.WriteLine("  :help, :h, :? - show this help");
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Type :help for available commands");
                    break;
            }
            continue;
        }

        // Validate the input
        if (grammar.TryValidate(input, startRule, out var errorPos, out var errorMsg))
        {
            Console.WriteLine($"✓ Valid");
        }
        else
        {
            Console.WriteLine($"✗ Invalid");
            Console.WriteLine($"  Position {errorPos}: {errorMsg}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

Console.WriteLine("\nGoodbye!");
return 0;
