using Bnf.Parsing;
using Bnf.Conversion;
using Bnf.Grammar;

namespace bnf.net.Tests;

public class DebugValidationTests
{
    [Fact]
    public void Debug_SimpleNumber()
    {
        // Simplest case: just a number
        var abnf = "number = 1*DIGIT\nDIGIT = %x30-39";
        var grammar = CreateGrammar(abnf);

        var isValid = grammar.TryValidate("123", "number", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void Debug_FactorIsNumber()
    {
        // Add factor layer
        var abnf = @"
factor = number
number = 1*DIGIT
DIGIT = %x30-39
";
        var grammar = CreateGrammar(abnf);

        var isValid = grammar.TryValidate("123", "factor", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void Debug_TermWithoutRepetition()
    {
        // Term without the repetition
        var abnf = @"
term = factor
factor = number
number = 1*DIGIT
DIGIT = %x30-39
";
        var grammar = CreateGrammar(abnf);

        var isValid = grammar.TryValidate("123", "term", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void Debug_TermWithEmptyRepetition()
    {
        // Term with repetition that should match zero times
        var abnf = @"
term = factor *( ""*"" factor )
factor = number
number = 1*DIGIT
DIGIT = %x30-39
";
        var grammar = CreateGrammar(abnf);

        var isValid = grammar.TryValidate("123", "term", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void Debug_TermWithOneRepetition()
    {
        // Term with repetition that matches once
        var abnf = """

            term = factor *( "*" factor )
            factor = number
            number = 1*DIGIT
            DIGIT = %x30-39

            """;
            
        var grammar = CreateGrammar(abnf);

        var isValid = grammar.TryValidate("2*3", "term", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Failed at position {errorPos}: {errorMsg}");
    }

    private Bnf.Grammar.Grammar CreateGrammar(string abnf)
    {
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        return AstToGrammarConverter.ToGrammar(ast);
    }
}
