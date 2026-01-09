using Bnf.Parsing;
using Bnf.Conversion;
using Bnf;

namespace bnf.net.Tests;

/// <summary>
/// Common ABNF grammar definitions used across tests
/// </summary>
public static class TestGrammars
{
    public const string SimpleNumber = """
        number = 1*DIGIT
        DIGIT = %x30-39
        """;
    
    public const string Arithmetic = """
        expr = term *( ("+" / "-") term )
        term = factor *( ("*" / "/") factor )
        factor = number / "(" expr ")"
        number = 1*DIGIT
        DIGIT = %x30-39
        """;

    public const string HexValue = "rule = %x41";
    public const string DecimalValue = "rule = %d65";
    public const string CharacterRange = "rule = %x41-5A"; // A-Z
    public const string CharacterConcatenation = "rule = %x41.42.43"; // ABC
}

public class ParserTests
{
    [Fact]
    public void ParseSimpleGrammar()
    {
        var tokens = Scanner.Scan(TestGrammars.SimpleNumber);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        Assert.Equal(2, ast.Rules.Count);
        Assert.Equal("number", ast.Rules[0].Name);
        Assert.Equal("DIGIT", ast.Rules[1].Name);
    }

    [Fact]
    public void ParseArithmeticGrammar()
    {
        var tokens = Scanner.Scan(TestGrammars.Arithmetic);
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
        var tokens = Scanner.Scan(TestGrammars.SimpleNumber);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        var grammar = AstToGrammarConverter.ToGrammar(ast);

        Assert.Equal(2, grammar.Rules.Count);
    }
}

/// <summary>
/// Test data for valid grammar validation cases
/// </summary>
public class ValidInputTestData : TheoryData<string, string, string, string>
{
    public ValidInputTestData()
    {
        // Grammar Name, Grammar, Rule Name, Input
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "1");
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "123");
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "0");
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "999");
        
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "123");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "1+2");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "10*5+3");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "(1+2)*3");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "((1+2)*(3+4))/5");
        
        Add("HexValue", TestGrammars.HexValue, "rule", "A");
        Add("DecimalValue", TestGrammars.DecimalValue, "rule", "A");
        
        Add("CharRange", TestGrammars.CharacterRange, "rule", "A");
        Add("CharRange", TestGrammars.CharacterRange, "rule", "M");
        Add("CharRange", TestGrammars.CharacterRange, "rule", "Z");
        
        Add("CharConcat", TestGrammars.CharacterConcatenation, "rule", "ABC");
    }
}

/// <summary>
/// Test data for invalid grammar validation cases
/// </summary>
public class InvalidInputTestData : TheoryData<string, string, string, string>
{
    public InvalidInputTestData()
    {
        // Grammar Name, Grammar, Rule Name, Input
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "");
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "abc");
        Add("SimpleNumber", TestGrammars.SimpleNumber, "number", "1a");
        
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "abc");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "1+");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "(1+2");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "+1");
        Add("Arithmetic", TestGrammars.Arithmetic, "expr", "1 + 2"); // spaces not allowed
        
        Add("CharRange", TestGrammars.CharacterRange, "rule", "a");
        Add("CharRange", TestGrammars.CharacterRange, "rule", "1");
        
        Add("CharConcat", TestGrammars.CharacterConcatenation, "rule", "AB");
        Add("CharConcat", TestGrammars.CharacterConcatenation, "rule", "ABCD");
    }
}

public class ValidationTests
{
    // private static Bnf.Grammar.Grammar ParseGrammar(string abnf) => Abnf.Parse(abnf);

    [Theory]
    [ClassData(typeof(ValidInputTestData))]
    public void ValidateInput_ShouldSucceed(string grammarName, string abnf, string ruleName, string input)
    {
        var grammar = Abnf.Parse(abnf);
        grammar.AssertValid(ruleName, input);
    }

    [Theory]
    [ClassData(typeof(InvalidInputTestData))]
    public void ValidateInput_ShouldFail(string grammarName, string abnf, string ruleName, string input)
    {
        var grammar = Abnf.Parse(abnf);
        grammar.AssertInvalid(ruleName, input);
    }
}
