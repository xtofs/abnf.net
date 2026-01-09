using Abnf.Parsing;

namespace Abnf.Tests;

public class ParserAbnfTests
{
    [Fact]
    public void ParseRepetitionOfGroup()
    {
        // Test: rule = *( "a" )
        var abnf = "rule = *(\"a\")";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        var rule = ast.Rules[0];
        Assert.IsType<AstNode.Expression.Repetition>(rule.Expr);
        var rep = (AstNode.Expression.Repetition)rule.Expr;
        Assert.Null(rep.Min);
        Assert.Null(rep.Max);
        
        // The repetition should contain a Group
        Assert.IsType<AstNode.Expression.Group>(rep.Element);
    }

    [Fact]
    public void ParseRepetitionWithSpacing()
    {
        // Test: rule = * ( "a" ) - with spaces
        var abnf = "rule = * ( \"a\" )";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        var rule = ast.Rules[0];
        Assert.IsType<AstNode.Expression.Repetition>(rule.Expr);
    }
}
