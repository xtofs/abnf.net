using Abnf.Parsing;

namespace Abnf.Tests;

public class ConversionDebuggingTests
{
    [Fact]
    public void InspectTermPattern()
    {
        var abnf = @"
term = factor *( ""*"" factor )
factor = number
number = 1*DIGIT
DIGIT = %x30-39
";
        var tokens = Scanner.Scan(abnf);
        var parser = new Parser(tokens);
        var ast = parser.ParseRuleList();
        
        // Inspect the AST for term
        var termRule = ast.Rules.First(r => r.Name == "term");
        var expr = termRule.Expr;
        
        // term should be a Concatenation of [factor, group]
        Assert.IsType<AstNode.Expression.Concatenation>(expr);
        var concat = (AstNode.Expression.Concatenation)expr;
        Assert.Equal(2, concat.Elements.Count);
        
        // First element should be RuleRef to factor
        Assert.IsType<AstNode.Expression.RuleRef>(concat.Elements[0]);
        
        // Dump what the second element actually is for debugging
        var secondElement = concat.Elements[1];
        System.Console.WriteLine($"Second element type: {secondElement.GetType().Name}");
        
        // Let's see what we're actually dealing with
        if (secondElement is AstNode.Expression.Group g)
        {
            System.Console.WriteLine($"  Group inner type: {g.Inner.GetType().Name}");
            if (g.Inner is AstNode.Expression.Concatenation innerConcat)
            {
                System.Console.WriteLine($"  Inner concatenation has {innerConcat.Elements.Count} elements");
                for (int i = 0; i < innerConcat.Elements.Count; i++)
                {
                    System.Console.WriteLine($"    Element {i}: {innerConcat.Elements[i].GetType().Name}");
                }
            }
        }
        
        // For now, just pass the test so we can see the output
        Assert.True(true);
    }
}
