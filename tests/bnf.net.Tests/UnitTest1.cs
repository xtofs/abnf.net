using Bnf.Parsing;
using Bnf.Conversion;

namespace bnf.net.Tests;

public class ParserTests
{
    [Fact]
    public void ParseSimpleGrammar()
    {
        var abnf = "number = 1*DIGIT\nDIGIT = %x30-39";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        Assert.Equal(2, ast.Rules.Count);
        Assert.Equal("number", ast.Rules[0].Name);
        Assert.Equal("DIGIT", ast.Rules[1].Name);
    }

    [Fact]
    public void ParseArithmeticGrammar()
    {
        var abnf = @"
expr = term *( (""+"" / ""-"") term )
term = factor *( (""*"" / ""/"") factor )
factor = number / ""("" expr "")""
number = 1*DIGIT
DIGIT = %x30-39
";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        Assert.Equal(5, ast.Rules.Count);
    }
}

public class GrammarConversionTests
{
    [Fact]
    public void ConvertSimpleGrammar()
    {
        var abnf = "number = 1*DIGIT\nDIGIT = %x30-39";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        Assert.Equal(2, grammar.Rules.Count);
    }
}

public class ValidationTests
{
    private Bnf.Grammar.Grammar CreateSimpleNumberGrammar()
    {
        var abnf = "number = 1*DIGIT\nDIGIT = %x30-39";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        return AstToGrammarConverter.ToGrammar(ast);
    }

    private Bnf.Grammar.Grammar CreateArithmeticGrammar()
    {
        var abnf = @"
expr = term *( (""+"" / ""-"") term )
term = factor *( (""*"" / ""/"") factor )
factor = number / ""("" expr "")""
number = 1*DIGIT
DIGIT = %x30-39
";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        return AstToGrammarConverter.ToGrammar(ast);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("999")]
    public void ValidateSimpleNumbers_Success(string input)
    {
        var grammar = CreateSimpleNumberGrammar();
        var isValid = grammar.TryValidate(input, "number", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Expected success for '{input}' but got error at position {errorPos}: {errorMsg}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1a")]
    public void ValidateSimpleNumbers_Failure(string input)
    {
        var grammar = CreateSimpleNumberGrammar();
        var isValid = grammar.TryValidate(input, "number", out _, out _);
        Assert.False(isValid, $"Expected failure for '{input}' but got success");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1+2")]
    [InlineData("10*5+3")]
    [InlineData("(1+2)*3")]
    [InlineData("((1+2)*(3+4))/5")]
    public void ValidateArithmeticExpressions_Success(string input)
    {
        var grammar = CreateArithmeticGrammar();
        var isValid = grammar.TryValidate(input, "expr", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Expected success for '{input}' but got error at position {errorPos}: {errorMsg}");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1+")]
    [InlineData("(1+2")]
    [InlineData("")]
    [InlineData("+1")]
    [InlineData("1 + 2")] // spaces not allowed in grammar
    public void ValidateArithmeticExpressions_Failure(string input)
    {
        var grammar = CreateArithmeticGrammar();
        var isValid = grammar.TryValidate(input, "expr", out _, out _);
        Assert.False(isValid, $"Expected failure for '{input}' but got success");
    }

}

public class CharacterValueTests
{
    [Fact]
    public void ParseHexadecimalValue()
    {
        var abnf = "rule = %x41";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        var isValid = grammar.TryValidate("A", "rule", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Expected success but got error at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void ParseDecimalValue()
    {
        var abnf = "rule = %d65";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        var isValid = grammar.TryValidate("A", "rule", out var errorPos, out var errorMsg);
        Assert.True(isValid, $"Expected success but got error at position {errorPos}: {errorMsg}");
    }

    [Fact]
    public void ParseCharacterRange()
    {
        var abnf = "rule = %x41-5A"; // A-Z
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        Assert.True(grammar.TryValidate("A", "rule", out _, out _));
        Assert.True(grammar.TryValidate("M", "rule", out _, out _));
        Assert.True(grammar.TryValidate("Z", "rule", out _, out _));
        Assert.False(grammar.TryValidate("a", "rule", out _, out _));
        Assert.False(grammar.TryValidate("1", "rule", out _, out _));
    }

    [Fact]
    public void ParseCharacterConcatenation()
    {
        var abnf = "rule = %x41.42.43"; // ABC
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        Assert.True(grammar.TryValidate("ABC", "rule", out _, out _));
        Assert.False(grammar.TryValidate("AB", "rule", out _, out _));
        Assert.False(grammar.TryValidate("ABCD", "rule", out _, out _));
    }
}
